using System;

public struct Control
{
	public float getRealAccel()
	{
		return (!this.accelBrakeSwap) ? this.accel : this.brake;
	}

	public float getRealBrake()
	{
		return (!this.accelBrakeSwap) ? this.brake : this.accel;
	}

	public float getRealSteer()
	{
		return ((!this.wheelFlip && !this.wheelDevil) ? 1f : (-1f)) * this.steer;
	}

	public void Initialize()
	{
		this.accel = 0f;
		this.brake = 0f;
		this.accelBrakeSwap = false;
		this.steer = 0f;
		this.wheelFlip = false;
		this.wheelDevil = false;
		this.stayTime = 0f;
		this.steerAngle = 0f;
		this.oldSteerAngle = 0f;
	}

	public float accel;

	public float brake;

	public bool accelBrakeSwap;

	public float steer;

	public bool wheelFlip;

	public bool wheelDevil;

	public float stayTime;

	public float steerAngle;

	public float oldSteerAngle;
}
