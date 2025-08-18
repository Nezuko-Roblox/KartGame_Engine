using System;
using UnityEngine;

public class GUIFontCalculatorEx2 : FontCalculatorInterface
{
	public GUIFontCalculatorEx2(float[] widthArray, float fontHeight)
	{
		this.widthArray_ = widthArray;
		this.height_ = fontHeight;
	}

	public void SetUV(Rect uv)
	{
	}

	public float GetWidth(int idx)
	{
		return this.widthArray_[idx];
	}

	public float GetHeight()
	{
		return this.height_;
	}

	public bool GetUV(int charIdx, ref Vector2[] uvs, int idx)
	{
		float num = (4f + (float)(idx % 10) * 102f) / 1024f;
		float num2 = (30f + (float)(9 - idx / 10) * 102f) / 1024f;
		float num3 = this.widthArray_[charIdx] / 1024f;
		float num4 = this.height_ / 1024f;
		uvs[idx] = new Vector2(num, num2 + num4);
		uvs[idx + 1] = new Vector2(num + num3, num2 + num4);
		uvs[idx + 2] = new Vector2(num, num2);
		uvs[idx + 3] = new Vector2(num + num3, num2);
		return true;
	}

	public const string FONT_STRING = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~?????";

	private const float X_POS = 4f;

	private const float Y_POS = 30f;

	private const float X_GAP = 102f;

	private const float Y_GAP = 102f;

	private const int X_NUM = 10;

	private float[] widthArray_;

	private float height_;
}
