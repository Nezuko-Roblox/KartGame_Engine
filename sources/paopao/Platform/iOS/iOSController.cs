using System;
using UnityEngine;

public class iOSController : TouchController
{
	public static iOSController Instance
	{
		get
		{
			if (iOSController.instance_ == null)
			{
				iOSController.instance_ = new iOSController();
			}
			return iOSController.instance_;
		}
	}

	private Rect CalcControlButtonRect(int x, int y)
	{
		return new Rect((float)x, (float)(Screen.height - y - 66), 64f, 66f);
	}

	private Rect CalcControlButtonRect(float left_, float top_, float right_, float bottom_)
	{
		return new Rect(left_, (float)Screen.height - bottom_, right_ - left_, bottom_ - top_);
	}

	public iOSControllerType Type
	{
		get
		{
			return this.type_;
		}
		set
		{
			this.type_ = value;
			switch (this.type_)
			{
			case iOSControllerType.iPhone1_1:
				this.breakB_.Region = this.CalcControlButtonRect(680f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f, (float)Screen.width, (float)Screen.height);
				this.driftLB_.Region = this.CalcControlButtonRect(0f, 368f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, (float)Screen.height);
				this.item1B_.Region = this.CalcControlButtonRect(0f, 148f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, 260f * (float)Screen.height / 480f);
				this.automaticAccel_ = true;
				this.analogSteer_ = true;
				this.buttons_ = new TouchRegion[] { this.breakB_, this.driftLB_, this.item1B_ };
				this.inputs_ = new InputStatus[] { this.break_, this.drift_, this.item1_ };
				break;
			case iOSControllerType.iPhone1_3:
				this.breakB_.Region = this.CalcControlButtonRect(576f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f, 702f * (float)Screen.width / 800f, (float)Screen.height);
				this.driftLB_.Region = this.CalcControlButtonRect(0f, 260f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f);
				this.driftRB_.Region = this.CalcControlButtonRect(680f * (float)Screen.width / 800f, 260f * (float)Screen.height / 480f, (float)Screen.width, 368f * (float)Screen.height / 480f);
				this.leftB_.Region = this.CalcControlButtonRect(0f, 368f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, (float)Screen.height);
				this.rightB_.Region = this.CalcControlButtonRect(702f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f, (float)Screen.width, (float)Screen.height);
				this.item1B_.Region = this.CalcControlButtonRect(0f, 148f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, 260f * (float)Screen.height / 480f);
				this.automaticAccel_ = true;
				this.analogSteer_ = false;
				this.buttons_ = new TouchRegion[] { this.breakB_, this.driftLB_, this.driftRB_, this.item1B_, this.leftB_, this.rightB_ };
				this.inputs_ = new InputStatus[] { this.break_, this.drift_, this.drift_, this.item1_, this.left_, this.right_ };
				break;
			case iOSControllerType.iPhone2:
				this.breakB_.Region = this.CalcControlButtonRect(576f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f, 702f * (float)Screen.width / 800f, (float)Screen.height);
				this.driftLB_.Region = this.CalcControlButtonRect(0f, 260f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f);
				this.driftRB_.Region = this.CalcControlButtonRect(680f * (float)Screen.width / 800f, 260f * (float)Screen.height / 480f, (float)Screen.width, 368f * (float)Screen.height / 480f);
				this.leftB_.Region = this.CalcControlButtonRect(0f, 368f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, (float)Screen.height);
				this.rightB_.Region = this.CalcControlButtonRect(702f * (float)Screen.width / 800f, 368f * (float)Screen.height / 480f, (float)Screen.width, (float)Screen.height);
				this.item1B_.Region = this.CalcControlButtonRect(0f, 148f * (float)Screen.height / 480f, 120f * (float)Screen.width / 800f, 260f * (float)Screen.height / 480f);
				this.automaticAccel_ = true;
				this.analogSteer_ = false;
				this.buttons_ = new TouchRegion[] { this.breakB_, this.driftLB_, this.driftRB_, this.item1B_, this.leftB_, this.rightB_ };
				this.inputs_ = new InputStatus[] { this.break_, this.driftL_, this.driftR_, this.item1_, this.left_, this.right_ };
				break;
			}
			int num = this.buttons_.Length;
			for (int i = 0; i < num; i++)
			{
				this.buttons_[i].Pressed = false;
				this.inputs_[i].Reset();
			}
		}
	}

	public const int IPHONE_BEGIN = 0;

	public const int IPHONE_END = 2;

	public const int IPAD_BEGIN = 3;

	public const int IPAD_END = 5;

	public const int IPHONE_DEFAULT = 2;

	public const int IPAD_DEFAULT = 3;

	private const int CONTROL_BUTTON_WIDTH = 64;

	private const int CONTROL_BUTTON_HEIGHT = 66;

	public static iOSController instance_;

	private iOSControllerType type_;
}
