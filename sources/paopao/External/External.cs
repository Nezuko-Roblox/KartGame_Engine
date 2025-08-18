using System;
using UnityEngine;

public struct External
{
	public void Initialize()
	{
		this.slip = false;
		this.dragFactor = 1f;
		this.compensationDragFactor = 1f;
		this.wheelFactor = 1f;
		this.annexForce = Vector3.zero;
		this.force = Vector3.zero;
		this.torque = Vector3.zero;
		this.upDownTime = 0f;
		this.upDownLastTime = 0f;
		this.gravityFactor = 1f;
		this.speedLimit = 0f;
	}

	public bool slip;

	public float dragFactor;

	public float compensationDragFactor;

	public float wheelFactor;

	public Vector3 annexForce;

	public Vector3 force;

	public Vector3 torque;

	public float gravityFactor;

	public float speedLimit;

	public float upDownTime;

	public float upDownLastTime;

	public float upDownInterval;

	public Vector3 upDownForce;

	public uint upDownForceIndex;

	public Vector3 liftVel;
}
