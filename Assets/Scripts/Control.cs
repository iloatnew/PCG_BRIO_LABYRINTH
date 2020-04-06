using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{
	public MazeLoader mazeLoader;
	public GameObject maze;
    // Start is called before the first frame update
    void Start()
    {
		ShiftMaze();
    }

	private void ShiftMaze()
	{
		var halfLengthRow = mazeLoader.mazeRows * mazeLoader.GetSize() / 2;
		var halfLengthColumn = mazeLoader.mazeColumns * mazeLoader.GetSize() / 2;
		var shifting = new Vector3(-halfLengthRow, 0, -halfLengthColumn);
		maze.transform.position = maze.transform.position + shifting;
	}

	// Update is called once per frame
	void Update()
    {
		var curRotation = transform.rotation.eulerAngles;
		var nextRotation = curRotation;
		if (Input.GetKey(KeyCode.W))
		{
			nextRotation = curRotation + new Vector3(0f, 0f, -0.5f);
		}
		if (Input.GetKey(KeyCode.A))
		{
			nextRotation = curRotation + new Vector3(0.5f, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.S))
		{
			nextRotation = curRotation + new Vector3(0f, 0f, 0.5f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			nextRotation = curRotation + new Vector3(-0.5f, 0f, 0f);
		}
		this.transform.rotation = Quaternion.Euler(nextRotation.x, nextRotation.y, nextRotation.z);
	}
}
