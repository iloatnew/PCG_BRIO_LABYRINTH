using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;

public class MazeAgent : Agent
{
	public MazeLoader mazeLoader;
	public GameObject agent;
	public Control control;
	Transform target;
	Transform ball;
	MazeCell[,] mazeCells;

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
		Debug.Log("reset");
		mazeLoader.Restart();
	}

	/// <summary>
	/// Collects the (vector, visual) observations of the agent.
	/// The agent observation describes the current environment from the
	/// perspective of the agent.
	/// </summary>
	public override void CollectObservations()
	{
		// Target and Agent positions
		AddVectorObs(target.position);
		AddVectorObs(ball.position);

		// Agent velocity
		AddVectorObs(ball.GetComponent<Rigidbody>().velocity.x);
		AddVectorObs(ball.GetComponent<Rigidbody>().velocity.z);
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

		// Actions, size = 2
		Vector3 controlSignal = Vector3.zero;
		controlSignal.x = vectorAction[0];
		controlSignal.z = vectorAction[1];

		control.RefreshRotation(controlSignal);

		// Rewards
		float distanceToTarget = Vector3.Distance(ball.position,
												  target.position);

		// Reached target
		if (distanceToTarget < 3f)
		{
			SetReward(1.0f);
			Done();
		}

		// Fell off platform
		if (distanceToTarget > mazeLoader.mazeRows*mazeLoader.mazeColumns*mazeLoader.GetSize())
		{
			Debug.Log("done");
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