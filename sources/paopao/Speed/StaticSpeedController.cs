using System;
using UnityEngine;

public class StaticSpeedController : SpeedController
{
	public StaticSpeedController()
	{
	}

	public StaticSpeedController(float speed, float duration, float defaultDeltaTick)
	{
		this.duration_ = duration;
		this.speed_ = speed;
		this.defaultDeltaTick_ = defaultDeltaTick;
	}

	public override SpeedControllerType GetSpeedControllerType()
	{
		return SpeedControllerType.STATIC_SPEED;
	}

	public override void Update()
	{
		this.duration_ -= Time.deltaTime;
	}

	public override float GetDeltaTick()
	{
		return (this.duration_ < 0f) ? this.defaultDeltaTick_ : this.speed_;
	}

	public override bool IsFinish()
	{
		return this.duration_ <= 0f;
	}

	protected float speed_;

	protected float duration_;

	protected float defaultDeltaTick_;
}
