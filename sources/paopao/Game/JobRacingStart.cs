using System;

public class JobRacingStart : KartJob
{
	public JobRacingStart()
		: this(10f)
	{
	}

	public JobRacingStart(float racingStartTime)
	{
		this.type_ = KartJobType.RACING_START;
		this.startTime_ = racingStartTime;
	}

	public float GetStartTime()
	{
		return this.startTime_;
	}

	public const float DEFAULT_START_TIME = 10f;

	private float startTime_;
}
