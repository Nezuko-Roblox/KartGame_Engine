using System;
using UnityEngine;

public class InputStatus
{
	public void Backup()
	{
		this.prevPressed_ = this.pressed_;
	}

	public void Press()
	{
		if (this.pressedAt_ <= 0f)
		{
			this.pressedAt_ = Time.time;
		}
		this.pressed_ = true;
	}

	public bool Pressed()
	{
		return this.pressed_;
	}

	public bool Released()
	{
		return this.prevPressed_ && !this.pressed_;
	}

	public bool Pushed()
	{
		return !this.prevPressed_ && this.pressed_;
	}

	public void Reset()
	{
		this.pressed_ = false;
		this.pressedAt_ = 0f;
	}

	public bool pressed_;

	public float pressedAt_;

	public bool prevPressed_;
}
