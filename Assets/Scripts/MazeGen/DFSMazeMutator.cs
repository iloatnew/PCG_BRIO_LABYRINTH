using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeMutator
{

	private enum Dir { north = 1, south = 2, east = 3, west = 4, none = 0 };
	private MazeCell[,] mazeCells;
	int mazeRows;
	int mazeColumns;
	MazeHelp mazeHelp;
	public List<MazeCell> solutionPath;

	public DFSMazeMutator(MazeHelp mazeHelp)
	{
		this.mazeCells = mazeHelp.mazeCells;
		this.mazeRows = mazeHelp.mazeRows;
		this.mazeColumns = mazeHelp.mazeColumns;
		this.mazeHelp = mazeHelp;
		solutionPath = new List<MazeCell>();
	}

	/// <summary>
	/// DFS to calculate the critical path
	/// </summary>
	public void DFS()
	{
		int currentColumn = 0;
		int currentRow = 0;
		int nextColumn = 0;
		int nextRow = 0;
		Dir fromDirection = Dir.none;
		MazeCell currentCell = mazeCells[currentRow, currentColumn];
		MazeCell lastCell = currentCell;
		bool finish = false;
		int pace = 0;

		var possibleNextPosition = 0;

		while (!finish && pace++ < mazeRows * mazeColumns * 2)
		{
			//Debug.Log("Now At " + "[" + currentRow + "," + currentColumn + "]" + " SavePoint is " + mazeCells[currentRow, currentColumn].savePoint + " Pace: " + pace);

			// note current cell
			currentCell = mazeCells[currentRow, currentColumn];

			// mark it as critical path (might be delete later)
			currentCell.inCriticalPath = true;

			// mark this as DFS visited
			currentCell.dfsVisited = true;

			// if last cell is the save point of this cell, restart path tracking
			if (lastCell == mazeCells[(int)currentCell.savePoint.x, (int)currentCell.savePoint.y])
			{
				currentCell.pathTilLastSaveP.Clear();
				currentCell.pathTilLastSaveP.Add(lastCell);
				currentCell.pathTilLastSaveP.Add(currentCell);
			}
			// if just jumped back, do nothing. Only extend path by a normal walk
			else if (possibleNextPosition != 0)
			{
				// take path from last cell, add this cell in path
				currentCell.pathTilLastSaveP = lastCell.pathTilLastSaveP;
				currentCell.pathTilLastSaveP.Add(currentCell);
			}


			// fin?
			if (currentColumn == mazeColumns - 1 && currentRow == mazeRows - 1)
			{
				finish = true;
				continue;
			}

			// calculate next position
			possibleNextPosition = PossibleNextPosition(currentRow, currentColumn, fromDirection);

			// move to Direction
			switch (possibleNextPosition)
			{
				//north
				case 1:
					nextRow = currentRow - 1;
					fromDirection = Dir.south;
					break;
				//south
				case 2:
					nextRow = currentRow + 1;
					fromDirection = Dir.north;
					break;
				//east
				case 3:
					nextColumn = currentColumn + 1;
					fromDirection = Dir.west;
					break;
				//west
				case 4:
					nextColumn = currentColumn - 1;
					fromDirection = Dir.east;
					break;
				//cant move, preform jump
				case 0:

					//go back to save point
					nextRow = (int)currentCell.savePoint.x;
					nextColumn = (int)currentCell.savePoint.y;
					fromDirection = Dir.none;

					//mark all cells in path as NONE critical
					foreach (MazeCell mc in currentCell.pathTilLastSaveP)
					{
						mc.inCriticalPath = false;
					}

					break;
				default:
					break;
			}

			// if gonna go back, then don't set new savePoint
			if (possibleNextPosition != 0)
			{
				// is corss
				if (currentCell.isCross)
				{
					// save this for next points
					mazeCells[nextRow, nextColumn].savePoint = new Vector2(currentRow, currentColumn);

				}
				// not cross
				else
				{
					// if this is NOT cross, use the last save point
					mazeCells[nextRow, nextColumn].savePoint = currentCell.savePoint;

				}
			}

			// store the last cell
			lastCell = currentCell;

			// Moved
			currentRow = nextRow;
			currentColumn = nextColumn;

		}

		pace = 0;
		
		while (currentCell != mazeCells[0, 0] && pace < 10)
		{
			var partPath = new List<MazeCell>();
			foreach (MazeCell mc in currentCell.pathTilLastSaveP)
				partPath.Add(mc);
			partPath.AddRange(solutionPath);
			solutionPath = partPath;
			currentCell = (MazeCell)currentCell.pathTilLastSaveP[0];
			pace++;
		}

	}

	/// <summary>
	/// Vector(x,y), x: possible direction, y: total number of possible directions
	/// </summary>
	private int PossibleNextPosition(int row, int column, Dir fromDirection)
	{

		var possibleNextPosition = 0;
		var cntOpen = 0;

		//north has open route & not from north
		if (mazeHelp.CellIsAvailable(row - 1, column, true) && fromDirection != Dir.north)
			if (mazeHelp.HasPathToUnvisitedCell(row, column, 1))
			{
				possibleNextPosition = 1;
				cntOpen += 1;
			}

		//south has open route & not from south
		if (mazeHelp.CellIsAvailable(row + 1, column, true))
			if (mazeHelp.HasPathToUnvisitedCell(row, column, 2) && fromDirection != Dir.south)
			{
				possibleNextPosition = 2;
				cntOpen += 1;
			}
		//east has open route & same
		if (mazeHelp.CellIsAvailable(row, column + 1, true))
			if (mazeHelp.HasPathToUnvisitedCell(row, column, 3) && fromDirection != Dir.east)
			{
				possibleNextPosition = 3;
				cntOpen += 1;
			}
		//west has open route
		if (mazeHelp.CellIsAvailable(row, column - 1, true))
			if (mazeHelp.HasPathToUnvisitedCell(row, column, 4) && fromDirection != Dir.west)
			{
				possibleNextPosition = 4;
				cntOpen += 1;
			}

		if (cntOpen > 1)
		{
			mazeCells[row, column].isCross = true;
		}
		return possibleNextPosition;

	}

	/// <summary>
	/// build walls when its not in critical path
	/// </summary>
	public void CriticalPathOnly()
	{
		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				
				// the northern cell exists 
				if (mazeHelp.CellIsAvailable(r - 1, c, true))
				{
					var criticCheck = (mazeCells[r - 1, c].inCriticalPath && mazeCells[r, c].inCriticalPath);
					var wallCheck = (mazeCells[r, c].northOpen == false || mazeCells[r - 1, c].southOpen == false);
					// not both are in critical path or there's wall in between
					if (!criticCheck || wallCheck)
					{
						// then build a wall
						mazeCells[r - 1, c].southOpen = false;
						mazeCells[r, c].northOpen = false;
						if(mazeCells[r - 1, c].southWall!=null)
							mazeCells[r - 1, c].NonDfsWalls.Add(mazeCells[r - 1, c].southWall.transform);
					}
					if (criticCheck && wallCheck)
					{
						//Debug.Log((r - 1) + " " + c + " has speicial south wall");
						if (mazeCells[r - 1, c].southWall != null)
							mazeCells[r - 1, c].DfsWalls.Add(mazeCells[r - 1, c].southWall.transform);
					}
				}

				// the southern cell
				if (mazeHelp.CellIsAvailable(r + 1, c, true))
				{
					var criticCheck = (mazeCells[r + 1, c].inCriticalPath && mazeCells[r, c].inCriticalPath);
					var wallCheck = (mazeCells[r, c].southOpen == false || mazeCells[r + 1, c].northOpen == false);
					if (!criticCheck || wallCheck)
					{
						mazeCells[r + 1, c].northOpen = false;
						mazeCells[r, c].southOpen = false;
						if (mazeCells[r, c].southWall != null)
							mazeCells[r, c].NonDfsWalls.Add(mazeCells[r, c].southWall.transform);
					}
					if (criticCheck && wallCheck)
					{
						//Debug.Log(r + " " + c + " has speicial south wall");
						if (mazeCells[r, c].southWall != null)
							mazeCells[r, c].DfsWalls.Add(mazeCells[r, c].southWall.transform);
					}
				}
				// the eastern cell
				if (mazeHelp.CellIsAvailable(r, c + 1, true))
				{
					var criticCheck = (mazeCells[r, c + 1].inCriticalPath && mazeCells[r, c].inCriticalPath);
					var wallCheck = (mazeCells[r, c + 1].westOpen == false || mazeCells[r, c].eastOpen == false);

					if (!criticCheck || wallCheck)
					{
						mazeCells[r, c + 1].westOpen = false;
						mazeCells[r, c].eastOpen = false;
						if(mazeCells[r, c].eastWall!=null)
							mazeCells[r, c].NonDfsWalls.Add(mazeCells[r, c].eastWall.transform);
					}
					if (criticCheck && wallCheck)
					{
						//Debug.Log(r + " " + c + " has speicial east wall");
						if (mazeCells[r, c].eastWall != null)
							mazeCells[r, c].DfsWalls.Add(mazeCells[r, c].eastWall.transform);
					}
				}

				// the western cell
				if (mazeHelp.CellIsAvailable(r, c - 1, true))
				{
					var criticCheck = (mazeCells[r, c - 1].inCriticalPath && mazeCells[r, c].inCriticalPath);
					var wallCheck = (mazeCells[r, c - 1].eastOpen == false || mazeCells[r, c].westOpen == false);

					if (!criticCheck || wallCheck)
					{
						mazeCells[r, c - 1].eastOpen = false;
						mazeCells[r, c].westOpen = false;
						if (mazeCells[r, c - 1].eastWall != null)
							mazeCells[r, c - 1].NonDfsWalls.Add(mazeCells[r, c - 1].eastWall.transform);
					}
					if (criticCheck && wallCheck)
					{
						//Debug.Log(r + " " + (c -1)  + " has speicial east wall");
						if (mazeCells[r, c - 1].eastWall != null)
							mazeCells[r, c - 1].DfsWalls.Add(mazeCells[r, c - 1].eastWall.transform);
					}
				}
				
				
			}
		}
	}




}
