using System;
using UnityEngine;

public class TouchController
{
	public TouchController()
	{
		this.inputStatus_ = new InputStatus[] { this.accel_, this.break_, this.drift_, this.driftL_, this.driftR_, this.left_, this.right_, this.boost_, this.item1_, this.item2_ };
	}

	public InputStatus GetInputStatus(InputStatusEnum e)
	{
		return (e != InputStatusEnum.NONE) ? this.inputStatus_[(int)e] : null;
	}

	public void InputUpdate()
	{
		this.InputStatusUpdate();
		this.AutomaticAccelInputUpdate();
		this.UpdateShaking();
	}

	public bool GetAccel()
	{
		return this.accel_.pressed_;
	}

	public bool GetBreak()
	{
		return !this.accel_.pressed_ && this.break_.pressed_;
	}

	public float GetDirection()
	{
		if (this.analogSteer_)
		{
			float y = Input.acceleration.y;
			if (y > this.deadZone_)
			{
				return Math.Max(-1f, -1f * this.steerMultiplier_ * y);
			}
			if (y < -this.deadZone_)
			{
				return Math.Min(1f, -1f * this.steerMultiplier_ * y);
			}
			return 0f;
		}
		else
		{
			float num = Mathf.Max(this.left_.pressedAt_, this.driftL_.pressedAt_);
			float num2 = Mathf.Max(this.right_.pressedAt_, this.driftR_.pressedAt_);
			if (num == 0f && num2 == 0f)
			{
				return 0f;
			}
			return (num < num2) ? 1f : (-1f);
		}
	}

	public bool GetDrift()
	{
		return this.drift_.pressed_ || this.driftL_.pressed_ || this.driftR_.pressed_;
	}

	public bool GetItem()
	{
		return this.item1_.Released() || this.item2_.Released();
	}

	public int GetShakeCount()
	{
		return this.shakeCount_;
	}

	public bool CheckShake
	{
		get
		{
			return this.isCheckShake_;
		}
		set
		{
			this.isCheckShake_ = value;
			if (this.isCheckShake_)
			{
				this.targetAcceleration_ = new Vector3(0.1f, 0.1f, 0.1f);
				this.shakeCount_ = 0;
			}
		}
	}

	protected void UpdateShaking()
	{
		if (!this.isCheckShake_)
		{
			return;
		}
		Vector3 acceleration = Input.acceleration;
		for (int i = 0; i < 3; i++)
		{
			if (Mathf.Abs(this.targetAcceleration_[i]) < acceleration[i] && this.targetAcceleration_[i] * acceleration[i] > 0f)
			{
				this.shakeCount_++;
				ref Vector3 ptr = ref this.targetAcceleration_;
				int num2;
				int num = (num2 = i);
				float num3 = ptr[num2];
				this.targetAcceleration_[num] = num3 * -1f;
			}
		}
	}

	public void ResetShaking()
	{
		this.shakeCount_ = 0;
		this.isCheckShake_ = false;
	}

	protected virtual void InputStatusUpdate()
	{
		if (this.buttons_ != null)
		{
			foreach (TouchRegion touchRegion in this.buttons_)
			{
				touchRegion.Pressed = false;
			}
			if (Input.touchCount > 0)
			{
				foreach (Touch touch in Input.touches)
				{
					Vector2 position = touch.position;
					if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
					{
						float num = (float)Screen.width - touch.position.x;
						float num2 = (float)Screen.height - touch.position.y;
						position = new Vector2(num, num2);
					}
					for (int k = 0; k < this.buttons_.Length; k++)
					{
						if (!this.buttons_[k].Pressed && this.buttons_[k].Region.Contains(position))
						{
							this.buttons_[k].Pressed = true;
						}
					}
				}
			}
			if (this.inputs_ != null)
			{
				int num3 = this.inputs_.Length;
				for (int l = 0; l < num3; l++)
				{
					this.inputs_[l].Backup();
					this.inputs_[l].pressed_ = false;
				}
				for (int m = 0; m < this.buttons_.Length; m++)
				{
					if (this.buttons_[m].Pressed)
					{
						this.inputs_[m].Press();
					}
				}
				for (int n = 0; n < num3; n++)
				{
					if (!this.inputs_[n].pressed_)
					{
						this.inputs_[n].Reset();
					}
				}
			}
		}
	}

	protected virtual void AutomaticAccelInputUpdate()
	{
		if (this.automaticAccel_)
		{
			if (!this.break_.Pressed())
			{
				this.accel_.Press();
			}
			else
			{
				this.accel_.Reset();
			}
		}
	}

	protected TouchRegion accelB_ = new TouchRegion();

	protected TouchRegion breakB_ = new TouchRegion();

	protected TouchRegion driftLB_ = new TouchRegion();

	protected TouchRegion driftRB_ = new TouchRegion();

	protected TouchRegion leftB_ = new TouchRegion();

	protected TouchRegion rightB_ = new TouchRegion();

	protected TouchRegion item1B_ = new TouchRegion();

	protected TouchRegion item2B_ = new TouchRegion();

	protected InputStatus accel_ = new InputStatus();

	protected InputStatus break_ = new InputStatus();

	protected InputStatus drift_ = new InputStatus();

	protected InputStatus driftL_ = new InputStatus();

	protected InputStatus driftR_ = new InputStatus();

	protected InputStatus left_ = new InputStatus();

	protected InputStatus right_ = new InputStatus();

	protected InputStatus boost_ = new InputStatus();

	protected InputStatus item1_ = new InputStatus();

	protected InputStatus item2_ = new InputStatus();

	protected float direction_;

	public bool automaticAccel_;

	public bool analogSteer_;

	public float deadZone_ = 0.1f;

	public float steerMultiplier_ = 2.86f;

	protected TouchRegion[] buttons_;

	protected InputStatus[] inputs_;

	private InputStatus[] inputStatus_;

	private Vector3 targetAcceleration_ = new Vector3(0.1f, 0.1f, 0.1f);

	private int shakeCount_;

	private bool isCheckShake_;
}
