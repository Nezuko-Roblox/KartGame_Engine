using System;
using UnityEngine;

public class GUIPosMoverWS
{
	public GUIPosMoverWS(GUIInterface gui, Vector2 distance)
	{
		this.gui_ = gui;
		this.distance_ = distance;
		this.originalPos_ = gui.GetLeftTopByWindowPos();
	}

	public void SetPos(int i)
	{
		Vector2 vector = this.originalPos_ + (float)i * this.distance_;
		this.gui_.SetRectByWindowSpace(vector.x, vector.y);
	}

	public GUIInterface Panel
	{
		get
		{
			return this.gui_;
		}
	}

	private GUIInterface gui_;

	private Vector2 distance_;

	private Vector2 originalPos_;
}
