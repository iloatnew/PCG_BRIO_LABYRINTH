using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MazeCellSerial
{
	public MazeCellSerial()
	{ }

	public MazeCellSerial(bool visited, bool northOpen, bool southOpen, bool eastOpen, bool westOpen, bool hasTrap, bool isCross, bool dfsVisited, bool inCriticalPath)
	{
		this.visited = visited;
		this.northOpen = northOpen;
		this.southOpen = southOpen;
		this.eastOpen = eastOpen;
		this.westOpen = westOpen;
		this.hasTrap = hasTrap;
		this.isCross = isCross;
		this.dfsVisited = dfsVisited;
		this.inCriticalPath = inCriticalPath;
	}

	public bool visited = false;

	public bool northOpen, southOpen, eastOpen, westOpen;


	public bool hasTrap;

	public bool isCross;
	public bool dfsVisited = false;
	public bool inCriticalPath = false;


}

public class MazeCell : MazeCellSerial
{

	public GameObject northWall, southWall, eastWall, westWall, floor;
	
	public Vector2 savePoint;

	public List<MazeCell> pathTilLastSaveP = new List<MazeCell>();

	public MazeCellSerial GetMazeCellSerial()
	{
		return new MazeCellSerial(visited, northOpen, southOpen, eastOpen, westOpen, hasTrap, isCross, dfsVisited, inCriticalPath);
	}

}
