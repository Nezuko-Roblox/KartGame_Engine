using System;
using UnityEngine;

public class GUIButton : GUIInterface, MouseNotifier
{
	public GUIButton(int type, float[] f, FiaTexture tex, int layer, Vector3 backGap)
	{
		this.Initialize(type, f, tex, layer, backGap, GUIFontCalculator.DEFAULT_GAP);
	}

	public GUIButton(int type, float[] f, FiaTexture tex, int layer, Vector3 backGap, Vector3 nameGap)
	{
		this.Initialize(type, f, tex, layer, backGap, nameGap);
	}

	public void SetTouchRegion(Rect rect_)
	{
		this.back_.TouchRegion = rect_;
	}

	public void SetTouchRegionByWindowRect(Rect rect_)
	{
		rect_.y = (float)Screen.height - rect_.y - rect_.height;
		this.SetTouchRegion(rect_);
	}

	public void SetTouchRegionByWindowPos(float _left, float _top, float _right, float _bottom)
	{
		Rect rect = new Rect(_left, _top, _right - _left, _bottom - _top);
		rect.y = (float)Screen.height - rect.y - rect.height;
		this.back_.TouchRegion = rect;
	}

	protected void Initialize(int type, float[] f, FiaTexture tex, int layer, Vector3 backGap, Vector3 nameGap)
	{
		if (f.Length == 17)
		{
			float[] array = new float[13];
			Array.Copy(f, array, 13);
			this.back_ = new GUIPanelEx3PartHorz(type, array, tex, layer + 1, backGap, 3f);
			Vector2 vector = new Vector2((f[0] + f[2] - (f[15] - f[13])) * 0.5f, (f[1] + f[3] - (f[16] - f[14])) * 0.5f);
			float[] array2 = new float[]
			{
				vector.x,
				vector.y,
				f[13],
				f[14],
				f[15],
				f[16]
			};
			this.name_ = GUIPanelFactory.Instance.CreateByWindowSpace(type, array2, tex, layer, nameGap);
			return;
		}
		if (f.Length == 11)
		{
			float[] array3 = new float[7];
			Array.Copy(f, array3, 7);
			this.back_ = new GUIPanelEx3PartHorz(type, array3, tex, layer + 1, backGap, 3f);
			Vector2 vector2 = new Vector2((f[0] + f[2] - (f[9] - f[7])) * 0.5f, (f[1] + f[3] - (f[10] - f[8])) * 0.5f);
			float[] array4 = new float[]
			{
				vector2.x * (float)Screen.width / 800f,
				vector2.y * (float)Screen.height / 480f,
				(f[9] - f[7] + vector2.x) * (float)Screen.width / 800f,
				(f[10] - f[8] + vector2.y) * (float)Screen.height / 480f,
				f[7],
				f[8],
				f[9],
				f[10]
			};
			this.name_ = GUIPanelFactory.Instance.CreateByWindowSpace(type, array4, tex, layer, nameGap);
		}
	}

	public void SetNameUV(int uv)
	{
		this.name_.SetUV(uv);
	}

	public int GetNameUV()
	{
		return this.name_.UV;
	}

	public bool SetPushed(bool isPushed)
	{
		return this.back_.SetUV((!isPushed) ? 0 : 1);
	}

	public void RegistPanelManager(GUIPanelManager manager)
	{
		this.back_.RegistPanelManager(manager);
		manager.RegistPanel(this.name_);
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.name_.MoveRectByWindowPos(x, y);
	}

	public void SetRectByWindowSpace(float x, float y)
	{
		Vector2 leftTopByWindowPos = this.GetLeftTopByWindowPos();
		this.MoveRectByWindowPos(x - leftTopByWindowPos.x, y - leftTopByWindowPos.y);
	}

	public Vector2 GetLeftTopByWindowPos()
	{
		return this.back_.GetLeftTopByWindowPos();
	}

	public bool Contains(Vector3 pos)
	{
		return this.back_.Contains(pos);
	}

	public int GetPriority()
	{
		if (this.back_ == null)
		{
			return 0;
		}
		return this.back_.GetPriority();
	}

	public bool IsEnabled()
	{
		return this.back_.IsEnabled();
	}

	public override string ToString()
	{
		return this.back_.ToString() + "\n\n" + this.name_.ToString();
	}

	public bool Visible
	{
		get
		{
			return this.back_.Visible;
		}
		set
		{
			this.back_.Visible = value;
			this.name_.Visible = value;
		}
	}

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx name_;
}
