using MLAgents;

public class MazeAcademy : Academy 
{
	public int index;
	/// <summary>
	/// Called when the academy first gets initialized
	/// </summary>
	public override void InitializeAcademy()
	{
		index = (int)FloatProperties.GetPropertyWithDefault("mazeindex", 0);

		// Set up code to be called every time the fish_speed parameter changes 
		// during curriculum learning
		FloatProperties.RegisterCallback("mazeindex", i =>
		{
			UnityEngine.Debug.Log("call back");
			index = (int)i;
		});
		
	}

}
