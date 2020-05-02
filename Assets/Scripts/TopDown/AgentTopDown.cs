using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System;
using Barracuda;

public class AgentTopDown: Agent
{
	public LoaderTopDown mazeLoader;
	public InteractionTopDown agentInteraction;
	public MazeAcademy mazeAcademy;
	public NNModel mazeBrain;
	public int index;
	public bool useCurriculum;
	Transform target;
	Transform ball;
	float lastDistance;
	float initDistance;

	void Start()
	{
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
		if (mazeLoader.usePresetMaze)
		{
			useCurriculum = true;
		}
	}

	/// <summary>
	/// Specifies the agent behavior when being reset, which can be due to
	/// the agent or Academy being done (i.e. completion of local or global
	/// episode).
	/// </summary>
	public override void AgentReset()
	{

		GiveModel("MazeBrain", mazeBrain);
		//curriculum with different mazes
		if (useCurriculum)
		{
			index = (int)mazeAcademy.FloatProperties.GetPropertyWithDefault("mazeindex", 0);
			mazeLoader.RebuildAndRestart(index);
		}
		//normal
		else 
		{
			mazeLoader.Restart();
		}
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
		initDistance = Math.Abs(Vector3.Distance(ball.position, target.position));
		//mazeLoader.RestartAndSpwan(index);

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
		List<float> state = agentInteraction.CollectBallState(ball.GetComponent<Rigidbody>(), mazeLoader); // 39 inputs collected??

		// Set mask makes agent only able to trun it backwards, when the board reaches already the max rotation
		var curRotation = transform.rotation.eulerAngles;

		var x = curRotation.x % 360;
		x = x > 180 ? x - 360 : x;

		var z = curRotation.z % 360;
		z = z > 180 ? z - 360 : z;

		if (x > 29f)
			SetActionMask(0, new int[3] { 0, 1, 3 });
		else if (x < -29f)
			SetActionMask(0, new int[3] { 0, 2, 4 });

		if (z > 29f)
			SetActionMask(1, new int[3] { 0, 1, 3 });
		else if (z < -29f)
			SetActionMask(1, new int[3] { 0, 2, 4 });

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
		// somehow these two always unassigned
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;

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
		
		CalcRewards();

		
	}

	private void CalcRewards()
	{
		float distanceToTarget = Math.Abs(Vector3.Distance(ball.position, target.position));

		//// Rewards for Curri differen spawning posi
		//if (distanceToTarget > initDistance + mazeLoader.GetSize() * 1.5)
		//{
		//	AddReward(-0.1f);
		//  SetReward(GetCumulativeReward());
		//	Debug.Log("outBound");
		//	Done();
		//}

		if (distanceToTarget < lastDistance)
		{
			AddReward(1f / agentParameters.maxStep);
		}
		else
		{
			AddReward(-1f / agentParameters.maxStep);
		}

		lastDistance = distanceToTarget;


		//isConered: punish
		if (agentInteraction.IsCornered(this.transform, ball))
		{
			AddReward(-(1f / agentParameters.maxStep));
			Debug.Log("Connered");
		}


		if (agentInteraction.OutTracked())
		{
			AddReward(-(1f / agentParameters.maxStep));
			Debug.Log("OutTracked");
		}

		// Fail
		float distanceToBoard = ball.localPosition.y + 3;

		if (GetStepCount() == agentParameters.maxStep)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());
			Debug.Log("Max Step Reached with " + GetCumulativeReward());
			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			AddReward(4.0f);
			SetReward(GetCumulativeReward());
			Debug.Log("Success with " + GetCumulativeReward());
			Done();
		}

		// Fell off platform
		if (Math.Abs(distanceToBoard) > 2.5f)
		{
			AddReward(-0.1f);
			SetReward(GetCumulativeReward());
			Debug.Log("Fall with " + GetCumulativeReward());
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
