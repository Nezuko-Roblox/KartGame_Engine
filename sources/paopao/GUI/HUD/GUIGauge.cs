using System;
using UnityEngine;

public class GUIGauge
{
	public GUIGauge(GUIPanelEx gui)
	{
		this.gui_ = gui;
		this.origin_ = this.gui_.Panel.GetRectWS();
	}

	public void SetFactor(float factor)
	{
		float num = this.origin_.width * factor;
		this.gui_.SetRectByWindowSpace(this.origin_.xMin, this.origin_.yMin, this.origin_.xMin + num, this.origin_.yMax);
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		this.gui_.MoveRectByWindowPos(x, y);
		this.origin_.xMin = this.origin_.xMin + x;
		this.origin_.xMax = this.origin_.xMax + x;
		this.origin_.yMin = this.origin_.yMin + y;
		this.origin_.yMax = this.origin_.yMax + y;
	}

	public void SetColor(Color clr)
	{
		this.gui_.VerticeColor = clr;
	}

	public GUIPanelEx Panel
	{
		get
		{
			return this.gui_;
		}
	}

	private GUIPanelEx gui_;

	private Rect origin_;
}
