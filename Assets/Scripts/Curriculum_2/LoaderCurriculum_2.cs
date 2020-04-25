using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoaderCurriculum_2 : MonoBehaviour
{
	public int mazeRows, mazeColumns;
	public GameObject mazeParent;
	public GameObject[] spawnPos;
	public GameObject ball;

	private MazeCell[,] mazeCells;
	private GameObject player;
	private float size = 6f;
	private Vector3 shifting;
	public bool restart;

	public GameObject maze;

	Vector3 fixPosition;


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
		player = Instantiate(ball, transform.TransformPoint(spawnPos[index].transform.position + new Vector3(0,1,0)), Quaternion.identity, maze.transform);
		player.transform.parent = maze.transform;
	}


	/// <summary>
	/// Use Awake() to make it called before Start from other method. Ensure all GameObjects are availible for others
	/// </summary>
	void Awake()
	{
		RestartAndSpwan(0);
		fixPosition = this.transform.position;
	}



	// Update is called once per frame
	void FixedUpdate()
	{

		if (restart)
		{
			restart = false;
			Restart();

		}


	}



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

		return GameObject.Find("Goal").gameObject;

	}

}
