using UnityEngine;

public class Room
{
	public readonly int RoomID;
	public Vector3 RoomCoord;
	public bool Visited;

	public Room(Vector3 coord, int id)
	{
		RoomCoord = coord;
		RoomID = id;
		Visited = false;
	}
}
