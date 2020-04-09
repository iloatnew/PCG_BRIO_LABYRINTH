using UnityEngine;
using System.Collections;
using System.Linq;



///************************************************************
/// <summary>
/// 1. Initialize Maze 
/// 2. CreateMaze (External Call <code> HuntAndKillMazeAlgorithm </code>) 
/// 3. Dig Hole
/// 4. Start and End
///</summary>
///************************************************************
public class MazeLoader : MonoBehaviour {
	public int mazeRows, mazeColumns;
	public GameObject wall;
	public GameObject trap;
	public GameObject goal;
	public GameObject ball;
	public GameObject mazeParent;
	public bool criticalOnly;

	[Range(0, 1f)]
	public float difficulty ;

	private MazeCell[,] mazeCells;
	private GameObject player;
	private float size = 6f;
	private Vector3 shifting;
	HuntAndKillMutated ma;

	public bool restart;


	public void Restart() 
	{
		transform.rotation = Quaternion.Euler(0,0,0);

		GameObject.Destroy(player);
		player = Instantiate(ball, new Vector3(0 * size, -(size / 2f) + 5f, 0 * size) + shifting, Quaternion.identity);
		player.transform.parent = mazeParent.transform;
	}

	/// <summary>
	/// Use Awake() to make it called before Start from other method. Ensure all GameObjects are availible for others
	/// </summary>
	void Awake () {

		var halfLengthRow = (mazeRows - 1) * GetSize() / 2;
		var halfLengthColumn = (mazeColumns - 1) * GetSize() / 2;
		shifting = new Vector3(-halfLengthRow, 0, -halfLengthColumn);

		InitializeMaze ();
		
		ma = new HuntAndKillMutated (mazeCells);
		ma.CreateMaze ();

		if (criticalOnly)
		{
			ma.CriticalPathOnly();
		}

		ma.DestroyWalls();
		
		DigHole(); // Dig Holes accroding to difficulty.

		InitializeBall();
	}

	/// <summary>
	/// Use Start() to active gravity for the ball again.
	/// </summary>
	void Start()
	{
		player.GetComponent<Rigidbody>().isKinematic = false;
	}

	// Update is called once per frame
	void FixedUpdate () {

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
	private void InitializeMaze() {

		mazeCells = new MazeCell[mazeRows,mazeColumns];
		

		for (int r = 0; r < mazeRows; r++) {
			for (int c = 0; c < mazeColumns; c++) {
				mazeCells [r, c] = new MazeCell ();

				// Use the wall or trap object for the floor!
				mazeCells [r, c] .floor = Instantiate (wall, new Vector3 (r*size, -(size/2f), c*size) + shifting, Quaternion.identity) as GameObject;
				mazeCells [r, c] .floor.name = "Floor " + r + "," + c;
				mazeCells [r, c] .floor.transform.Rotate (Vector3.right, 90f);
				mazeCells[r, c].floor.transform.parent = mazeParent.transform;

				if (c == 0) {
					mazeCells[r,c].westWall = Instantiate (wall, new Vector3 (r*size, 0, (c*size) - (size/2f)) + shifting, Quaternion.identity) as GameObject;
					mazeCells [r, c].westWall.name = "West Wall " + r + "," + c;
					mazeCells[r, c].westWall.transform.parent = mazeParent.transform;
				}

				mazeCells [r, c].eastWall = Instantiate (wall, new Vector3 (r*size, 0, (c*size) + (size/2f)) + shifting, Quaternion.identity) as GameObject;
				mazeCells [r, c].eastWall.name = "East Wall " + r + "," + c;
				mazeCells[r, c].eastWall.transform.parent = mazeParent.transform;

				if (r == 0) {
					mazeCells [r, c].northWall = Instantiate (wall, new Vector3 ((r*size) - (size/2f), 0, c*size) + shifting, Quaternion.identity) as GameObject;
					mazeCells [r, c].northWall.name = "North Wall " + r + "," + c;
					mazeCells [r, c].northWall.transform.Rotate (Vector3.up * 90f);
					mazeCells[r, c].northWall.transform.parent = mazeParent.transform;
				}

				mazeCells[r,c].southWall = Instantiate (wall, new Vector3 ((r*size) + (size/2f), 0, c*size) + shifting, Quaternion.identity) as GameObject;
				mazeCells [r, c].southWall.name = "South Wall " + r + "," + c;
				mazeCells [r, c].southWall.transform.Rotate (Vector3.up * 90f);
				mazeCells[r, c].southWall.transform.parent = mazeParent.transform;
				
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
	private void DigHole()
	{
		int mass = mazeRows * mazeColumns;
		int massMinusStartEnd = mass - 2;
		// Start and End exclusiv
		int numberOfTraps = (int)(massMinusStartEnd * difficulty);
		// Same reason, Start and End exclusiv
		var trapPositions = Enumerable.Range(2, massMinusStartEnd).ToList();
		
		for (int i = 0; i < (massMinusStartEnd - numberOfTraps); i++)
		{
			trapPositions.RemoveAt(Random.Range(0, massMinusStartEnd - i));
		}

		int curPosition;

		for (int r = 0; r < mazeRows; r++){
			for (int c = 0; c < mazeColumns; c++){

				curPosition = (mazeColumns * r) + c + 1;

				if (trapPositions.Contains(curPosition)){
					GameObject.Destroy(mazeCells[r, c].floor);
					mazeCells[r, c].floor = Instantiate(trap, new Vector3(r * size, -(size / 2f), c * size) + shifting, Quaternion.identity) as GameObject;
					mazeCells[r, c].floor.name = "Trap " + r + "," + c;
					mazeCells[r, c].floor.transform.Rotate(Vector3.right, 90f);
					mazeCells[r, c].floor.transform.parent = mazeParent.transform;
				}

				// Instantiate Goal
				if (curPosition == mass) {
					GameObject.Destroy(mazeCells[r, c].floor);
					mazeCells[r, c].floor = Instantiate(goal, new Vector3(r * size, -(size / 2f), c * size) + shifting, Quaternion.identity) as GameObject;
					mazeCells[r, c].floor.name = "Goal " + r + "," + c;
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
		player = Instantiate(ball, new Vector3(0 * size, -(size / 2f) + 5f , 0 * size) + shifting, Quaternion.identity);
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

	public MazeCell[,] GetMazeCells()
	{
		return this.mazeCells;
	}

	public GameObject GetPlayer()
	{
		return this.player;
	}

	public GameObject GetGoal()
	{
		return this.mazeCells[mazeRows - 1, mazeColumns - 1].floor;
	}
}
