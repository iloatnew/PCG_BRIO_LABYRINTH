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

	public void RefreshRotation(Vector3 controlRotation)
	{
		var curRotation = transform.rotation.eulerAngles;
		var nextRotation = curRotation + controlRotation;

		var x = nextRotation.x % 360;
		x = x > 180 ? x - 360 : x;
		x =  x > 45 ? 45 : x ;
		x = x < -45 ? -45 :x;

		var z = nextRotation.z % 360;
		z = z > 180 ? z - 360 : z;
		z = z > 45 ? 45 : z;
		z = z < -45 ? -45 : z;

		this.transform.rotation = Quaternion.Euler(x, 0, z);
	}
}
