using UnityEngine;
using System.Collections;

public class ChangeBallLayer : MonoBehaviour
{

	public int LayerOnEnter; // BallInHole
	public int LayerOnExit;  // BallOnTable
	

	void OnTriggerStay(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
		
			other.gameObject.layer = LayerOnEnter;
		}
	}
}

