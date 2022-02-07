using System.Collections.Generic;
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

	private MazeDataGenerator _dataGenerator;
	private int[,] _data;
	private int _maxX;
	private int _maxZ;

	private int _minX;
	private int _minZ;

	private void Awake() => _dataGenerator = new MazeDataGenerator();
	private void Start()
	{
		//spawn spawn room
		//var spawnRoom = Instantiate(rooms[0], Vector3.zero, Quaternion.identity);
		// add (0, 0) to room coordinates
		var spawn = new RoomPositions(Vector3.zero, 0);
		_roomCoordinates.Add(spawn);
		//for loop
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
			//spawn that room in the correct position based on coordinates and length/width

			//spawnRoom = Instantiate(room, new Vector3(roomLocation.x * roomLength, roomLocation.y, roomLocation.z * roomWidth), Quaternion.identity);

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
		FindArraySize();
		MakeRoomArray();

		_data = _dataGenerator.FromDimensions(_data);
		AddPerimeter();
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
				if (maze[i, j] != -1)
					message += "....";
				else
					message += "==";
				//message += data [i, j];
			}
			message += "\n";
		}

		GUI.Label(new Rect(20, 20, 500, 500), message);

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
		//data = new int[2 * (maxX - minX + 1) + 1, 2 * (maxZ - minZ + 1) + 1];
		for (var i = 0; i < _roomCoordinates.Count; i++)
		{
			var x = 2 * ((int)_roomCoordinates[i].RoomCoord.x - _minX) + 1;
			var z = 2 * ((int)_roomCoordinates[i].RoomCoord.z - _minZ) + 1;
			_data[z, x] = _roomCoordinates[i].RoomID + 1;
			//data [x, z] = _roomCoordinates [i].RoomID + 1;
		}
	}

	private void AddPerimeter()
	{
		for (var i = 0; i <= _data.GetUpperBound(0); i++)
		{
			for (var j = 0; j <= _data.GetUpperBound(1); j++)
			{
				// If the value is already a room, it can't be a wall
				if (_data[i, j] > 0)
					continue;
				if (i == 0)
				{
					if (_data[i + 1, j] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (j != _data.GetUpperBound(1) && _data[i + 1, j + 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (j != 0 && _data[i + 1, j - 1] > 0)
						_data[i, j] = -1;
				}
				else if (i == _data.GetUpperBound(0))
				{
					if (_data[i - 1, j] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (j != _data.GetUpperBound(1) && _data[i - 1, j + 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (j != 0 && _data[i - 1, j - 1] > 0)
						_data[i, j] = -1;
				}
				else if (j == 0)
				{
					if (_data[i, j + 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (_data[i - 1, j + 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (_data[i + 1, j + 1] > 0) _data[i, j] = -1;
				}
				else if (j == _data.GetUpperBound(1))
				{
					if (_data[i, j - 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (_data[i - 1, j - 1] > 0)
					{
						_data[i, j] = -1;
						continue;
					}
					if (_data[i + 1, j - 1] > 0) _data[i, j] = -1;
				}
				else
				{
					var n = _data[i + 1, j] > 0;
					var ne = _data[i + 1, j + 1] > 0;
					var e = _data[i, j + 1] > 0;
					var se = _data[i - 1, j + 1] > 0;
					var s = _data[i - 1, j] > 0;
					var sw = _data[i - 1, j - 1] > 0;
					var w = _data[i, j - 1] > 0;
					var nw = _data[i + 1, j - 1] > 0;

					if (n != s)
						_data[i, j] = -1;
					if (w != e)
						_data[i, j] = -1;
					if (ne != sw)
						_data[i, j] = -1;
					if (nw != se)
						_data[i, j] = -1;
				}
			}
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
		var n = _data[i + 1, j] == 0;
		var e = _data[i, j + 1] == 0;
		var s = _data[i - 1, j] == 0;
		var w = _data[i, j - 1] == 0;
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

	public RoomPositions(Vector3 coord, int id)
	{
		RoomCoord = coord;
		RoomID = id;
	}
}
