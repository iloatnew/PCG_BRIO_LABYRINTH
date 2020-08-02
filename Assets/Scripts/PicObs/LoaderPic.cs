using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;

///************************************************************
/// <summary>
/// 1. Initialize Maze 
/// 2. CreateMaze (External Call <code> HuntAndKillMazeAlgorithm </code>) 
/// 3. Dig Hole
/// 4. Start and End
///</summary>
///************************************************************
public class LoaderPic : MonoBehaviour
{
	public int mazeRows, mazeColumns;
	public GameObject ball;
	public GameObject mazeParent;
	public GameObject wall;
	public GameObject floor;
	public GameObject trap;
	public GameObject goal;
	public bool criticalOnly;

	[Range(0, 1f)]
	public float difficulty;
	[Range(0, 10)]
	public int token;

	private MazeCell[,] mazeCells;
	private GameObject player;
	private float size = 6f;
	private Vector3 shifting;
	private float maxScale;

	public bool restart;

	Vector3 fixPosition;
	HuntAndKillMutated ma;
	List<Transform> DfsWalls = new List<Transform>();
	List<Transform> NonDfsWalls = new List<Transform>();
	GameObject dummy;


	#region Restart Methods
	public void Restart()
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);

		GameObject.Destroy(player);
		player = Instantiate(ball, transform.TransformPoint(new Vector3(0 * size, -(size / 2f) + 1f, 0 * size) + shifting), Quaternion.identity, mazeParent.transform);
		player.transform.parent = mazeParent.transform;
	}
	#endregion

	#region initialize method
	/// <summary>
	/// Use Awake() to make it called before Start from other method. Ensure all GameObjects are availible for others
	/// </summary>
	void Awake()
	{
		fixPosition = transform.position;
		dummy = new GameObject();

		var halfLengthRow = (mazeRows - 1) * GetSize() / 2;
		var halfLengthColumn = (mazeColumns - 1) * GetSize() / 2;
		shifting = new Vector3(-halfLengthRow, 0, -halfLengthColumn);

		InitializeMaze();

		ma = new HuntAndKillMutated(mazeCells);
		ma.SetMazeHelp(token);
		ma.CreateMaze(token);

		if (criticalOnly)
		{
			ma.CriticalPathOnly();
		}

		//no neeed to really destroy Gameobj
		ma.DestroyWalls();

		DigHole(token); // Dig Holes accroding to difficulty.

		InitializeBall();

		

	}

	/// <summary>
	/// Use Start() to active gravity for the ball again.
	/// </summary>
	void Start()
	{
		player.GetComponent<Rigidbody>().isKinematic = false;
		//COMMENT HERE DEACTIVATE CONTINOUS
		RegisterWall();
	}
	#endregion
	// Update is called once per frame
	void FixedUpdate()
	{

		if (restart)
		{
			restart = false;
			Restart();

		}

	}

	/// Block 1  Initialize Maze
	/// <summary>
	/// Generating Maze [Rows * Colums]
	/// </summary>
	///************************************************************
	private void InitializeMaze()
	{

		mazeCells = new MazeCell[mazeRows, mazeColumns];


		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				mazeCells[r, c] = new MazeCell();

				// Use the wall or trap object for the floor!
				mazeCells[r, c].floor = Instantiate(floor, transform.TransformPoint(new Vector3(r * size, -(size / 2f), c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
				mazeCells[r, c].floor.name = "Floor " + r + "," + c;
				mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);
				mazeCells[r, c].floor.transform.parent = mazeParent.transform;

				if (c == 0)
				{
					mazeCells[r, c].westWall = Instantiate(wall, transform.TransformPoint(new Vector3(r * size, 0, (c * size) - (size / 2f)) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
					mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
					mazeCells[r, c].westWall.transform.parent = mazeParent.transform;
				}

				mazeCells[r, c].eastWall = Instantiate(wall, transform.TransformPoint(new Vector3(r * size, 0, (c * size) + (size / 2f)) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
				mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
				mazeCells[r, c].eastWall.transform.parent = mazeParent.transform;

				if (r == 0)
				{
					mazeCells[r, c].northWall = Instantiate(wall, transform.TransformPoint(new Vector3((r * size) - (size / 2f), 0, c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
					mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
					mazeCells[r, c].northWall.transform.parent = mazeParent.transform;
				}

				mazeCells[r, c].southWall = Instantiate(wall, transform.TransformPoint(new Vector3((r * size) + (size / 2f), 0, c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
				mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
				mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);

			}
		}
	}
	/// Block 1  Initialize Maze
	///************************************************************

	/// Block 2  Dig Hole
	/// <summary>
	/// Randomly Diging Holes
	/// </summary>
	///************************************************************
	private void DigHole(int token)
	{

		//generated maze
		///**********************local var*****************************
		int mass = mazeRows * mazeColumns;
		int massMinusStartEnd = mass - 2;
		int curPosition = 0;
		// Start and End exclusiv
		int numberOfTraps = (int)(massMinusStartEnd * difficulty);
		// Same reason, Start and End exclusiv, initial a list from 1 to last - 1 (num = massMinusStartEnd)
		var trapPositions = Enumerable.Range(1, massMinusStartEnd).ToList();
		///************************************************************

		var index = token;

		if (index != 0)
			for (int i = 0; i < (massMinusStartEnd - numberOfTraps); i++)
			{
				if (massMinusStartEnd - i - 1 != 0)
				{
					index += ProceduralNumberGenerator.Stat_GetNextNumber(token);
					index = index % (massMinusStartEnd - i - 1);
					trapPositions.RemoveAt(index);
				}
				else
					trapPositions.RemoveAt(0);
			}
		else
			for (int i = 0; i < (massMinusStartEnd - numberOfTraps); i++)
			{
				if (massMinusStartEnd - i - 1 != 0)
				{
					int pos = UnityEngine.Random.Range(0, massMinusStartEnd - 1 - i);
					trapPositions.RemoveAt(pos);
				}
				else
					trapPositions.RemoveAt(0);
			}

		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{

				curPosition = (mazeColumns * r) + c;

				if (trapPositions.Contains(curPosition))
				{
					GameObject.Destroy(mazeCells[r, c].floor);
					mazeCells[r, c].floor = Instantiate(trap, transform.TransformPoint(new Vector3(r * size, -(size / 2f), c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
					mazeCells[r, c].floor.name = "Trap " + r + "," + c;
					mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);
					mazeCells[r, c].floor.transform.parent = mazeParent.transform;
					mazeCells[r, c].hasTrap = true;
				}

				// Instantiate Goal
				if ((curPosition + 1) == mass)
				{
					GameObject.Destroy(mazeCells[r, c].floor);
					mazeCells[r, c].floor = Instantiate(goal, transform.TransformPoint(new Vector3(r * size, -(size / 2f), c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
					mazeCells[r, c].floor.name = "Goal";
					mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);
					mazeCells[r, c].floor.transform.parent = mazeParent.transform;
				}
			}
		}


	}
	/// Block 2  Dig Hole
	///************************************************************

	/// Block 3  Generate Start and End
	/// <summary>
	/// Randomly Diging Holes
	/// </summary>
	///************************************************************
	private void InitializeBall()
	{
		player = Instantiate(ball, transform.TransformPoint(new Vector3(0 * size, -(size / 2f) + 1f, 0 * size) + shifting), Quaternion.identity, mazeParent.transform);
		player.name = "Player";
		//player.GetComponent<Rigidbody>().isKinematic = true;
		player.transform.parent = mazeParent.transform;
	}
	/// Block 2  Dig Hole
	///************************************************************
	///

	public float GetSize()
	{
		return this.size;
	}

	public MazeCellSerial[,] GetMazeCells()
	{
		return mazeCells;
	}

	public GameObject GetPlayer()
	{
		return this.player;
	}

	public GameObject GetGoal()
	{
		if (goal != null)
			if (goal.transform.parent != null)
				if (goal.transform.parent == transform.GetChild(0))
					return goal;

		foreach (Transform tra in this.transform)
		{
			if (tra.name == "Goal")
				goal = tra.gameObject;
		}

		return goal;

	}


# region continuous_curriculum

	private void RegisterWall()
	{
		//	return;
		DfsWalls.Clear();
		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				if (mazeCells[r, c].DfsWalls.Count > 0)
				{
					mazeCells[r, c].DfsWalls = mazeCells[r, c].DfsWalls.Distinct().ToList();
					// check all wall within DFS speicial wall list
					foreach (Transform tra in mazeCells[r, c].DfsWalls)
					{
						if (tra.name.Contains("North"))
						{
							RegisterWallNorth(tra, r, c);
						}
						else if (tra.name.Contains("South"))
						{
							RegisterWallSouth(tra, r, c);
						}
						else if (tra.name.Contains("East"))
						{
							RegisterWallEast(tra, r, c);
						}
						else 
						{
							RegisterWallWest(tra, r, c);
						}
					}
				}
					
				
;			}
		}
		foreach (Transform tra in DfsWalls)
		{
			tra.localScale = new Vector3(0, tra.localScale.y, tra.localScale.z);
		}
	}

	//check neighbor walls, decide which direction to shrink
	private void RegisterWallWest(Transform tra, int r, int c)
	{
		throw new NotImplementedException();
	}

	private void RegisterWallEast(Transform tra, int r, int c)
	{
		if (r == (mazeRows - 1))
		{
			PrepShrinkUp(tra, r, c);
		}
		else if (!mazeCells[r + 1, c].eastOpen)
		{
			PrepShrinkUp(tra, r, c);
		}
		else
		{
			PrepShrinkDown(tra, r, c);
		}
	}



	private void RegisterWallNorth(Transform tra, int r, int c)
	{
		throw new NotImplementedException();
	}

	private void RegisterWallSouth(Transform tra, int r, int c)
	{
		if (c == (mazeColumns - 1))
		{
			PrepShrinkLeft(tra, r, c);
		}
		else if (!mazeCells[r, c + 1].southOpen)
		{
			PrepShrinkLeft(tra, r, c);
		}
		else
		{
			PrepShrinkRight(tra, r, c);
		}
	}

	// create parent object, place it to adjust child(wall) pivot, so that only shrink in on direction
	private void PrepShrinkRight(Transform tra, int r, int c)
	{
		GameObject parent = Instantiate(
			dummy, new Vector3(tra.position.x, tra.position.y, (tra.position.z - GetSize() / 2)),
			tra.rotation,
			mazeParent.transform);
		tra.SetParent(parent.transform);
		DfsWalls.Add(parent.transform);
	}

	// create parent object, place it to adjust child(wall) pivot, so that only shrink in on direction
	private void PrepShrinkLeft(Transform tra, int r, int c)
	{
		GameObject parent = Instantiate(
			dummy, new Vector3(tra.position.x,tra.position.y,(tra.position.z + GetSize() / 2)),
			tra.rotation, 
			mazeParent.transform);
		tra.SetParent(parent.transform);
		DfsWalls.Add(parent.transform);
	}

	private void PrepShrinkUp(Transform tra, int r, int c)
	{
		GameObject parent = Instantiate(
			dummy, new Vector3((tra.position.x + GetSize() / 2), tra.position.y, tra.position.z),
			tra.rotation,
			mazeParent.transform);
		tra.SetParent(parent.transform);
		DfsWalls.Add(parent.transform);
	}

	private void PrepShrinkDown(Transform tra, int r, int c)
	{
		GameObject parent = Instantiate(
			dummy, new Vector3((tra.position.x - GetSize() / 2), tra.position.y, tra.position.z),
			tra.rotation,
			mazeParent.transform);
		tra.SetParent(parent.transform);
		DfsWalls.Add(parent.transform);
	}

	public void Shrink(float delta)
	{
		//return;
		maxScale += delta;
		maxScale = Math.Min(maxScale, 1);
		var minScale = Math.Max(0f, maxScale - 0.3f);
		var newScale = UnityEngine.Random.Range(minScale, maxScale);
		foreach (Transform tra in DfsWalls)
		{
			tra.localScale = new Vector3( newScale, tra.localScale.y, tra.localScale.z);
			Debug.Log(" maze " + this.name + " new scale is " + newScale);
		}
	
	}

	#endregion
}
