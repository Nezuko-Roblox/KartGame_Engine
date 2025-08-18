using System;

public abstract class SpeedController
{
	public abstract void Update();

	public abstract float GetDeltaTick();

	public abstract bool IsFinish();

	public abstract SpeedControllerType GetSpeedControllerType();

	public SpeedController nextSpeedController_;
}
