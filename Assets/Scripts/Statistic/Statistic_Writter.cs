using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistic_Writter : MonoBehaviour
{
	private int turn = 0;
	private bool success;
	private string[] stats = new string[101];

	public void WriteStat( bool success, int step)
	{
		Vector2 stat;
		if (turn < 100)
		{
			if (success)
				stat = new Vector2(1, (float)step);
			else
				stat = new Vector2(0, (float)step);

			stats[turn] = stat.x + ";" + stat.y;

		}

		if (turn == 5)
		{
			string dir = @"D:\Beruf\BADATA\"+ "dense3"+ "_" + gameObject.name +".txt";
			try
			{
				System.IO.File.ReadLines(dir);
				Debug.Log("file exists already!");
			}
			catch
			{
				//file not exist
				System.IO.File.WriteAllLines(dir, stats);
				Debug.Log(gameObject.name + " write! " + turn);
			}
		
		}
		//turn += 1;
	}
  

}
