using MLAgents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AgentPic : Agent
{
	public Academy m_Academy;
	[FormerlySerializedAs("m_Area")]
	[Header("Specific to GridWorld")]
	public float timeBetweenDecisionsAtInference;
	float m_TimeSinceDecision;
	float lastDistance=0;

	[Tooltip("Because we want an observation right before making a decision, we can force " +
		"a camera to render before making a decision. Place the agentCam here if using " +
		"RenderTexture as observations.")]
	public Camera renderCamera;

	[Tooltip("Selecting will turn on action masking. Note that a model trained with action " +
		"masking turned on may not behave optimally when action masking is turned off.")]
	public bool maskActions = true;


	public LoaderPic mazeLoader;
	public Statistic_Writter statistic_Writter;
	int successCnt = 0;
	Transform target;
	Transform ball;

	public override void CollectObservations()
	{
		List<float> ballState = new List<float>();
		Rigidbody _rigidbody = mazeLoader.GetPlayer().transform.GetComponent<Rigidbody>();

		Vector3 normalizedVelocity = NormalizeVel(_rigidbody.velocity, -10, 10);
		ballState.Add(normalizedVelocity.x);
		ballState.Add(normalizedVelocity.y);
		ballState.Add(normalizedVelocity.z);

		AddVectorObs(ballState);

		// Mask the necessary actions if selected by the user.
		if (maskActions)
		{
			SetMask();
		}
	}

	/// <summary>
	/// Applies the mask for the agents action to disallow unnecessary actions.
	/// </summary>
	void SetMask()
	{
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
	}

	// to be implemented by the developer
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

		RefreshRotation(controlSignal);

		CalcRewards();
	}

	private void RefreshRotation(Vector3 controlRotation)
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

	// to be implemented by the developer
	public override void AgentReset()
	{
		if (successCnt > 9)
		{
			successCnt = 0;
			mazeLoader.Shrink(0.1f);
			Debug.Log("Maze " + this.name + " evolved ");
		}
		else 
		{
			mazeLoader.Shrink(0f);
		}
		mazeLoader.Restart();
	}

	public void FixedUpdate()
	{
		WaitTimeInference();
	}

	void WaitTimeInference()
	{
		if (renderCamera != null)
		{
			renderCamera.Render();
		}

		if (m_Academy.IsCommunicatorOn)
		{
			RequestDecision();
		}
		else
		{
			if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
			{
				m_TimeSinceDecision = 0f;
				RequestDecision();
			}
			else
			{
				m_TimeSinceDecision += Time.fixedDeltaTime;
			}
		}
	}

	private void CalcRewards()
	{
		//AddReward(-0.001f);

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
		//	Debug.Log("Connered");
		//}


		//if (agentInteraction.OutTracked())
		//{
		//	AddReward(-(1f / agentParameters.maxStep));
		//	//Debug.Log("Maze " + mazeLoader.maze.name + " OutTracked");
		//}

		// Fail
		float distanceToBoard = ball.localPosition.y + 3;

		if (GetStepCount() == agentParameters.maxStep)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());

			successCnt -= 2;
			successCnt = Math.Max(successCnt, 0);

			//Debug.Log("Max Step Reached with " + GetCumulativeReward());
			statistic_Writter.WriteStat(false, GetStepCount());
			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			AddReward(4.0f);
			SetReward(GetCumulativeReward());
			//Debug.Log("Maze " + mazeLoader.name + "Success with " + GetCumulativeReward());

			successCnt += 1;

			statistic_Writter.WriteStat(true, GetStepCount());
			Done();
		}

		// Fell off platform
		if (Math.Abs(distanceToBoard) > 2.5f)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());
			//Debug.Log("Fall with " + GetCumulativeReward());

			successCnt -= 2;
			successCnt = Math.Max(successCnt, 0);

			statistic_Writter.WriteStat(false, GetStepCount());

			Done();
		}

	}

	private Vector3 NormalizeVel(Vector3 original, int min, int max)
	{
		var x = original.x;
		var y = original.y;
		var z = original.z;

		Func<float, float> normalize = vel => (vel - min) / (max - min);

		return new Vector3(normalize(x), normalize(y), normalize(z));

	}
}
