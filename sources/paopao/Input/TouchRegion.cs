using System;
using UnityEngine;

public class TouchRegion
{
	public TouchRegion()
	{
		this.isPressed_ = false;
	}

	public Rect Region
	{
		get
		{
			return this.rc_;
		}
		set
		{
			this.rc_ = value;
			this.isPressed_ = false;
		}
	}

	public bool Pressed
	{
		get
		{
			return this.isPressed_;
		}
		set
		{
			this.isPressed_ = value;
		}
	}

	private Rect rc_;

	private bool isPressed_;
}
