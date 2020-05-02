using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;
using Barracuda;

public class AgentCurriculum_2 : Agent
{
	public LoaderCurriculum_2 mazeLoader;
	public AgentInteraction agentInteraction;
	public MazeAcademy mazeAcademy;
	public NNModel mazeBrain;
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
		index = (int)mazeAcademy.FloatProperties.GetPropertyWithDefault("mazeindex", 0);
		GiveModel("MazeBrain", mazeBrain);
		Debug.Log(index);

		mazeLoader.RestartAndSpwan(index);
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
		initDistance = Math.Abs(Vector3.Distance(ball.position, target.position));
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

		// Total 43 inputs
		List<float> state = agentInteraction.CollectBallState(ball.GetComponent<Rigidbody>(), target, transform.GetChild(0), ball); // 39 inputs collected??

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

		//isConered: punish
		if (agentInteraction.IsCornered(this.transform, ball))
		{
			AddReward(-0.005f);
		}

		// Actions, size = 2, Discret
		Vector3 controlSignal = Vector3.zero;

		if ((int)vectorAction[0] == 1)
			controlSignal.x = 1f;
		else if ((int)vectorAction[0] == 2)
			controlSignal.x = -1f;
		if ((int)vectorAction[1] == 1)
			controlSignal.z = 1f;
		else if ((int)vectorAction[1] == 2)
			controlSignal.z = -1f;

		agentInteraction.RefreshRotation(controlSignal);

		// Rewards
		float distanceToTarget = Math.Abs(Vector3.Distance(ball.position, target.position));
		if (distanceToTarget > initDistance + mazeLoader.GetSize() * 1.5)
		{
			SetReward(-0.1f);
			Debug.Log("outBound");
			Done();
		}

		//if (distanceToTarget < lastDistance)
		//{
		//	AddReward(0.01f);
		//}
		//else
		//{
		//	AddReward(-0.01f);
		//}


		lastDistance = distanceToTarget;
		// Fail
		float distanceToBoard = ball.localPosition.y + 3;

		if (IsMaxStepReached())
		{
			AddReward(-0.5f);
			Debug.Log("Max Step Reached");
			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			AddReward(4.0f);
			Debug.Log("Success");
			Done();
		}

		// Fell off platform
		if (Math.Abs(distanceToBoard) > 2.5f)
		{
			AddReward(-0.1f);
			Debug.Log("Fall");
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
