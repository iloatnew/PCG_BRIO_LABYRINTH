using UnityEngine;
using System.Collections;

public class ProceduralNumberGenerator {
	public static int stat_currentPosition = 0;
	public int currentPosition = 0;
	public const string con_key = "12342412334242143223314444121233443212122334412";
	public string key = "12342412334242143223314444121233443212122334412";

	public static int Stat_GetNextNumber(int token) {

		string currentNum = con_key.Substring( (stat_currentPosition += token) % con_key.Length, 1);
		return int.Parse(currentNum);
	
	}

	public static int GetRandomNumber()
	{
		return Random.Range(1,5);
	}

	public int GetNextNumber(int token)
	{

		string currentNum = key.Substring((currentPosition += token) % key.Length, 1);
		return int.Parse(currentNum);

	}
}
