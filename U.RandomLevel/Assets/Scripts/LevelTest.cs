using System.Collections.Generic;
using UnityEngine;

public class LevelTest : MonoBehaviour
{
	[Tooltip("Number of rooms that need to be spawned, not counting the spawn room."), Range(0, 200)]
	public int numRooms;
	[Tooltip("List of room prefabs. Put the spawn room first in the list and add at least one other room.")]
	public GameObject[] rooms;
	[Tooltip("The width of the room prefabs."), Range(5, 200)]
	public float roomWidth;
	[Tooltip("The length of the room prefabs."), Range(5, 200)]
	public float roomLength;
	private readonly List<Vector3> _nextLocationCandidates = new();

	private readonly List<Room> _roomPositions = new();
	private int[,] _level;
	private int _maxX, _maxZ, _minX, _minZ;

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
}
