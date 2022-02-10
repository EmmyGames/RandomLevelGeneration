using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FillList : MonoBehaviour
{
	public Level level;

	/// <summary>
	///     Adds the spawn locations adjacent to the most recently added room position.
	/// </summary>
	private static void AddCandidates(List<Room> roomPositions, List<Vector3> candidates)
	{
		var mods = new List<Vector2> { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
		for (var i = 0; i < mods.Count; i++)
		{
			var coord = new Vector3(roomPositions[^1].RoomCoord.x + mods[i].x, roomPositions[^1].RoomCoord.y,
				roomPositions[^1].RoomCoord.z + mods[i].y);
			candidates.AddRange(roomPositions.TakeWhile(t => coord != t.RoomCoord)
											.Where((t, j) => j == roomPositions.Count - 1).Select(t => coord));
		}
	}

	/// <summary>
	///     Determines the layout of rooms and adds them to an array to be used later.
	/// </summary>
	public void PopulateRoomList(List<Vector3> candidates)
	{
		// Add the spawn room.
		var spawn = new Room(Vector3.zero, 0)
		{
			Visited = true
		};
		level.Rooms.Add(spawn);

		for (var i = 0; i < level.numRooms; i++)
		{
			// Add next location candidates based on room coordinates.
			AddCandidates(level.Rooms, candidates);
			// Pick a spawn location.
			var roomLocation = candidates[Random.Range(0, candidates.Count)];
			// Pick a room.
			var room = Random.Range(1, level.roomPrefabs.Length);
			if (level.roomPrefabs.Length == 1)
				room = 0;

			// Remove picked coord from spawn coordinates and account for multiple entries.
			bool isFound;
			do
			{
				isFound = candidates.Remove(roomLocation);
			} while (isFound);

			//add it to room coordinates
			var roomPos = new Room(roomLocation, room);
			level.Rooms.Add(roomPos);
		}
		// Sort rooms by distance from the spawn room ascending.
		level.Rooms.Sort((x, y) =>
			(Mathf.Abs(x.RoomCoord.x) + Mathf.Abs(x.RoomCoord.z)).CompareTo(Mathf.Abs(y.RoomCoord.x) +
																			Mathf.Abs(y.RoomCoord.z)));
	}

	/// <summary>
	///     Helper method to convert a location in the Level array to its corresponding world position.
	///     This does not take into account room length and width until initialization.
	/// </summary>
	public Vector3 ConvertToRoom(int row, int col)
	{
		var xPos = (float)((col - 1) / 2 + level.minX);
		var zPos = (float)((row - 1) / 2 + level.minZ);
		var position = new Vector3(xPos, 0f, zPos);
		return position;
	}
	
	/// <summary>
	/// The _level array is an int[,] that places all the rooms from _roomPositions. The room value represents their
	/// index in the rooms array to keep track of its prefab. Spaces are left in between rooms to keep track of walls
	/// or open space between rooms. 0s are the default and are walls. Some of these change to -1s (spaces) during
	/// pathfinding to spawn.
	/// </summary>
	public void MakeLevelArray(int[,] levelLayout)
	{
		levelLayout = new int[2 * (level.maxZ - level.minZ + 1) + 1, 2 * (level.maxX - level.minX + 1) + 1];
		foreach (var t in level.Rooms)
		{
			var x = 2 * ((int)t.RoomCoord.x - level.minX) + 1;
			var z = 2 * ((int)t.RoomCoord.z - level.minZ) + 1;
			levelLayout[z, x] = t.RoomID + 1;
		}
	}
}
