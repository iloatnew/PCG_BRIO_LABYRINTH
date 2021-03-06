﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DFSMazeMutator : MonoBehaviour
{

	private enum Dir { north = 1, south = 2, east = 3, west = 4, none = 0 };
	private MazeCell[,] mazeCells;
	int mazeRows;
	int mazeColumns;
	MazeHelp mazeHelp;

	public DFSMazeMutator(MazeHelp mazeHelp)
	{
		this.mazeCells = mazeHelp.mazeCells;
		this.mazeRows = mazeHelp.mazeRows;
		this.mazeColumns = mazeHelp.mazeColumns;
		this.mazeHelp = mazeHelp;
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
				return;
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
				mazeCells[r, c].southOpen = false;
				mazeCells[r, c].northOpen = false;
				mazeCells[r, c].eastOpen = false;
				mazeCells[r, c].westOpen = false;

				// the northern cell
				if (mazeHelp.CellIsAvailable(r - 1, c, true))
					if(mazeCells[r - 1, c].inCriticalPath && mazeCells[r,c].inCriticalPath)
					{
						mazeCells[r - 1, c].southOpen = true;
						mazeCells[r, c].northOpen = true;
					}
				// the southern cell
				if (mazeHelp.CellIsAvailable(r + 1, c, true))
					if (mazeCells[r + 1, c].inCriticalPath && mazeCells[r, c].inCriticalPath)
					{
						mazeCells[r + 1, c].northOpen = true;
						mazeCells[r, c].southOpen = true;
					}
				// the eastern cell
				if (mazeHelp.CellIsAvailable(r , c + 1 , true))
					if (mazeCells[r, c + 1].inCriticalPath && mazeCells[r, c].inCriticalPath)
					{
						mazeCells[r, c + 1].westOpen = true;
						mazeCells[r, c].eastOpen = true;
					}

				// the western cell
				if (mazeHelp.CellIsAvailable(r, c - 1, true))
					if (mazeCells[r, c - 1].inCriticalPath && mazeCells[r, c].inCriticalPath)
					{
						mazeCells[r, c - 1].eastOpen = true;
						mazeCells[r, c].westOpen = true;
					}
					
			}
		}
		

		//MazeCell goal = mazeCells[mazeRows - 1, mazeColumns - 1];
		//List<MazeCell> criticalPath = new List<MazeCell>();
		//criticalPath = goal.pathTilLastSaveP;

		//while (criticalPath != new List<MazeCell>())
		//{
		//	criticalPath[0].


		//}


	}

}
