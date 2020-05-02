using System.Collections.Generic;
using UnityEngine;

public class MazeCell {
	public bool visited = false;

	public GameObject northWall, southWall, eastWall, westWall, floor;
	public bool northOpen, southOpen, eastOpen, westOpen;


	public bool hasTrap;

	public bool isCross;
	public bool dfsVisited = false;
	public bool inCriticalPath = false;

	public Vector2 savePoint;

	public List<MazeCell> pathTilLastSaveP = new List<MazeCell>();


}
