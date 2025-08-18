using System;
using UnityEngine;

public class MaintainedLerpSpeedController : SpeedController
{
	public MaintainedLerpSpeedController(float from, float to, float lerpDuration, float maintainDuration)
	{
		this.from_ = from;
		this.to_ = to;
		this.lerpDuration_ = lerpDuration;
		this.maintainDuration_ = maintainDuration;
	}

	public override SpeedControllerType GetSpeedControllerType()
	{
		return SpeedControllerType.MAINTAINED_LERP;
	}

	public override void Update()
	{
		this.accum_ += Time.deltaTime;
		if (this.accum_ < this.lerpDuration_)
		{
			this.speed_ = Mathf.Lerp(this.from_, this.to_, this.accum_ / this.lerpDuration_);
		}
	}

	public override float GetDeltaTick()
	{
		return this.speed_;
	}

	public override bool IsFinish()
	{
		return !MathHelper.IsBetweenII(this.accum_, 0f, this.lerpDuration_ + this.maintainDuration_);
	}

	private float accum_;

	private float lerpDuration_;

	private float maintainDuration_;

	private float speed_;

	private float from_;

	private float to_;
}
