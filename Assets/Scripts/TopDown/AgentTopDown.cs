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
	public Statistic_Writter statistic_Writter;
	public int index;
	public bool useCurriculum;
	public bool useRandomSpawn;
	Transform target;
	Transform ball;
	float lastDistance;
	Vector3 cur_state = new Vector3();
	Vector3[] states = new Vector3[8];
	Vector3[] oldstates = new Vector3[8];
	int success_cnt = 0;
	public float noise;

	private List<MazeCell> solutionPath;

	void Start()
	{
		target = mazeLoader.GetGoal().transform;
		ball = mazeLoader.GetPlayer().transform;
		//world position
		cur_state = transform.TransformPoint( mazeLoader.GetGoal().transform.localPosition + new Vector3(0f, ball.localScale.y, 0) );
		//uncomment here, activate rev curri
		SampleNextStates(cur_state);
		oldstates = states;
	}

	/// <summary>
	/// Specifies the agent behavior when being reset, which can be due to
	/// the agent or Academy being done (i.e. completion of local or global
	/// episode).
	/// </summary>
	public override void AgentReset()
	{

		if (useCurriculum)
		{
			//Debug.Log("success " + success_cnt + " times");
			if (success_cnt >= 5)
			{
				success_cnt = 0;
				SampleNextStates(cur_state);
				cur_state = transform.TransformPoint(states[0]);
			}
		
			mazeLoader.RebuildAndSpwan(oldstates, states);
		}
		//normal
		else 
		{
			mazeLoader.RebuildAndRestart();
			//mazeLoader.Restart();
		}
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
			controlSignal.x = 0.75f;
		else if ((int)vectorAction[0] == 2)
			controlSignal.x = -0.75f;
		else if ((int)vectorAction[0] == 3)
			controlSignal.x = 1.75f;
		else if ((int)vectorAction[0] == 4)
			controlSignal.x = -1.75f;

		if ((int)vectorAction[1] == 1)
			controlSignal.z = 0.75f;
		else if ((int)vectorAction[1] == 2)
			controlSignal.z = -0.75f;
		else if ((int)vectorAction[1] == 3)
			controlSignal.z = 1.75f;
		else if ((int)vectorAction[1] == 4)
			controlSignal.z = -1.75f;

		agentInteraction.RefreshRotation(controlSignal);
		CalcRewards();

	}

	private void FixedUpdate()
	{
		//Debug.Log(GetCumulativeReward());
	}

	private void CalcRewards()
	{
		// Dense Reward 2

		//float percent = agentInteraction.AddRouteStates(
		//	mazeLoader.GetMazeCells(),
		//	mazeLoader.GetSolutionPath(),
		//	mazeLoader.GetPlayer(),
		//	this.gameObject
		//)[0];

		//if (percent > lastDistance)
		//{
		//	AddReward( 2f / mazeLoader.GetSolutionPath().Count);
		//}
		//else if(percent < lastDistance)
		//{
		//	AddReward(-2f / mazeLoader.GetSolutionPath().Count );
		//}
		
		//lastDistance = percent;

		float distanceToTarget = Math.Abs(Vector3.Distance(ball.position, target.position));

		// Dense reward 1
		
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

			statistic_Writter.WriteStat(false, GetStepCount());

			Done();
		}

		// Reached target
		if (distanceToTarget < 3f)
		{
			AddReward(2.0f);
			success_cnt++;
			SetReward(GetCumulativeReward());
			Debug.Log("Maze " + mazeLoader.maze.name + "Success with " + GetCumulativeReward());

			statistic_Writter.WriteStat(true, GetStepCount());

			Done();
		}

		// Fell off platform
		if (distanceToBoard < -3.5f)
		{
			AddReward(-0.5f);
			SetReward(GetCumulativeReward());
			//Debug.Log("Fall with " + GetCumulativeReward());

			statistic_Writter.WriteStat(false, GetStepCount());

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

	public void SampleNextStates(Vector3 cur_state)
	{
		//Debug.Log("sample with " + cur_state);

		Vector3 state = new Vector3();
		Ray[] rays = new Ray[16];
		float step = 360 / 16;
		for (int i = 0; i < 16; i++)
		{
			Vector3 rayDirection = Quaternion.AngleAxis(step * i, transform.up) * transform.forward;
			rays[i] = new Ray(cur_state, rayDirection);
		}

		int iter = 0;

		// 16 floats: Execute raycasts on holes and walls
		foreach (var ray in rays)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, agentInteraction._rayLength, agentInteraction._holeMask | agentInteraction._wallMask))
			{
				//Debug.DrawLine(ray.origin, ray.origin + ray.direction * hit.distance, Color.red);

				// new state: cell_size away from cur state or the hit position. Must be far away than current position
				var shift = (hit.point - cur_state).normalized * mazeLoader.GetSize();
				if (shift.magnitude > (hit.point - cur_state).magnitude) 
				{
					shift = (hit.point - cur_state);
				}
				shift -= (hit.point - cur_state).normalized * ball.localScale.y/2;
				state = cur_state + shift;

				var new_state_start = Math.Abs( Vector3.Distance(transform.TransformPoint(mazeLoader.GetShift()), state) ) - noise;
				var cur_state_start = Math.Abs( Vector3.Distance(transform.TransformPoint(mazeLoader.GetShift()), cur_state));

				if (new_state_start < cur_state_start) {

					//Debug.Log("add state " + iter + " from " + cur_state + " value: " + transform.InverseTransformPoint(state));
					// state[] local position
					states[iter] = transform.InverseTransformPoint(state);

					//cant greater than 7
					if (iter < 7)
						iter++;

					// 5%, refresh old states buffer
					if (UnityEngine.Random.Range(0, 1) < 0.05)
					{
						oldstates[iter] = states[iter];
					}
				}
				
				
			}
			
		}

		//if (init) {
		//	init = false;
		//	oldstates = states;
		//	AgentReset();
		//}

	}


}
