﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentInteraction : MonoBehaviour
{
	public MazeLoader mazeLoader;
	public int _numRays;
	public float _rayLength;
	public LayerMask _wallMask;
	public LayerMask _holeMask;

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


	#region Public Functions
	/// <summary>
	/// Collects the ball's state
	/// </summary>
	/// <returns>Returns a list of features to describe the ball's state. Features: Ball position in relation to the final hole, several raycasts to sense its surroundings and its velocity.</returns>
	public List<float> CollectBallState(Rigidbody _rigidbody,  Transform _targetGoal, Transform _agent, Transform ball)
	{

		List<float> ballState = new List<float>();

		// 3 floats: Add velocity
		Vector3 normalizedVelocity = _rigidbody.velocity.normalized;
		ballState.Add(normalizedVelocity.x);
		ballState.Add(normalizedVelocity.y);
		ballState.Add(normalizedVelocity.z);

		// 3 floats: Add relative position of the ball to the goal
		Vector3 relativePosition = ball.transform.position - _targetGoal.position;
		ballState.Add(relativePosition.x );
		ballState.Add(relativePosition.y );
		ballState.Add(relativePosition.z );

		// 1 floats: Add ball height
		Ray verticalRay = new Ray(ball.transform.position, -_agent.transform.up);
		RaycastHit floorHit;
		if (Physics.Raycast(verticalRay, out floorHit, 1.0f))
		{
			ballState.Add(floorHit.distance);
		}
		else
		{
			ballState.Add(1.0f);
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

		// 8 floats: Execute raycasts on walls
		foreach (var ray in rays)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, _rayLength, _wallMask))
			{
				ballState.Add(hit.distance / _rayLength);
				Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.red);
			}
			else
			{
				ballState.Add(1.0f);
			}
		}

		// 8 floats: Execute raycasts on holes
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
	#endregion


}
