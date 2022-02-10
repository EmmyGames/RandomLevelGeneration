using System.Collections.Generic;
using UnityEngine;

public class Pathfind : MonoBehaviour
{
	/// <summary>
	/// Helper method to convert a location in the Level array to its corresponding room object
	/// in the room array.
	/// </summary>

	public Level level;

	private Room FindRoomInLevel(int row, int col)
	{
		var pos = level.fill.ConvertToRoom(row, col);
		var room = level.Rooms.Find(m => m.RoomCoord == pos);
		return room;
	}
	
	public void PathToSpawn(int[,] rooms, Room r)
	{
		// Make an empty list of visited rooms to be filled each iteration.
		var visitedRooms = new List<Room> { r };

		// Get the row and column equivalent in the level array
		var row = 2 * ((int)r.RoomCoord.z - level.minZ) + 1;
		var col = 2 * ((int)r.RoomCoord.x - level.minX) + 1;

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
				if (row + (int)nextSpot.x < 0 || row + (int)nextSpot.x > rooms.GetUpperBound(0))
				{
					nextSpots.Remove(nextSpot);
					continue;
				}
				if (col + (int)nextSpot.y < 0 || col + (int)nextSpot.y > rooms.GetUpperBound(1))
				{
					nextSpots.Remove(nextSpot);
					continue;
				}

				var nextRow = row + (int)nextSpot.x;
				var nextCol = col + (int)nextSpot.y;
				// If we found a room.
				if (rooms[nextRow, nextCol] > 0)
				{
					// Make the space in the middle an open space.
					rooms[row + (int)nextSpot.x / 2, col + (int)nextSpot.y / 2] = -1;
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
}
