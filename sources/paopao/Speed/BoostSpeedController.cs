using System;

public class BoostSpeedController : StaticSpeedController
{
	public BoostSpeedController()
	{
	}

	public BoostSpeedController(float speed, float duration, float defaultDeltaTick)
		: base(speed, duration, defaultDeltaTick)
	{
	}

	public override SpeedControllerType GetSpeedControllerType()
	{
		return SpeedControllerType.BOOST_SPEED;
	}
}
