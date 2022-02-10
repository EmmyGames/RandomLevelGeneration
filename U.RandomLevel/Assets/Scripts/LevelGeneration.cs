using UnityEngine;

public class LevelGeneration : MonoBehaviour
{
	public Level level;

    /// <summary>
    ///     Instantiates the rooms based on the _level array.
    /// </summary>
    public void InstantiateRooms(int[,] levelLayout, int minX, int minZ)
	{
		var parent = new GameObject("Rooms");
		for (var i = 0; i <= levelLayout.GetUpperBound(0); i++)
		{
			for (var j = 0; j <= levelLayout.GetUpperBound(1); j++)
			{
				if (levelLayout[i, j] <= 0) continue;
				var roomGO = level.roomPrefabs[levelLayout[i, j] - 1];
				var pos = level.fill.ConvertToRoom(i, j);
				pos.x *= level.roomLength;
				pos.z *= level.roomWidth;
				var room = Instantiate(roomGO, pos, Quaternion.identity, parent.transform);
				DoorController.OpenDoors(levelLayout, room, i, j);
			}
		}
	}
}
