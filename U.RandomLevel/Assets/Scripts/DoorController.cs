using UnityEngine;

public static class DoorController
{
    /// <summary>
    ///     Checks adjacent indexes in the _level array to determine if there should be walls or doorways.
    /// </summary>
    public static void OpenDoors(int[,] level, GameObject room, int i, int j)
	{
		var n = level[i + 1, j] == -1;
		var e = level[i, j + 1] == -1;
		var s = level[i - 1, j] == -1;
		var w = level[i, j - 1] == -1;
		var doors = room.GetComponent<Doors>();
		if (n) doors.doorsNESW[0].gameObject.SetActive(false);
		if (e) doors.doorsNESW[1].gameObject.SetActive(false);
		if (s) doors.doorsNESW[2].gameObject.SetActive(false);
		if (w) doors.doorsNESW[3].gameObject.SetActive(false);
	}
}
