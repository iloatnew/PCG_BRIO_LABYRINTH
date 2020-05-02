using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeHelp 
{
	public MazeCell[,] mazeCells;
	public int mazeRows;
	public int mazeColumns;
	public int token;
	public DFSMazeMutator dFSMazeMutator;
	private ProceduralNumberGenerator png;

	public MazeHelp(MazeCell[,] mazeCells, int mazeRows, int mazeColumns, int token) 
	{
		this.mazeCells = mazeCells;
		this.mazeRows = mazeRows;
		this.mazeColumns = mazeColumns;
		this.token = token;
		dFSMazeMutator = new DFSMazeMutator(this);
		png = new ProceduralNumberGenerator();
	}


	public bool RouteStillAvailable(int row, int column)
	{
		int availableRoutes = 0;

		if (row > 0 && !mazeCells[row - 1, column].visited)
		{
			availableRoutes++;
		}

		if (row < mazeRows - 1 && !mazeCells[row + 1, column].visited)
		{
			availableRoutes++;
		}

		if (column > 0 && !mazeCells[row, column - 1].visited)
		{
			availableRoutes++;
		}

		if (column < mazeColumns - 1 && !mazeCells[row, column + 1].visited)
		{
			availableRoutes++;
		}

		return availableRoutes > 0;
	}


	/// <summary>
	/// Cell has an adjacent unvisited cell.
	/// </summary>
	public bool CellIsAvailable(int row, int column, bool dfs = false)
	{
		if (dfs)
		{
			if (row >= 0 && row < mazeRows && column >= 0 && column < mazeColumns )
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else 
		{
			if (row >= 0 && row < mazeRows && column >= 0 && column < mazeColumns && !mazeCells[row, column].visited)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		
	}

	/// <summary>
	/// Cell has an adjacent visited cell
	/// </summary>
	public bool CellHasAnAdjacentVisitedCell(int row, int column)
	{
		int visitedCells = 0;

		// Look 1 row up (north) if we're on row 1 or greater
		if (row > 0 && mazeCells[row - 1, column].visited)
		{
			visitedCells++;
		}

		// Look one row down (south) if we're the second-to-last row (or less)
		if (row < (mazeRows - 2) && mazeCells[row + 1, column].visited)
		{
			visitedCells++;
		}

		// Look one row left (west) if we're column 1 or greater
		if (column > 0 && mazeCells[row, column - 1].visited)
		{
			visitedCells++;
		}

		// Look one row right (east) if we're the second-to-last column (or less)
		if (column < (mazeColumns - 2) && mazeCells[row, column + 1].visited)
		{
			visitedCells++;
		}

		// return true if there are any adjacent visited cells to this one
		return visitedCells > 0;
	}

	/// <summary>
	/// Destroy adjacent wall randomly
	/// </summary>
	public void DestroyAdjacentWall(int row, int column)
	{
		bool wallDestroyed = false;

		while (!wallDestroyed)
		{
			int direction = 1;

			if (token == 0)
				direction = ProceduralNumberGenerator.GetRandomNumber();
			else
				direction = png.GetNextNumber(token);

			if (direction == 1 && row > 0 && mazeCells[row - 1, column].visited)
			{
				mazeCells[row, column].northOpen = true;
				mazeCells[row - 1, column].southOpen = true;
				wallDestroyed = true;
			}
			else if (direction == 2 && row < (mazeRows - 2) && mazeCells[row + 1, column].visited)
			{
				mazeCells[row, column].southOpen = true;
				mazeCells[row + 1, column].northOpen = true;
				wallDestroyed = true;
			}
			else if (direction == 3 && column > 0 && mazeCells[row, column - 1].visited)
			{
				mazeCells[row, column].westOpen = true;
				mazeCells[row, column - 1].eastOpen = true;
				wallDestroyed = true;
			}
			else if (direction == 4 && column < (mazeColumns - 2) && mazeCells[row, column + 1].visited)
			{
				mazeCells[row, column].eastOpen = true;
				mazeCells[row, column + 1].westOpen = true;
				wallDestroyed = true;
			}
		}

	}


	public void DestroyWalls( )
	{
		foreach (MazeCell mc in mazeCells)
		{
			if (mc.eastOpen)
			{
				GameObject.Destroy(mc.eastWall);
			}
			if (mc.westOpen)
			{
				GameObject.Destroy(mc.westWall);
			}
			if (mc.northOpen)
			{
				GameObject.Destroy(mc.northWall);
			}

			if (mc.southOpen)
			{
				GameObject.Destroy(mc.southWall);
			}
		}
	}

	/// <summary>
	/// Wall in this direction does not exist
	/// </summary>
	public bool HasPathToUnvisitedCell(int row, int column, int direction )
	{
		switch (direction)
		{
			//north
			case 1:
				return (mazeCells[row, column].northOpen == true) 
					&& (mazeCells[row - 1, column].southOpen == true)
					&& !mazeCells[row - 1, column].dfsVisited;
			//south
			case 2:
				return (mazeCells[row, column].southOpen == true) 
					&& (mazeCells[row + 1, column].northOpen == true)
					&& !mazeCells[row + 1, column].dfsVisited;
			//east
			case 3:
				return (mazeCells[row, column].eastOpen == true)
					&& (mazeCells[row, column + 1].westOpen == true)
					&& !mazeCells[row, column + 1].dfsVisited;
			//west
			case 4:
				return (mazeCells[row, column].westOpen == true) 
					&& (mazeCells[row, column - 1].eastOpen == true)
					&& !mazeCells[row, column - 1].dfsVisited;
			default:
				return false;
		}
	}
}
