using System;
using UnityEngine;

public class Cameraman
{
	public Cameraman()
	{
		for (int i = 0; i < 21; i++)
		{
			this.m_pressed[i] = false;
		}
	}

	~Cameraman()
	{
	}

	public virtual void reset(int style)
	{
	}

	public virtual int getStyle()
	{
		return 0;
	}

	public virtual string getDescription()
	{
		return string.Empty;
	}

	public virtual void calc(int tick, ref Vector3 retPos, ref Transform retOrt, ref float retFov)
	{
		if (this.m_startTick == 0)
		{
			this.m_startTick = tick;
		}
		if (this.m_lastTick == 0)
		{
			this.m_lastTick = tick;
		}
		this.m_elapse = tick - this.m_lastTick;
		this.m_lastTick = tick;
	}

	public void enableControl(bool enable)
	{
		this.m_controlActive = enable;
	}

	protected virtual void processControll()
	{
	}

	protected int getElapse()
	{
		return this.m_elapse;
	}

	protected int getTimePassed(int tick)
	{
		return (this.m_startTick != 0) ? (tick - this.m_startTick) : 0;
	}

	protected void resetStartTick()
	{
		this.m_startTick = 0;
	}

	protected bool m_controlActive;

	protected MoveSet[] m_move = new MoveSet[5];

	protected bool[] m_pressed = new bool[21];

	protected int m_startTick;

	protected int m_lastTick;

	protected int m_elapse;

	private enum Move : byte
	{
		FRONTBACK,
		LEFTRIGHT,
		YAW,
		PITCH,
		ZOOM,
		_MAXNUM
	}
}
