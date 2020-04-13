using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

///************************************************************
/// <summary>
/// 1. HuntAndKill
/// 2. DFS
///</summary>
///************************************************************
public class HuntAndKillMutated : MazeAlgorithm
{
	private int currentRow = 0;
	private int currentColumn = 0;

	public bool courseComplete = false;

	private MazeHelp mazeHelp;
	private ProceduralNumberGenerator png;

	public HuntAndKillMutated(MazeCell[,] mazeCells) : base(mazeCells)
	{
		png = new ProceduralNumberGenerator();
	}

	public void SetMazeHelp(int token)
	{
		mazeHelp = new MazeHelp(mazeCells, mazeRows, mazeColumns, token);
	}

	/// <summary>
	/// Create maze with token, token = 0 -> pure random
	///</summary>
	public override void CreateMaze(int token)
	{
		HuntAndKill(token);
		
	}

	private void HuntAndKill(int token)
	{
		mazeCells[currentRow, currentColumn].visited = true;

		while (!courseComplete)
		{
			Kill(token); // Will run until it hits a dead end.
			Hunt(); // Finds the next unvisited cell with an adjacent visited cell. If it can't find any, it sets courseComplete to true.
		}
		
		mazeHelp.dFSMazeMutator.DFS();
		
	}


	private void Kill(int token)
	{
		while (mazeHelp.RouteStillAvailable(currentRow, currentColumn))
		{
			
			int direction = png.GetNextNumber(token);

			if (direction == 1 && mazeHelp.CellIsAvailable(currentRow - 1, currentColumn))
			{
				// North
				mazeCells[currentRow, currentColumn].northOpen = true;
				mazeCells[currentRow - 1, currentColumn].southOpen = true;
				currentRow--;
			}
			else if (direction == 2 && mazeHelp.CellIsAvailable(currentRow + 1, currentColumn))
			{
				// South
				mazeCells[currentRow, currentColumn].southOpen = true;
				mazeCells[currentRow + 1, currentColumn].northOpen = true;
				currentRow++;
			}
			else if (direction == 3 && mazeHelp.CellIsAvailable(currentRow, currentColumn + 1))
			{
				// east
				mazeCells[currentRow, currentColumn].eastOpen = true;
				mazeCells[currentRow, currentColumn + 1].westOpen = true;
				currentColumn++;
			}
			else if (direction == 4 && mazeHelp.CellIsAvailable(currentRow, currentColumn - 1))
			{
				// west
				mazeCells[currentRow, currentColumn].westOpen = true;
				mazeCells[currentRow, currentColumn - 1].eastOpen = true;
				currentColumn--;
			}

			mazeCells[currentRow, currentColumn].visited = true;
		}
	}

	private void Hunt()
	{
		courseComplete = true; // Set it to this, and see if we can prove otherwise below!

		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				if (!mazeCells[r, c].visited && mazeHelp.CellHasAnAdjacentVisitedCell(r, c))
				{
					courseComplete = false; // Yep, we found something so definitely do another Kill cycle.
					currentRow = r;
					currentColumn = c;
					mazeHelp.DestroyAdjacentWall(currentRow, currentColumn);
					mazeCells[currentRow, currentColumn].visited = true;
					return; // Exit the function
				}
			}
		}
	}

	public void CriticalPathOnly()
	{
		this.mazeHelp.dFSMazeMutator.CriticalPathOnly();
	}

	public void DestroyWalls()
	{ 
		this.mazeHelp.DestroyWalls();
	}



}

