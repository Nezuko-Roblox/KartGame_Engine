using System;
using UnityEngine;

public class InputManager
{
	public void InputUpdate()
	{
		for (int i = 0; i < 6; i++)
		{
			this.keyArray_[i].InputUpdate();
		}
		if (this.keyArray_[0].keyPushTime_ > this.keyArray_[1].keyPushTime_)
		{
			this.keyArray_[1].keyPushTime_ = 0f;
		}
		else if (this.keyArray_[0].keyPushTime_ < this.keyArray_[1].keyPushTime_)
		{
			this.keyArray_[0].keyPushTime_ = 0f;
		}
		else
		{
			this.keyArray_[0].keyPushTime_ = 0f;
			this.keyArray_[1].keyPushTime_ = 0f;
		}
	}

	public float GetKeyPushTime(InputType type)
	{
		return this.keyArray_[(int)type].keyPushTime_;
	}

	public KeyState GetKeyState(InputType type)
	{
		return this.keyArray_[(int)type].GetKeyState();
	}

	private basicInput[] keyArray_ = new basicInput[]
	{
		new basicInput(0f, KeyCode.LeftArrow),
		new basicInput(0f, KeyCode.RightArrow),
		new basicInput(0f, KeyCode.UpArrow),
		new basicInput(0f, KeyCode.DownArrow),
		new basicInput(0f, KeyCode.LeftShift),
		new basicInput(0f, KeyCode.LeftControl)
	};
}
