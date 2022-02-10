using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates a procedurally generated roguelike-dungeon made from various room prefabs.
/// </summary>
public class Level : MonoBehaviour
{
	[Tooltip("Number of rooms that need to be spawned, not counting the spawn room."), Range(0, 200)] 
	public int numRooms;
	[Tooltip("List of room prefabs. Put the spawn room first in the list and add at least one other room.")]
	public GameObject[] roomPrefabs;
	[Tooltip("The width of the room prefabs."), Range(5, 200)]
	public float roomWidth;
	[Tooltip("The length of the room prefabs."), Range(5, 200)]
	public float roomLength;

	[HideInInspector]
	public readonly List<Room> Rooms = new List<Room>();
	private readonly List<Vector3> _nextLocationCandidates = new();
	private int[,] _levelLayout;
	[HideInInspector]
	public int maxX, maxZ, minX, minZ;

	public LevelGeneration generation;
	public FillList fill;
	public Pathfind path;

	private void Start()
	{
		if (roomPrefabs.Length == 0)
		{
			Debug.LogWarning("Please add prefabs to the rooms list in LevelGenerator");
			return;
		}
		GenerateLevel();
	}

	/// <summary>
	/// Finds the path to spawn for every room in the room in the level.
	/// </summary>
	private void ConnectRooms()
	{
		foreach (var room in Rooms.Where(r => !r.Visited))
		{
			// Calculate the position of the room in the Room Array
			path.PathToSpawn(_levelLayout, room);
		}
	}

	/// <summary>
	/// Finds the bounds of the level by determining the minimum and maximum x and z coordinates.
	/// </summary>
	private void FindLevelBounds()
	{
		foreach (var t in Rooms)
		{
			if (t.RoomCoord.x < minX)
				minX = (int)t.RoomCoord.x;
			if (t.RoomCoord.x > maxX)
				maxX = (int)t.RoomCoord.x;
			if (t.RoomCoord.z < minZ)
				minZ = (int)t.RoomCoord.z;
			if (t.RoomCoord.z > maxZ)
				maxZ = (int)t.RoomCoord.z;
		}
	}

	/// <summary>
	/// Generates a procedurally generated roguelike-dungeon made from various room prefabs.
	/// </summary>
	private void GenerateLevel()
	{
		fill.PopulateRoomList(_nextLocationCandidates);
		FindLevelBounds();
		fill.MakeLevelArray(_levelLayout);
		ConnectRooms();
		generation.InstantiateRooms(_levelLayout, minX, minZ);
	}
}
