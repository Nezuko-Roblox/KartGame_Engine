using System;
using UnityEngine;

public class GUIMinimapPanel : GUIPanelEx
{
	public GUIMinimapPanel(float[] f)
	{
		if (f.Length == 4)
		{
			this.panel_ = new GUIPanel(f[0], f[1], f[2], f[3], 1);
			this.calc_ = new GUIFontCalculator(RectHelper.zero, RectHelper.zero, Vector2.zero, Vector2.zero, GUIFontCalculator.DEFAULT_GAP);
			this.color_ = new Color(0f, 0f, 0f, 0.5f);
		}
	}
}
