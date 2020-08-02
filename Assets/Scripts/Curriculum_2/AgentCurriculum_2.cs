using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;
using Barracuda;

public class AgentCurriculum_2 : Agent
{
	public LoaderTopDown mazeLoader;
	public MazeAcademy mazeAcademy;
	public InteractionTopDown agentInteraction;
	public int index;
	Transform target;
	Transform ball;
	float lastDistance;
	float initDistance;

	void Start()
	{
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
	}

	/// <summary>
	/// Specifies the agent behavior when being reset, which can be due to
	/// the agent or Academy being done (i.e. completion of local or global
	/// episode).
	/// </summary>
	public override void AgentReset()
	{
		mazeLoader.Restart();
	}

	/// <summary>
	/// Collects the (vector, visual) observations of the agent.
	/// The agent observation describes the current environment from the
	/// perspective of the agent.
	/// </summary>
	public override void CollectObservations()
	{
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
		List<float> ballState = new List<float>();
		// Raycast surroundings
		// Create rays
		Ray[] rays = new Ray[16];
		float step = 360 / 16;
		for (int i = 0; i < 16; i++)
		{
			Vector3 rayDirection = Quaternion.AngleAxis(step * i, transform.up) * transform.forward;
			rays[i] = new Ray(ball.transform.position, rayDirection);
		}

		// 16 floats: Execute raycasts on walls
		foreach (var ray in rays)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, agentInteraction._rayLength, (agentInteraction._wallMask | agentInteraction._holeMask) ))
			{
				ballState.Add(hit.distance / agentInteraction._rayLength);
				//Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.red);
			}
			else
			{
				ballState.Add(1.0f);
			}
		}
		AddVectorObs(ballState);

	}


	/// <summary>
	/// Specifies the agent behavior at every step based on the provided
	/// action.
	/// </summary>
	/// <param name="vectorAction">
	/// Vector action. Note that for discrete actions, the provided array
	/// will be of length 1.
	/// </param>
	public override void AgentAction(float[] vectorAction)
	{

		// Actions, branch = 2; size = 5, Discret
		Vector3 controlSignal = Vector3.zero;

		if ((int)vectorAction[0] == 1)
			controlSignal.x = 1f;
		else if ((int)vectorAction[0] == 2)
			controlSignal.x = -1f;
		else if ((int)vectorAction[0] == 3)
			controlSignal.x = 2.5f;
		else if ((int)vectorAction[0] == 4)
			controlSignal.x = -2.5f;

		if ((int)vectorAction[1] == 1)
			controlSignal.z = 1f;
		else if ((int)vectorAction[1] == 2)
			controlSignal.z = -1f;
		else if ((int)vectorAction[1] == 3)
			controlSignal.z = 2.5f;
		else if ((int)vectorAction[1] == 4)
			controlSignal.z = -2.5f;

		agentInteraction.RefreshRotation(controlSignal);

		float distanceToTarget = Math.Abs(Vector3.Distance(ball.position, target.position));

		//// Rewards for Curri differen spawning posi
		//if (distanceToTarget > initDistance + mazeLoader.GetSize() * 1.5)
		//{
		//	AddReward(-0.1f);
		//	SetReward(GetCumulativeReward());
		//	Debug.Log("outBound");
		//	Done();
		//}

		//if (distanceToTarget < lastDistance)
		//{
		//	AddReward(1f / agentParameters.maxStep);
		//}
		//else
		//{
		//	AddReward(-1f / agentParameters.maxStep);
		//}

		//lastDistance = distanceToTarget;


		////isConered: punish
		//if (agentInteraction.IsCornered(this.transform, ball))
		//{
		//	AddReward(-(1f / agentParameters.maxStep));
		//	//Debug.Log("Connered");
		//}


		//if (agentInteraction.OutTracked())
		//{
		//	AddReward(-(1f / agentParameters.maxStep));
		//	//Debug.Log("Maze " + mazeLoader.maze.name + " OutTracked");
		//}

		// Fail
		float distanceToBoard = ball.localPosition.y;

		if (GetStepCount() == agentParameters.maxStep)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());
			Debug.Log("Max Step Reached with " + GetCumulativeReward());

			//statistic_Writter.WriteStat(false, GetStepCount());

			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			AddReward(4.0f);
			//success_cnt++;
			SetReward(GetCumulativeReward());
			Debug.Log("Maze " + mazeLoader.maze.name + "Success with " + GetCumulativeReward());

			//statistic_Writter.WriteStat(true, GetStepCount());

			Done();
		}

		// Fell off platform
		if (distanceToBoard < -3.5f)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());
			//Debug.Log("Fall with " + GetCumulativeReward());

			//statistic_Writter.WriteStat(false, GetStepCount());

			Done();
		}
	}

	/// <summary>
	/// When the Agent uses Heuristics, it will call this method every time it
	/// needs an action. This can be used for debugging or controlling the agent
	/// with keyboard.
	/// </summary>
	/// <returns> A float array corresponding to the next action of the Agent
	/// </returns>
	public override float[] Heuristic()
	{
		var action = new float[2];

		if (Input.GetKeyDown(KeyCode.W))
			action[0] = 1;
		else if (Input.GetKeyDown(KeyCode.S))
			action[0] = 2;
		if (Input.GetKeyDown(KeyCode.A))
			action[1] = 1;
		else if (Input.GetKeyDown(KeyCode.D))
			action[1] = 2;
		return action;
	}

}
