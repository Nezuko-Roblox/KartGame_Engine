using System;

public struct StuckHelper
{
	public void Initialize()
	{
		this.wallStuckTime = 0f;
		this.obstStuckTime = 0f;
		this.inStuck = false;
	}

	public float wallStuckTime;

	public float gndStuckTime;

	public float obstStuckTime;

	public bool inStuck;
}
