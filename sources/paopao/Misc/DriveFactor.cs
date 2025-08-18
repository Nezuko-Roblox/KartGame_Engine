using System;

public struct DriveFactor
{
	public void Initialize()
	{
		this.frontGripFactor = 0f;
		this.rearGripFactor = 0f;
		this.driftSlipFactor = 1f;
		this.backFrontGripFactor = 0f;
		this.backRearGripFactor = 0f;
		this.backDriftSlipFactor = 1f;
		this.betaCut = 1f;
		this.onDriftSteerFactor = 1f;
		this.onRestTimeSteerFactor = 1f;
		this.onTriggerSteerFactor = 1f;
		this.speedLimit = 120f;
	}

	public float speedLimit;

	public float frontGripFactor;

	public float rearGripFactor;

	public float driftSlipFactor;

	public float backFrontGripFactor;

	public float backRearGripFactor;

	public float backDriftSlipFactor;

	public float betaCut;

	public float onTriggerSteerFactor;

	public float onDriftSteerFactor;

	public float onRestTimeSteerFactor;
}
