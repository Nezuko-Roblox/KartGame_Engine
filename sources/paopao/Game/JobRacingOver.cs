using System;

public class JobRacingOver : KartJob
{
	public JobRacingOver(float raceOverTime)
	{
		this.type_ = KartJobType.RACE_OVER;
		this.raceOverTime_ = raceOverTime;
	}

	public float GetRaceOverTime()
	{
		return this.raceOverTime_;
	}

	private float raceOverTime_;
}
