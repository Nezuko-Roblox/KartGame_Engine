using System;

public class JobTimeOver : KartJob
{
	public JobTimeOver(float driveEndTime)
	{
		this.type_ = KartJobType.TIME_OVER;
		this.driveEndTime_ = driveEndTime;
	}

	public float GetDriveEndTime()
	{
		return this.driveEndTime_;
	}

	private float driveEndTime_;
}
