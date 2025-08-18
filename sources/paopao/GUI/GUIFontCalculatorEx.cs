using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIFontCalculatorEx : FontCalculatorInterface
{
	public GUIFontCalculatorEx(Rect[] font)
	{
		this.font_ = font;
	}

	public GUIFontCalculatorEx(GUIFontCalculatorExParam param)
		: this(param.widths_, param.height_, param.basePos_, param.uvRect_, param.orgRect_, param.fontString_)
	{
	}

	public GUIFontCalculatorEx(float[][] widths, float height, Vector2 basePos, Rect uvRect, Rect orgRect, string fontString)
	{
		Vector2 vector = new Vector2(uvRect.width / orgRect.width, uvRect.height / orgRect.height);
		this.revision_.x = 0.5f * vector.x;
		this.revision_.y = 0.5f * vector.y;
		List<Rect> list = new List<Rect>();
		for (int i = 0; i < widths.Length; i++)
		{
			for (int j = 0; j < widths[i].Length - 1; j++)
			{
				float num = (basePos.x + widths[i][j]) * vector.x + uvRect.x;
				float num2 = (orgRect.height - (basePos.y + height * (float)(i + 1))) * vector.y + uvRect.y;
				float num3 = (widths[i][j + 1] - widths[i][j]) * vector.x;
				float num4 = height * vector.y;
				list.Add(new Rect(num, num2, num3, num4));
			}
		}
		this.font_ = list.ToArray();
		this.uvRect_ = uvRect;
		this.orgRect_ = orgRect;
		this.fontString_ = fontString;
	}

	public bool UseRevision
	{
		get
		{
			return this.useRevision_;
		}
		set
		{
			this.useRevision_ = value;
		}
	}

	public float GetWidth(string str)
	{
		float num = 0f;
		for (int i = 0; i < str.Length; i++)
		{
			int num2 = this.fontString_.IndexOf(str[i]);
			if (num2 >= 0)
			{
				num += this.font_[num2].width;
			}
			else
			{
				num += this.font_[this.font_.Length - 1].width;
			}
		}
		return num * this.orgRect_.width / this.uvRect_.width;
	}

	public int GetIndex(char c)
	{
		return this.fontString_.IndexOf(c);
	}

	public int GetLastFontIndex()
	{
		return this.fontString_.Length - 1;
	}

	public float GetWidth(int idx)
	{
		return this.font_[idx].width * this.orgRect_.width / this.uvRect_.width;
	}

	public float GetHeight()
	{
		return this.font_[0].height * this.orgRect_.height / this.uvRect_.height;
	}

	public void SetUV(Rect uv)
	{
	}

	public bool GetUV(int charIdx, ref Vector2[] uvs, int idx)
	{
		float num = 0f;
		float num2 = 0f;
		if (this.useRevision_ && ScreenController.Instance.IsDisplayingLandscapeRight())
		{
			num = -this.revision_.x;
			num2 = -this.revision_.y;
		}
		uvs[idx] = new Vector2(this.font_[charIdx].xMin + num, this.font_[charIdx].yMax + num2);
		uvs[idx + 1] = new Vector2(this.font_[charIdx].xMax + num, this.font_[charIdx].yMax + num2);
		uvs[idx + 2] = new Vector2(this.font_[charIdx].xMin + num, this.font_[charIdx].yMin + num2);
		uvs[idx + 3] = new Vector2(this.font_[charIdx].xMax + num, this.font_[charIdx].yMin + num2);
		return true;
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < this.font_.Length; i++)
		{
			text += string.Format("[{0}] : {1}\n", i, this.font_[i]);
		}
		return text;
	}

	private Rect[] font_;

	private Rect uvRect_;

	private Rect orgRect_;

	private string fontString_ = string.Empty;

	private Vector3 revision_ = Vector3.zero;

	private bool useRevision_;
}
