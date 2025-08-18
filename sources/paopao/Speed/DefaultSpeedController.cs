using System;

public class DefaultSpeedController : SpeedController
{
	public DefaultSpeedController(int kartIndex, AIControllerType aiType, float defaultDeltaTick)
	{
		this.defaultDeltaTick_ = defaultDeltaTick;
		this.deltaTick_ = this.defaultDeltaTick_;
		this.controlDeltaTick_ = defaultDeltaTick / 3f;
	}

	public override SpeedControllerType GetSpeedControllerType()
	{
		return SpeedControllerType.DEFAULT;
	}

	public override float GetDeltaTick()
	{
		return this.deltaTick_;
	}

	public override void Update()
	{
		if (this.deltaTick_ != this.defaultDeltaTick_)
		{
			if (MathHelper.IsBetweenII(this.deltaTick_ - this.defaultDeltaTick_, -this.controlDeltaTick_, this.controlDeltaTick_))
			{
				this.deltaTick_ = this.defaultDeltaTick_;
			}
			else
			{
				this.deltaTick_ += ((this.deltaTick_ <= this.defaultDeltaTick_) ? this.controlDeltaTick_ : (-this.controlDeltaTick_));
			}
		}
	}

	public override bool IsFinish()
	{
		return false;
	}

	public void Reset(float deltaTick)
	{
		this.deltaTick_ = deltaTick;
	}

	private float defaultDeltaTick_;

	private float deltaTick_;

	private float controlDeltaTick_;
}
