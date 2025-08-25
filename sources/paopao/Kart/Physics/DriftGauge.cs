using System;

public struct DriftGauge
{
	public void Initialize()
	{
		this.gauge = 0f;
		this.progressOn = false;
		this.progress = 0f;
		this.lastProgress = 0f;
	}

	public float gauge;

	public bool progressOn;

	public float progressTime;

	public float progress;

	public float lastProgress;
}
