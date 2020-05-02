using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionTopDown : MonoBehaviour
{
	public int _numRays;
	public float _rayLength;
	public LayerMask _wallMask;
	public LayerMask _holeMask;
	bool outTracked;


	public void RefreshRotation(Vector3 controlRotation)
	{
		var curRotation = transform.rotation.eulerAngles;
		var nextRotation = curRotation + controlRotation;

		var x = nextRotation.x % 360;
		x = x > 180 ? x - 360 : x;
		x = x > 30 ? 30 : x;
		x = x < -30 ? -30 : x;

		var z = nextRotation.z % 360;
		z = z > 180 ? z - 360 : z;
		z = z > 30 ? 30 : z;
		z = z < -30 ? -30 : z;

		this.transform.rotation = Quaternion.Euler(x, 0, z);

	}



	/// <summary>
	/// Collects the ball's state, size = (massMaze * 5) + (4 * numTrap) + 32; maxsize = (massMaze + 4) * 10 
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	public List<float> CollectBallState(Rigidbody _rigidbody, LoaderTopDown mazeLoader )
	{
		GameObject ball = mazeLoader.GetPlayer();
		Transform _targetGoal = mazeLoader.GetGoal().transform;
		GameObject _agent = this.gameObject;
		MazeCell[,] mazeCells = mazeLoader.GetMazeCells();

		List<float> ballState = new List<float>();

		// (num mazeCells * 5) floats: the layout of maze
		for (int r = 0; r < mazeLoader.mazeRows; r++)
		{
			for (int c = 0; c < mazeLoader.mazeColumns; c++)
			{
				// 4 floats: 1/0 Wall or not
				ballState.AddRange(AddWallStates(mazeCells[r, c], r, c));
				// 1 floats: 1/0 Trap or not
				ballState.AddRange(AddFloorStates(mazeCells[r, c], r, c));
			}
		}

		// 1 floats: 1/0 In Critic path or not
		ballState.AddRange(AddRouteStates(mazeCells, ball, _agent));
	
		// 2 floats: rotation x, z
		ballState.Add(transform.rotation.eulerAngles.x);
		ballState.Add(transform.rotation.eulerAngles.z);

		// 3 floats: Add velocity
		Vector3 normalizedVelocity = _rigidbody.velocity.normalized;
		ballState.Add(normalizedVelocity.x);
		ballState.Add(normalizedVelocity.y);
		ballState.Add(normalizedVelocity.z);

		// 3 floats: Add position of the ball 
		Vector3 ballPosition = ball.transform.position;
		ballState.Add(ballPosition.x);
		ballState.Add(ballPosition.y);
		ballState.Add(ballPosition.z);

		// 2 floats: Add position of the goal 
		Vector3 goalPosition = _targetGoal.transform.position;
		ballState.Add(goalPosition.x);
		ballState.Add(goalPosition.z);

		// 2 floats: Add relative position of the ball to the goal
		Vector3 relativePosition = ball.transform.position - _targetGoal.position;
		ballState.Add(relativePosition.x);
		ballState.Add(relativePosition.z);

		// 1 float: absolute distance from ball to goal
		ballState.Add(Math.Abs(Vector3.Distance(ball.transform.position, _targetGoal.position)));

		// num Trap * 4 floats: x & z positions of traps, relative x & z positions from ball toward traps  
		foreach (Transform child in transform)
		{
			if (child.name.Contains("Trap"))
			{
				ballState.Add(child.position.x);
				ballState.Add(child.position.z);

				Vector3 relativePositionToTrap = ball.transform.position - child.position;
				ballState.Add(relativePositionToTrap.x);
				ballState.Add(relativePositionToTrap.z);
			}
		}


		// Raycast surroundings
		// Create rays
		Ray[] rays = new Ray[_numRays];
		float step = 360 / _numRays;
		for (int i = 0; i < _numRays; i++)
		{
			Vector3 rayDirection = Quaternion.AngleAxis(step * i, _agent.transform.up) * _agent.transform.forward;
			rays[i] = new Ray(ball.transform.position, rayDirection);
		}

		//// Draw rays for debugging
		//foreach (var ray in rays)
		//{
		//	Debug.DrawLine(ray.origin, ray.origin + ray.direction * _rayLength, Color.red);
		//}

		// 16 floats: Execute raycasts on walls
		foreach (var ray in rays)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, _rayLength, _wallMask))
			{
				ballState.Add(hit.distance / _rayLength);
				//Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.red);
			}
			else
			{
				ballState.Add(1.0f);
			}
		}

		//// 16 floats: Execute raycasts on holes
		//foreach (var ray in rays)
		//{
		//	RaycastHit hit;
		//	if (Physics.Raycast(ray, out hit, _rayLength, _holeMask | _wallMask))
		//	{
		//		ballState.Add(hit.distance / _rayLength);
		//		Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.green);
		//	}
		//	else
		//	{
		//		ballState.Add(1.0f);
		//	}
		//}

		return ballState;
	}

	#region subfunction
	/// <summary>
	/// In critical path: 1, not in critical path: 0
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	private List<float> AddRouteStates( MazeCell[,] mazeCells, GameObject ball, GameObject _agent)
	{
		List<float> ballState = new List<float>();
		Ray verticalRay = new Ray(ball.transform.position, -_agent.transform.up);
		string log = "";
		RaycastHit floorHit;


		if (Physics.Raycast(verticalRay, out floorHit, 1.0f))
		{
			try
			{
				string[] names = floorHit.transform.name.Split(',');
				int r = int.Parse(names[0].Last<char>().ToString());
				int c = int.Parse(names[1].First<char>().ToString());

				if (mazeCells[r, c].inCriticalPath)
				{
					ballState.Add(1.0f);
					outTracked = false;
					log += ("ball in critical path " + r + " " + c + " ; ");
				}
				else
				{
					ballState.Add(0.0f);
					outTracked = true;
					log += ("ball in UNcritical path " + r + " " + c + " ; ");
				}
			}
			catch (Exception exp)
			{
				if (floorHit.transform.name.Contains("Goal"))
				{
					ballState.Add(1.0f);
					outTracked = false;
					log += ("ball in critical path " + "Goal; ");
				}
				else
				{
					outTracked = true;
					ballState.Add(-1.0f);
				}
			}
		}
		else
		{
			ballState.Add(-1.0f);
			outTracked = true;
			log += ("floor under ball not found");
		}
		return ballState;
		//Debug.Log(log);
	}


	/// <summary>
	/// Trap: 0, NoTrap: 1.
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	private List<float> AddFloorStates(MazeCell mazeCell, int r, int c)
	{
		List<float> ballState = new List<float>();
		string log = "cell " + r + " " + c + " : ";
		if (mazeCell.hasTrap)
		{
			ballState.Add(0);
			log += "has trap";
		}
		else
		{
			ballState.Add(0);
		}
		//Debug.Log(log);
		return ballState;
	}


	/// <summary>
	/// Path: 0, Wall: 1.
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	private List<float> AddWallStates(MazeCell mazeCell, int r, int c)
	{
		List<float>  ballState = new List<float>();
		string log = "cell " + r + " " + c + " : ";
		if (mazeCell.northOpen)
		{
			ballState.Add(1);
			log += "no wall on north; ";
		}
		else
		{
			ballState.Add(0);
		}
		if (mazeCell.southOpen)
		{
			ballState.Add(1);
			log += "no wall on south; ";
		}
		else
		{
			ballState.Add(0);
		}
		if (mazeCell.eastOpen)
		{
			ballState.Add(1);
			log += "no wall on east; ";
		}
		else
		{
			ballState.Add(0);
		}
		if (mazeCell.westOpen)
		{
			ballState.Add(1);
			log += "no wall on west; ";
		}
		else
		{
			ballState.Add(0);
		}
		//Debug.Log(log);
		return ballState;
	}

	#endregion


	public bool IsCornered(Transform plant, Transform ball)
	{
		// Check if ball is stuck in a corner
		// Setup rays
		Ray[] rays = new Ray[4];
		RaycastHit[] hits = new RaycastHit[rays.Length];
		bool[] wallHits = new bool[rays.Length];
		rays[0] = new Ray(ball.transform.position, -plant.transform.right); // left
		rays[1] = new Ray(ball.transform.position, plant.transform.forward); // forward
		rays[2] = new Ray(ball.transform.position, plant.transform.right); // right
		rays[3] = new Ray(ball.transform.position, -plant.transform.forward); // back
																			  // Execute raycasts
		for (int i = 0; i < rays.Length; i++)
		{
			wallHits[i] = Physics.Raycast(rays[i], out hits[i], transform.localScale.x / 2 + 0.005f, _wallMask);
			Debug.DrawLine(rays[i].origin, rays[i].origin + (rays[i].direction.normalized * (transform.localScale.x / 2 + 0.005f)), Color.black, 0.0f);
		}
		// Evaluate raycasts
		var _isCornered = false;
		for (int i = 0; i < rays.Length; i++)
		{
			if (i < rays.Length - 1)
			{
				if (wallHits[i] && wallHits[i + 1])
					_isCornered = true;

			}
			else
			{
				if (wallHits[i] && wallHits[0])
					_isCornered = true;
			}
		}
		return _isCornered;
	}


	public bool OutTracked()
	{

		return outTracked;
	}



}
