using System;

public class MoveSet
{
	private MoveSet()
	{
	}

	public void setEnvironment(float maxVel, float friction)
	{
		this.m_maxVel = maxVel;
		this.m_friction = friction;
	}

	public void setAccel(float accel)
	{
		this.m_accel = accel;
	}

	public void update()
	{
		if ((this.m_accel > 0f && this.m_vel + this.m_accel < this.m_maxVel) || (this.m_accel < 0f && this.m_vel + this.m_accel > -this.m_maxVel))
		{
			this.m_vel += this.m_accel;
		}
		if (this.m_vel - this.m_friction > 0f)
		{
			this.m_vel -= this.m_friction;
		}
		else if (this.m_vel + this.m_friction < 0f)
		{
			this.m_vel += this.m_friction;
		}
		else
		{
			this.m_vel = 0f;
		}
		this.m_accel = 0f;
	}

	public float getResultVel()
	{
		return this.m_vel;
	}

	public void setResultVelDirectly(float vel)
	{
		this.m_vel = vel;
	}

	public void reset()
	{
		this.m_accel = 0f;
		this.m_vel = 0f;
	}

	protected float m_accel;

	protected float m_vel;

	protected float m_maxVel;

	protected float m_friction;
}
