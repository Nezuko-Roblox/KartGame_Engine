using System;
using UnityEngine;

public class WaterMissilePath
{
	public WaterMissilePath(Vector3 src, Vector3 dst, float t)
	{
		this.src_ = src;
		this.dst_ = dst;
		this.duration_ = t;
		this.tick_ = 0f;
	}

	public void Update(float t)
	{
		this.tick_ += t;
	}

	public Vector3 GetPath()
	{
		return Vector3.Lerp(this.src_, this.dst_, this.tick_ / this.duration_);
	}

	public bool IsFinish()
	{
		return this.duration_ < this.tick_;
	}

	public Vector3 src_;

	public Vector3 dst_;

	public float duration_;

	public float tick_;
}
