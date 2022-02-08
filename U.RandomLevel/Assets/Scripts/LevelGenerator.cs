using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
	public bool showDebug;
	public GameObject[] rooms;
	public float roomWidth;
	public float roomLength;
	public int numRooms;
	private readonly List<RoomPositions> _roomCoordinates = new();
	private readonly List<Vector3> _spawnCoordinates = new();
	private int[,] _data;

	private MazeDataGenerator _dataGenerator;
	private int _maxX;
	private int _maxZ;

	private int _minX;
	private int _minZ;

	private void Awake() => _dataGenerator = new MazeDataGenerator();
	private void Start()
	{
		// add (0, 0) to room coordinates
		var spawn = new RoomPositions(Vector3.zero, 0);
		spawn.Visited = true;
		_roomCoordinates.Add(spawn);

		for (var i = 0; i < numRooms; i++)
		{
			//add spawn coordinates based on room coordinates
			AddNewSpawnCoords(_roomCoordinates.Count - 1, new Vector2(1, 0));
			AddNewSpawnCoords(_roomCoordinates.Count - 1, new Vector2(-1, 0));
			AddNewSpawnCoords(_roomCoordinates.Count - 1, new Vector2(0, 1));
			AddNewSpawnCoords(_roomCoordinates.Count - 1, new Vector2(0, -1));
			//pick one randomly
			var roomLocation = _spawnCoordinates[Random.Range(0, _spawnCoordinates.Count)];
			//pick room randomly
			var room = Random.Range(1, rooms.Length);

			//remove picked coord from spawn coordinates
			bool isFound;
			do
			{
				isFound = _spawnCoordinates.Remove(roomLocation);
			} while (isFound);

			//add it to room coordinates
			var roomPos = new RoomPositions(roomLocation, room);
			_roomCoordinates.Add(roomPos);
		}
		_roomCoordinates.Sort((x, y) =>
			(Mathf.Abs(x.RoomCoord.x) + Mathf.Abs(x.RoomCoord.z)).CompareTo(Mathf.Abs(y.RoomCoord.x) +
																			Mathf.Abs(y.RoomCoord.z)));
		FindArraySize();
		MakeRoomArray();
		ConnectRooms();
		InstantiateRooms();
	}

	private void OnGUI()
	{
		if (!showDebug) return;

		var maze = _data;
		var rMax = maze.GetUpperBound(0);
		var cMax = maze.GetUpperBound(1);

		var message = "";

		for (var i = rMax; i >= 0; i--)
		{
			for (var j = 0; j <= cMax; j++)
			{
				if (maze[i, j] != 0)
					message += "....";
				else
					message += "==";
				//message += data [i, j];
			}
			message += "\n";
		}
		GUI.Label(new Rect(20, 20, 500, 500), message);
	}

	private void ConnectRooms()
	{
		foreach (var r in _roomCoordinates.Where(r => !r.Visited))
		{
			// Calculate the position of the room in the Room Array
			VisitRooms(r);
		}
	}

	private void VisitRooms(RoomPositions r)
	{
		// Make an empty list of visited rooms to be filled each iteration.
		var visitedRooms = new List<RoomPositions>();
		visitedRooms.Add(r);
		
		// Get the row and col equivalent in the level array
		var row = 2 * ((int)r.RoomCoord.z - _minZ) + 1;
		var col = 2 * ((int)r.RoomCoord.x - _minX) + 1;
		
		// Make a list of the next possible positions to visit.
		var nextSpots = new List<Vector2>{ Vector2.left, Vector2.right, Vector2.up, Vector2.down };
		// Make a copy of that list to remove invalid items and improve performance.
		var nextSpotsCopy = nextSpots;
		
		var isVisited = false;
		while (!isVisited)
		{
			var isInBounds = false;
			while (!isInBounds)
			{
				var nextSpot = 2 * nextSpotsCopy[Random.Range(0, nextSpots.Count)];
				if (row + (int)nextSpot.x < 0 || row + (int)nextSpot.x > _data.GetUpperBound(0))
				{
					nextSpotsCopy.Remove(nextSpot);
					continue;
				}

				if (col + (int)nextSpot.y < 0 || col + (int)nextSpot.y > _data.GetUpperBound(1))
				{
					nextSpotsCopy.Remove(nextSpot);
					continue;
				}
				var nextRow = row + (int)nextSpot.x;
				var nextCol = col + (int)nextSpot.y;
				// If we found a room.
				if (_data[nextRow, nextCol] != 0)
				{
					// Open Door
					_data[row + (int)nextSpot.x / 2, col + (int)nextSpot.y / 2] = -1;
					var room = ConvertToRoom(nextRow, nextCol);
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
					nextSpotsCopy.Remove(nextSpot);
				}
			}
		}
		foreach (var v in visitedRooms)
		{
			v.Visited = true;
		}
	}

	private RoomPositions ConvertToRoom(int row, int col)
	{
		var xPos = (float)((col - 1) / 2 + _minX);
		var zPos = (float)((row - 1) / 2 + _minZ);
		var pos = new Vector3(xPos, 0f, zPos);

		var room = _roomCoordinates.Find(x => x.RoomCoord == pos);
		return room;
	}

	private void PrintArray()
	{
		foreach (var t in _roomCoordinates)
		{
			Debug.Log(t.RoomCoord + "\n");
		}
	}

	private void AddNewSpawnCoords(int room, Vector2 mod)
	{
		var coord = new Vector3(_roomCoordinates[room].RoomCoord.x + mod.x, _roomCoordinates[room].RoomCoord.y,
			_roomCoordinates[room].RoomCoord.z + mod.y);
		for (var k = 0; k < _roomCoordinates.Count; k++)
		{
			if (coord == _roomCoordinates[k].RoomCoord)
				return;
			if (k == _roomCoordinates.Count - 1)
				_spawnCoordinates.Add(coord);
		}
	}
	private void FindArraySize()
	{
		foreach (var t in _roomCoordinates)
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

	private void MakeRoomArray()
	{
		_data = new int[2 * (_maxZ - _minZ + 1) + 1, 2 * (_maxX - _minX + 1) + 1];
		for (var i = 0; i < _roomCoordinates.Count; i++)
		{
			var x = 2 * ((int)_roomCoordinates[i].RoomCoord.x - _minX) + 1;
			var z = 2 * ((int)_roomCoordinates[i].RoomCoord.z - _minZ) + 1;
			_data[z, x] = _roomCoordinates[i].RoomID + 1;
		}
	}

	private void InstantiateRooms()
	{
		for (var i = 0; i <= _data.GetUpperBound(0); i++)
		{
			for (var j = 0; j <= _data.GetUpperBound(1); j++)
			{
				if (_data[i, j] <= 0) continue;
				var xPos = ((j - 1) / 2 + _minX) * roomLength;
				var zPos = ((i - 1) / 2 + _minZ) * roomWidth;
				var roomGO = rooms[_data[i, j] - 1];
				var room = Instantiate(roomGO, new Vector3(xPos, 0, zPos), Quaternion.identity);
				OpenDoors(room, i, j);
			}
		}
	}

	private void OpenDoors(GameObject room, int i, int j)
	{
		// Changed "open" areas to be -1, and walls are 0
		var n = _data[i + 1, j] == -1;
		var e = _data[i, j + 1] == -1;
		var s = _data[i - 1, j] == -1;
		var w = _data[i, j - 1] == -1;
		var doors = room.GetComponent<Room>();
		if (n) doors.doorsNESW[0].gameObject.SetActive(false);
		if (e) doors.doorsNESW[1].gameObject.SetActive(false);
		if (s) doors.doorsNESW[2].gameObject.SetActive(false);
		if (w) doors.doorsNESW[3].gameObject.SetActive(false);
	}
}

public class RoomPositions
{
	public Vector3 RoomCoord;
	public int RoomID;
	public bool Visited;

	public RoomPositions(Vector3 coord, int id)
	{
		RoomCoord = coord;
		RoomID = id;
		Visited = false;
	}
}
