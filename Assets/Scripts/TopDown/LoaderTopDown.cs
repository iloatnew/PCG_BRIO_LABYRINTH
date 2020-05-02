using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

///************************************************************
/// <summary>
/// 1. Initialize Maze 
/// 2. CreateMaze (External Call <code> HuntAndKillMazeAlgorithm </code>) 
/// 3. Dig Hole
/// 4. Start and End
///</summary>
///************************************************************
public class LoaderTopDown : MonoBehaviour
{
	public int mazeRows, mazeColumns;
	public GameObject ball;
	public GameObject mazeParent;
	public GameObject wall;
	public GameObject trap;
	public GameObject goal;
	public bool criticalOnly;
	public GameObject[] mazes;
	public bool usePresetMaze;

	[Range(0, 1f)]
	public float difficulty;
	[Range(0, 10)]
	public int token;

	private MazeCell[,] mazeCells;
	private MazeCellSerial[,] mazeCellSerials;
	private GameObject player;
	public GameObject[] spawnPos;
	private float size = 6f;
	private Vector3 shifting;

	public bool restart;

	Vector3 fixPosition;
	HuntAndKillMutated ma;
	public GameObject maze;

	#region Restart Methods
	public void Restart()
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);

		GameObject.Destroy(player);
		player = Instantiate(ball, transform.TransformPoint(new Vector3(0 * size, -(size / 2f) + 1f, 0 * size) + shifting), Quaternion.identity, mazeParent.transform);
		player.transform.parent = mazeParent.transform;
	}


	/// <summary>
	/// Build Maze New
	/// </summary>
	public void RestartAndSpwan(int index)
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.position = fixPosition;

		GameObject.Destroy(player);
		player = Instantiate(ball, transform.TransformPoint(spawnPos[index].transform.position + new Vector3(0, 1, 0)), Quaternion.identity, maze.transform);
		player.transform.parent = maze.transform;
	}


	/// <summary>
	/// Build new maze, accroding to curriculum
	/// </summary>
	public void RebuildAndRestart(int index)
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.position = fixPosition;

		foreach (Transform tra in transform)
		{
			GameObject.Destroy(tra.gameObject);
		}

		string name = "Assets/MazeData/MazeCell_";
		name += mazes[index].name;
		name += ".dat";
		using (Stream stream = File.Open(name, FileMode.Open))
		{
			var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

			mazeCellSerials = (MazeCellSerial[,])bformatter.Deserialize(stream);

			mazeRows = mazeCellSerials.GetLength(0);
			mazeColumns = mazeCellSerials.GetLength(1);

			var halfLengthRow = (mazeRows - 1) * GetSize() / 2;
			var halfLengthColumn = (mazeColumns - 1) * GetSize() / 2;
			shifting = new Vector3(-halfLengthRow, 0, -halfLengthColumn);

			maze = GameObject.Instantiate(mazes[index], transform.TransformPoint(new Vector3(0, 0, 0)), Quaternion.identity, mazeParent.transform);

			player = Instantiate(ball, transform.TransformPoint(new Vector3(0 * size, -(size / 2f) + 1.5f, 0 * size) + shifting), Quaternion.identity, maze.transform);
			player.transform.parent = maze.transform;
		}
	}
	#endregion

	#region initialize method
	/// <summary>
	/// Use Awake() to make it called before Start from other method. Ensure all GameObjects are availible for others
	/// </summary>
	void Awake()
	{
		fixPosition = transform.position;
		if (!usePresetMaze)
		{
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

			//serialization
			//MazeCellSerial[,] mazeCellSerials = new MazeCellSerial[mazeRows, mazeColumns];
			//for (int r = 0; r < mazeRows; r++)
			//{
			//	for (int c = 0; c < mazeColumns; c++)
			//	{
			//		mazeCellSerials[r, c] = mazeCells[r, c].GetMazeCellSerial();
			//	}
			//}
			//		FileStream fs = new FileStream("Assets/MazeData/MazeCell_3_3_1.dat", FileMode.Create);
			//BinaryFormatter bf = new BinaryFormatter();
			//bf.Serialize(fs, mazeCellSerials);
			//fs.Close();

		}
		else 
		{
			InitializeBall();
		
		}

	}

	/// <summary>
	/// Use Start() to active gravity for the ball again.
	/// </summary>
	void Start()
	{
		player.GetComponent<Rigidbody>().isKinematic = false;
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
				mazeCells[r, c].floor = Instantiate(wall, transform.TransformPoint(new Vector3(r * size, -(size / 2f), c * size) + shifting), Quaternion.identity, mazeParent.transform) as GameObject;
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

		////preset Maze
		//foreach (Transform child in transform)
		//{
		//	if (child.name.Contains("Trap"))
		//	{
		//		string[] names = child.name.Split(',');
		//		int r = int.Parse(names[0].Last<char>().ToString());
		//		int c = int.Parse(names[1].First<char>().ToString());

		//		mazeCells[r, c].hasTrap = true;
		//	}

		//}


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

		if (token != 0)
			for (int i = 0; i < (massMinusStartEnd - numberOfTraps); i++)
			{
				if (massMinusStartEnd - i - 1 != 0)
				{
					token = token % (massMinusStartEnd - i - 1);
					trapPositions.RemoveAt(token);
					token += token;
				}
				else
					trapPositions.RemoveAt(0);
			}
		else
			for (int i = 0; i < (massMinusStartEnd - numberOfTraps); i++)
			{
				if (massMinusStartEnd - i - 1 != 0)
				{
					int pos = Random.Range(0, massMinusStartEnd - 1 - i);
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
				if ((curPosition+1) == mass)
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
		player.GetComponent<Rigidbody>().isKinematic = true;
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
		if (!usePresetMaze)
			return mazeCells;
		else
			return mazeCellSerials;
	}

	public GameObject GetPlayer()
	{
		return this.player;
	}

	public GameObject GetGoal()
	{
		return GameObject.Find("Goal");
	}
}
