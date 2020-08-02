using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
	public List<float> CollectBallState(Rigidbody _rigidbody, LoaderTopDown mazeLoader)
	{
		GameObject ball = mazeLoader.GetPlayer();
		Transform _targetGoal = mazeLoader.GetGoal().transform;
		GameObject _agent = this.gameObject;

		List<MazeCell> solutionPath = mazeLoader.GetSolutionPath();
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

		if (mazeLoader.mazeRows * mazeLoader.mazeColumns < 120)
		{
			int cnt = 120 - (mazeLoader.mazeRows * mazeLoader.mazeColumns*5);
			while (cnt > 0)
			{
				ballState.Add(1);
				cnt--;
			}
		}

		// 1 floats: 1/0 In Critic path or not
		ballState.AddRange(AddRouteStates(mazeCells, solutionPath, ball, _agent));

		// 2 floats: rotation x, z

		Vector3 normalized = NormalizeDeg(transform.rotation.eulerAngles);
		ballState.Add(normalized.x);
		ballState.Add(normalized.z);


		// 3 floats: Add velocity
		Vector3 normalizedVelocity = NormalizeVel(_rigidbody.velocity, -10, 10);
		ballState.Add(normalizedVelocity.x);
		ballState.Add(normalizedVelocity.y);
		ballState.Add(normalizedVelocity.z);

		// 3 floats: Add position of the ball 
		Vector3 ballPosition = NormalizePos(ball.transform.position, mazeLoader.GetSize(), mazeLoader.mazeRows, mazeLoader.mazeColumns);
		ballState.Add(ballPosition.x);
		ballState.Add(ballPosition.y);
		ballState.Add(ballPosition.z);

		// 2 floats: Add position of the goal 
		Vector3 goalPosition = NormalizePos(_targetGoal.transform.position, mazeLoader.GetSize(), mazeLoader.mazeRows, mazeLoader.mazeColumns);
		ballState.Add(goalPosition.x);
		ballState.Add(goalPosition.z);

		// 2 floats: Add relative position of the ball to the goal
		Vector3 relativePosition = goalPosition - ballPosition;
		ballState.Add(relativePosition.x);
		ballState.Add(relativePosition.z);

		// 1 float: absolute distance from ball to goal
		ballState.Add(Math.Abs(Vector3.Distance(goalPosition, ballPosition)));

		//// num Trap * 4 floats: x & z positions of traps, relative x & z positions from ball toward traps  
		//foreach (Transform child in transform)
		//{
		//	if (child.name.Contains("Trap"))
		//	{
		//		ballState.Add(child.position.x);
		//		ballState.Add(child.position.z);

		//		Vector3 relativePositionToTrap = ball.transform.position - child.position;
		//		ballState.Add(relativePositionToTrap.x);
		//		ballState.Add(relativePositionToTrap.z);
		//	}
		//}


		// Raycast surroundings
		// Create rays
		Ray[] rays = new Ray[_numRays];
		float step = 360 / _numRays;
		for (int i = 0; i < _numRays; i++)
		{
			Vector3 rayDirection = Quaternion.AngleAxis(step * i, _agent.transform.up) * _agent.transform.forward;
			rays[i] = new Ray(ball.transform.position, rayDirection);
		}

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

		// 16 floats: Execute raycasts on holes
		foreach (var ray in rays)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, _rayLength, _holeMask | _wallMask))
			{
				ballState.Add(hit.distance / _rayLength);
				Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.green);
			}
			else
			{
				ballState.Add(1.0f);
			}
		}
		return ballState;
	}


	#region subfunction
	/// <summary>
	/// In critical path: 1, not in critical path: 0
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	public List<float> AddRouteStates(MazeCell[,] mazeCells, List<MazeCell> solutionPath, GameObject ball, GameObject _agent)
	{
		List<float> ballState = new List<float>();
		Ray verticalRay = new Ray(ball.transform.position, -_agent.transform.up);
		string log = "";
		RaycastHit floorHit;
		MazeCell cellInSolution = new MazeCell();
		float percent;

		if (Physics.Raycast(verticalRay, out floorHit, 1.0f))
		{
			try
			{
				string[] names = floorHit.transform.name.Split(',');
				int r = int.Parse(names[0].Last<char>().ToString());
				int c = int.Parse(names[1].First<char>().ToString());
				foreach (MazeCell mc in solutionPath)
				{
					if (floorHit.transform.name == mc.floor.name)
					{
						cellInSolution = mc;
					}
				}
				
				if (mazeCells[r, c].inCriticalPath)
				{
					percent = (float) solutionPath.IndexOf(cellInSolution) / solutionPath.Count();
					ballState.Add(percent);
					outTracked = false;
					log += ("ball in critical path " + r + " " + c + " with percentage " + percent + " ; ");
				}
				else
				{
					ballState.Add(0.0f);
					outTracked = true;
					log += ("ball in dead end " + r + " " + c + " ; ");
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

		//Debug.Log(log);
		return ballState;;
	}


	/// <summary>
	/// Trap: 0, NoTrap: 1.
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	private List<float> AddFloorStates(MazeCellSerial mazeCell, int r, int c)
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
	private List<float> AddWallStates(MazeCellSerial mazeCell, int r, int c)
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


	public Vector3 NormalizeDeg(Vector3 original)
	{
		var x = original.x;
		var y = original.y;
		var z = original.z;

		Func<float, float> normalize = deg =>  deg % 360 > 0 ? (deg % 360)/360: (deg % 360 + 360)/360;

		return new Vector3(normalize(x), normalize(y), normalize(z));
	
	}

	private Vector3 NormalizeVel(Vector3 original, int min, int max)
	{
		var x = original.x;
		var y = original.y;
		var z = original.z;

		Func<float, float> normalize = vel => (vel - min)/(max - min);

		return new Vector3(normalize(x), normalize(y), normalize(z));

	}

	private Vector3 NormalizePos(Vector3 original, float size,  int row, int col)
	{
		// col = z , row = x

		var x = original.x;
		var y = original.y;
		var z = original.z;

		Func<float, float> normalizeX = pos => pos / (row * size) + 0.5f ;
		Func<float, float> normalizeZ = pos => pos / (col * size) + 0.5f;

		return new Vector3(normalizeX(x), 0, normalizeZ(z));

	}


}
