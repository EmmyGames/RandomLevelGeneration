using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates a procedurally generated roguelike-dungeon made from various room prefabs.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
	[Tooltip("Number of rooms that need to be spawned, not counting the spawn room."), Range(0, 200)] 
	public int numRooms;
	[Tooltip("List of room prefabs. Put the spawn room first in the list and add at least one other room.")]
	public GameObject[] rooms;
	[Tooltip("The width of the room prefabs."), Range(5, 200)]
	public float roomWidth;
	[Tooltip("The length of the room prefabs."), Range(5, 200)]
	public float roomLength;

	private readonly List<Room> _roomPositions = new();
	private readonly List<Vector3> _nextLocationCandidates = new();
	private int[,] _level;
	private int _maxX, _maxZ, _minX, _minZ;

	private void Start()
	{
		if (rooms.Length == 0)
		{
			Debug.LogWarning("Please add prefabs to the rooms list in LevelGenerator");
			return;
		}
		GenerateLevel();
	}
	
	/// <summary>
	/// Adds the spawn locations adjacent to the most recently added room position.
	/// </summary>
	private void AddCandidates()
	{
		var mods = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
		for (var i = 0; i < mods.Count; i++)
		{
			var coord = new Vector3(_roomPositions[^1].RoomCoord.x + mods[i].x, _roomPositions[^1].RoomCoord.y,
				_roomPositions[^1].RoomCoord.z + mods[i].y);
			for (var j = 0; j < _roomPositions.Count; j++)
			{
				if (coord == _roomPositions[j].RoomCoord)
					break;
				if (j == _roomPositions.Count - 1)
					_nextLocationCandidates.Add(coord);
			}
		}
	}
	
	/// <summary>
	/// Finds the path to spawn for every room in the room in the level.
	/// </summary>
	private void ConnectRooms()
	{
		foreach (var room in _roomPositions.Where(r => !r.Visited))
		{
			// Calculate the position of the room in the Room Array
			PathToSpawn(room);
		}
	}
	
	/// <summary>
	/// Helper method to convert a location in the Level array to its corresponding world position.
	/// This does not take into account room length and width until initialization.
	/// </summary>
	private Vector3 ConvertToRoom(int row, int col)
	{
		var xPos = (float)((col - 1) / 2 + _minX);
		var zPos = (float)((row - 1) / 2 + _minZ);
		var position = new Vector3(xPos, 0f, zPos);
		return position;
	}
	
	/// <summary>
	/// Finds the bounds of the level by determining the minimum and maximum x and z coordinates.
	/// </summary>
	private void FindLevelBounds()
	{
		foreach (var t in _roomPositions)
		{
			if (t.RoomCoord.x < _minX)
				_minX = (int)t.RoomCoord.x;
			if (t.RoomCoord.x > _maxX)
				_maxX = (int)t.RoomCoord.x;
			if (t.RoomCoord.z < _minZ)
				_minZ = (int)t.RoomCoord.z;
			if (t.RoomCoord.z > _maxZ)
				_maxZ = (int)t.RoomCoord.z;
		}
	}
	
	/// <summary>
	/// Helper method to convert a location in the Level array to its corresponding room object
	/// in the room array.
	/// </summary>

	private Room FindRoomInLevel(int row, int col)
	{
		var pos = ConvertToRoom(row, col);
		var room = _roomPositions.Find(m => m.RoomCoord == pos);
		return room;
	}

	/// <summary>
	/// Generates a procedurally generated roguelike-dungeon made from various room prefabs.
	/// </summary>
	private void GenerateLevel()
	{
		PopulateRoomList();
		FindLevelBounds();
		MakeLevelArray();
		ConnectRooms();
		InstantiateRooms();
	}
	
	/// <summary>
	/// Instantiates the rooms based on the _level array.
	/// </summary>
	private void InstantiateRooms()
	{
		var parent = new GameObject("Rooms");
		for (var i = 0; i <= _level.GetUpperBound(0); i++)
		{
			for (var j = 0; j <= _level.GetUpperBound(1); j++)
			{
				if (_level[i, j] <= 0) continue;
				var roomGO = rooms[_level[i, j] - 1];
				var pos = ConvertToRoom(i, j);
				pos.x *= roomLength;
				pos.z *= roomWidth;
				var room = Instantiate(roomGO, pos, Quaternion.identity, parent.transform);
				OpenDoors(room, i, j);
			}
		}
	}

	/// <summary>
	/// The _level array is an int[,] that places all the rooms from _roomPositions. The room value represents their
	/// index in the rooms array to keep track of its prefab. Spaces are left in between rooms to keep track of walls
	/// or open space between rooms. 0s are the default and are walls. Some of these change to -1s (spaces) during
	/// pathfinding to spawn.
	/// </summary>
	private void MakeLevelArray()
	{
		_level = new int[2 * (_maxZ - _minZ + 1) + 1, 2 * (_maxX - _minX + 1) + 1];
		foreach (var t in _roomPositions)
		{
			var x = 2 * ((int)t.RoomCoord.x - _minX) + 1;
			var z = 2 * ((int)t.RoomCoord.z - _minZ) + 1;
			_level[z, x] = t.RoomID + 1;
		}
	}
	
	/// <summary>
	/// Checks adjacent indexes in the _level array to determine if there should be walls or doorways.
	/// </summary>
	private void OpenDoors(GameObject room, int i, int j)
	{
		var n = _level[i + 1, j] == -1;
		var e = _level[i, j + 1] == -1;
		var s = _level[i - 1, j] == -1;
		var w = _level[i, j - 1] == -1;
		var doors = room.GetComponent<Doors>();
		if (n) doors.doorsNESW[0].gameObject.SetActive(false);
		if (e) doors.doorsNESW[1].gameObject.SetActive(false);
		if (s) doors.doorsNESW[2].gameObject.SetActive(false);
		if (w) doors.doorsNESW[3].gameObject.SetActive(false);
	}
	
	/// <summary>
	/// Makes a path from r to spawn by visiting adjacent rooms until it encounters a visited room. It then
	/// changes all the rooms that were moved through to be visited as well.
	/// </summary>
	private void PathToSpawn(Room r)
	{
		// Make an empty list of visited rooms to be filled each iteration.
		var visitedRooms = new List<Room> { r };

		// Get the row and column equivalent in the level array
		var row = 2 * ((int)r.RoomCoord.z - _minZ) + 1;
		var col = 2 * ((int)r.RoomCoord.x - _minX) + 1;
		
		var isVisited = false;
		while (!isVisited)
		{
			// Make a list of the next possible positions to visit.
			var nextSpots = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
			var isInBounds = false;
			while (!isInBounds)
			{
				var nextSpot = 2 * nextSpots[Random.Range(0, nextSpots.Count)];
				
				// If the next spot is out of bounds, it is not a valid spot.
				if (row + (int)nextSpot.x < 0 || row + (int)nextSpot.x > _level.GetUpperBound(0))
				{
					nextSpots.Remove(nextSpot);
					continue;
				}
				if (col + (int)nextSpot.y < 0 || col + (int)nextSpot.y > _level.GetUpperBound(1))
				{
					nextSpots.Remove(nextSpot);
					continue;
				}
				
				var nextRow = row + (int)nextSpot.x;
				var nextCol = col + (int)nextSpot.y;
				// If we found a room.
				if (_level[nextRow, nextCol] > 0)
				{
					// Make the space in the middle an open space.
					_level[row + (int)nextSpot.x / 2, col + (int)nextSpot.y / 2] = -1;
					var room = FindRoomInLevel(nextRow, nextCol);
					// If the room hasn't already been visited this iteration, add it to the list.
					if (visitedRooms.Find(x => x.RoomCoord == room.RoomCoord) == null) visitedRooms.Add(room);
					// Update the selected row and column.
					row = nextRow;
					col = nextCol;
					isInBounds = true;
					// If we found a visited room, we move onto the next closest room to spawn.
					if (room.Visited)
						isVisited = true;
				}
				// If we didn't find a room, it is not a valid spot
				else
				{
					nextSpots.Remove(nextSpot);
				}
			}
		}
		foreach (var v in visitedRooms)
		{
			v.Visited = true;
		}
	}
	
	/// <summary>
	/// Determines the layout of rooms and adds them to an array to be used later.
	/// </summary>
	private void PopulateRoomList()
	{
		// Add the spawn room.
		var spawn = new Room(Vector3.zero, 0)
		{
			Visited = true
		};
		_roomPositions.Add(spawn);

		for (var i = 0; i < numRooms; i++)
		{
			// Add next location candidates based on room coordinates.
			AddCandidates();
			// Pick a spawn location.
			var roomLocation = _nextLocationCandidates[Random.Range(0, _nextLocationCandidates.Count)];
			// Pick a room.
			var room = Random.Range(1, rooms.Length);
			if (rooms.Length == 1)
				room = 0;

			// Remove picked coord from spawn coordinates and account for multiple entries.
			bool isFound;
			do
			{
				isFound = _nextLocationCandidates.Remove(roomLocation);
			} while (isFound);

			//add it to room coordinates
			var roomPos = new Room(roomLocation, room);
			_roomPositions.Add(roomPos);
		}
		// Sort rooms by distance from the spawn room ascending.
		_roomPositions.Sort((x, y) =>
			(Mathf.Abs(x.RoomCoord.x) + Mathf.Abs(x.RoomCoord.z)).CompareTo(Mathf.Abs(y.RoomCoord.x) +
																			Mathf.Abs(y.RoomCoord.z)));
	}
}
