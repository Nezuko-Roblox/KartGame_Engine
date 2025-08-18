using System;

public struct CollisionState
{
	public void Initialize()
	{
		this.kartCollide = false;
		this.kartCollideVel = 0f;
		this.kartCollideDominant = false;
		this.shock = false;
		this.hop = false;
		this.unmovingTime = 0f;
	}

	public bool kartCollide;

	public float kartCollideVel;

	public bool kartCollideDominant;

	public bool shock;

	public float shockVel;

	public bool hop;

	public float unmovingTime;
}
