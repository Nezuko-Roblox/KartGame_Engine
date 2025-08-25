using System;
using UnityEngine;

public class LerpSpeedController : SpeedController
{
	public LerpSpeedController()
	{
		this.duration_ = 0f;
		this.accum_ = 0f;
		this.speed_ = 0f;
		this.from_ = 0f;
		this.to_ = 0f;
	}

	public LerpSpeedController(float from, float to, float duration)
	{
		this.duration_ = duration;
		this.from_ = from;
		this.to_ = to;
		this.accum_ = 0f;
		this.speed_ = 0f;
	}

	public override SpeedControllerType GetSpeedControllerType()
	{
		return SpeedControllerType.LERP;
	}

	public override void Update()
	{
		this.accum_ += Time.deltaTime;
		this.speed_ = Mathf.Lerp(this.from_, this.to_, this.accum_ / this.duration_);
	}

	public override float GetDeltaTick()
	{
		return this.speed_;
	}

	public override bool IsFinish()
	{
		return !MathHelper.IsBetweenII(this.accum_, 0f, this.duration_);
	}

	public float duration_;

	public float accum_;

	public float speed_;

	public float from_;

	public float to_;
}
