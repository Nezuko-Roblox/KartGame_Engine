using System;
using UnityEngine;

public struct basicInput
{
	public basicInput(float time, KeyCode code1)
	{
		this.keyPushTime_ = 0f;
		this.keyCode_ = code1;
		this.keyState_ = KeyState.NONE;
	}

	public void InputUpdate()
	{
		if (this.keyCode_ == KeyCode.UpArrow)
		{
			if (Input.acceleration.x > -0.3f)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		else if (this.keyCode_ == KeyCode.DownArrow)
		{
			if (Input.acceleration.x < -0.8f)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		else if (this.keyCode_ == KeyCode.LeftArrow)
		{
			if (Input.acceleration.y > 0.4f)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		else if (this.keyCode_ == KeyCode.RightArrow)
		{
			if (Input.acceleration.y < -0.4f)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		if (this.keyCode_ == KeyCode.LeftShift)
		{
			if (Input.touchCount == 1)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		if (this.keyCode_ == KeyCode.LeftControl)
		{
			if (Input.touchCount == 2)
			{
				this.keyPushTime_ = Time.time;
			}
			else
			{
				this.keyPushTime_ = 0f;
			}
		}
		this.keyState_ = KeyStateTransfer.GetKeyState(this.keyState_, this.keyPushTime_ > 0f);
	}

	public KeyState GetKeyState()
	{
		return this.keyState_;
	}

	public float keyPushTime_;

	public KeyCode keyCode_;

	private KeyState keyState_;
}
