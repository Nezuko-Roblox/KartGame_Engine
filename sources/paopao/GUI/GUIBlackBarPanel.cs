using System;
using UnityEngine;

public class GUIBlackBarPanel : GUIPanelEx
{
	public GUIBlackBarPanel(float[] f)
	{
		if (f.Length == 4)
		{
			this.panel_ = new GUIPanel(f[0], f[1], f[2], f[3], 1);
			this.calc_ = new GUIFontCalculator(RectHelper.zero, RectHelper.zero, Vector2.zero, Vector2.zero, GUIFontCalculator.DEFAULT_GAP);
			this.color_ = Color.black;
			this.value_ = 0f;
			this.max_ = (float)Screen.height * 0.09375f + 0.5f;
			this.speed_ = this.max_ / 0.375f;
		}
	}

	public float value_;

	public float max_;

	public float speed_;
}
