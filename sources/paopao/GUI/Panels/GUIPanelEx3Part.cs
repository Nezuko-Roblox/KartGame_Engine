using System;
using UnityEngine;

public class GUIPanelEx3Part : GUIInterface, MouseNotifier
{
	public void RegistPanelManager(GUIPanelManager manager)
	{
		for (int i = 0; i < 3; i++)
		{
			manager.RegistPanel(this.parts_[i]);
		}
	}

	public bool Contains(Vector3 pos)
	{
		return (!this.useTouchRegion) ? this.rect_.Contains(pos) : this.touchRegion_.Contains(pos);
	}

	public int GetPriority()
	{
		if (this.parts_[0] == null)
		{
			return 0;
		}
		return this.parts_[0].GetPriority();
	}

	public Rect TouchRegion
	{
		get
		{
			return this.touchRegion_;
		}
		set
		{
			this.touchRegion_ = value;
			this.useTouchRegion = true;
		}
	}

	public bool SetUV(int idx)
	{
		bool flag = false;
		foreach (GUIPanelEx guipanelEx in this.parts_)
		{
			flag |= guipanelEx.SetUV(idx);
		}
		return flag;
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		foreach (GUIPanelEx guipanelEx in this.parts_)
		{
			guipanelEx.MoveRectByWindowPos(x, y);
		}
		this.rect_.xMin = this.rect_.xMin + x;
		this.rect_.xMax = this.rect_.xMax + x;
		this.rect_.yMin = this.rect_.yMin - y;
		this.rect_.yMax = this.rect_.yMax - y;
		if (this.useTouchRegion)
		{
			this.touchRegion_.xMin = this.touchRegion_.xMin + x;
			this.touchRegion_.xMax = this.touchRegion_.xMax + x;
			this.touchRegion_.yMin = this.touchRegion_.yMin + y;
			this.touchRegion_.yMax = this.touchRegion_.yMax + y;
		}
	}

	public void SetRectByWindowSpace(float x, float y)
	{
		float num = x - this.parts_[0].Panel.left_;
		float num2 = y - ((float)Screen.height - this.parts_[0].Panel.top_);
		this.MoveRectByWindowPos(num, num2);
	}

	public Vector2 GetLeftTopByWindowPos()
	{
		return this.parts_[0].GetLeftTopByWindowPos();
	}

	public virtual void ResizeByWindowSpace(float xMin, float xMax)
	{
	}

	public override string ToString()
	{
		string text = string.Empty;
		foreach (GUIPanelEx guipanelEx in this.parts_)
		{
			text = text + guipanelEx.ToString() + "\n\n";
		}
		return text;
	}

	public bool IsEnabled()
	{
		return this.parts_[0].IsEnabled();
	}

	public bool Visible
	{
		get
		{
			return this.parts_[0].Visible;
		}
		set
		{
			foreach (GUIPanelEx guipanelEx in this.parts_)
			{
				guipanelEx.Visible = value;
			}
		}
	}

	protected GUIPanelEx[] parts_ = new GUIPanelEx[3];

	protected Rect rect_ = RectHelper.zero;

	protected float minRange_;

	protected Rect touchRegion_ = RectHelper.zero;

	protected bool useTouchRegion;
}
