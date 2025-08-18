using System;

public class JobFinishNotices : KartJob
{
	public JobFinishNotices(float driveEndTime)
	{
		this.type_ = KartJobType.FINISH_NOTICES;
		this.driveEndTime_ = driveEndTime;
	}

	public float GetDriveEndTime()
	{
		return this.driveEndTime_;
	}

	private float driveEndTime_;
}
