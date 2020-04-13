using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;

public class MazeAgent : Agent
{
	public MazeLoader mazeLoader;
	public AgentInteraction agentInteraction;
	Transform target;
	Transform ball;
	MazeCell[,] mazeCells;
	float lastDistance;

	void Start()
	{
		mazeCells = mazeLoader.GetMazeCells();
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
		
		//AddVectorObs(Vector3.Distance(ball.position,
		//									  target.position));

		// Total 43 inputs
		List<float> state = agentInteraction.CollectBallState(ball.GetComponent<Rigidbody>(), target, transform, ball); // 39 inputs collected??

		state.Add(transform.localEulerAngles.x / 360f);
		state.Add(transform.localEulerAngles.z / 360f);
		//state.Add(Convert.ToSingle(_ballBehavior.IsCornered));

		AddVectorObs(state);
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
		// somehow these two always lost
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;

		// Actions, size = 2, Discret
		Vector3 controlSignal = Vector3.zero;

		if ((int)vectorAction[0] == 1)
			controlSignal.x = 1;
		else if ((int)vectorAction[0] == 2)
			controlSignal.x = -1;
		if ((int)vectorAction[1] == 1)
			controlSignal.z = 1;
		else if ((int)vectorAction[1] == 2)
			controlSignal.z = -1;
			
		agentInteraction.RefreshRotation(controlSignal);

		// Rewards
		float distanceToTarget = Math.Abs(Vector3.Distance(ball.position, target.position));

		if (distanceToTarget < lastDistance)
		{
			SetReward(0.01f);
		}
		else
		{
			SetReward(-0.01f);
		}


		lastDistance = distanceToTarget;
		// Fail
		float distanceToBoard = ball.localPosition.y + 3;

		if (IsMaxStepReached())
		{
			SetReward(-0.1f);
			Debug.Log("Max Step Reached");
			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			SetReward(1.0f);
			Done();
		}

		// Fell off platform
		if (Math.Abs(distanceToBoard) > 2.5f)
		{
			SetReward(-0.1f);
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
		action[0] = Input.GetAxis("Horizontal");
		action[1] = Input.GetAxis("Vertical");
		return action;
	}
}