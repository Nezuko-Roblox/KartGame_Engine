using System;

public struct AdBoost
{
	public void Initialize()
	{
		this.validTrigger = false;
		this.validTime = 0f;
		this.useLeftTime = 0f;
	}

	public bool validTrigger;

	public float validTime;

	public float useLeftTime;
}
