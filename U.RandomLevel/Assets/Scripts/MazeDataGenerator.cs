using UnityEngine;

/// <summary>
///     Brief summary of what the class does
/// </summary>
public class MazeDataGenerator
{
	public float PlacementThreshold;

	public MazeDataGenerator() => PlacementThreshold = 0.1f;

	public int[,] FromDimensions(int[,] maze)
	{
		var rows = maze.GetUpperBound(0);
		var cols = maze.GetUpperBound(1);

		for (var i = 0; i <= rows; i++)
		{
			for (var j = 0; j <= cols; j++)
			{
				if (i == 0 || j == 0 || i == rows || j == cols) { }
				else if (i % 2 == 0 && j % 2 == 0)
				{
					if (Random.value > PlacementThreshold)
					{
						maze[i, j] = -1;

						var a = Random.value < 0.5f ? 0 : Random.value < 0.5f ? -1 : 1;
						var b = a != 0 ? 0 : Random.value < 0.5f ? -1 : 1;
						maze[i + a, j + b] = -1;
					}
				}
			}
		}

		return maze;
	}
}
