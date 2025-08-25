using System;
using UnityEngine;

public class GUIFontCalculator : FontCalculatorInterface
{
	public GUIFontCalculator(FiaTexture tex, Vector2 fontSize, Vector2 fontPos)
	{
		this.Initialize(tex.UV, tex.OrgRect, fontSize, fontPos, GUIFontCalculator.DEFAULT_GAP);
	}

	public GUIFontCalculator(Rect uvRect, Rect orgRect, Vector2 fontSize, Vector2 fontPOs)
	{
		this.Initialize(uvRect, orgRect, fontSize, fontPOs, GUIFontCalculator.DEFAULT_GAP);
	}

	public GUIFontCalculator(Rect uvRect, Rect orgRect, Vector2 fontSize, Vector2 fontPOs, Vector3 fontGap)
	{
		this.Initialize(uvRect, orgRect, fontSize, fontPOs, fontGap);
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

	public override string ToString()
	{
		return this.font_.ToString() + " " + this.fontGap_.ToString();
	}

	private void Initialize(Rect uvRect, Rect orgRect, Vector2 fontSize, Vector2 fontPos, Vector3 fontGap)
	{
		if (!RectHelper.IsValidRect(orgRect))
		{
			return;
		}
		Vector2 vector = new Vector2(uvRect.width / orgRect.width, uvRect.height / orgRect.height);
		this.fontGap_.x = fontGap.x * vector.x;
		this.fontGap_.y = fontGap.y * vector.y;
		this.fontGap_.z = fontGap.z;
		this.font_.x = fontPos.x * vector.x + uvRect.x;
		this.font_.y = (fontPos.y - fontSize.y) * vector.y + uvRect.y;
		this.revision_.x = 0.5f * vector.x;
		this.revision_.y = 0.5f * vector.y;
		this.font_.width = fontSize.x * vector.x;
		this.font_.height = fontSize.y * vector.y;
	}

	public static GUIFontCalculator CreateByWindowSpace(FiaTexture tex, Vector2 fontSize, Vector2 fontPos)
	{
		Vector2 vector = new Vector2(fontPos.x, tex.OrgRect.yMax - fontPos.y);
		return new GUIFontCalculator(tex, fontSize, vector);
	}

	public static GUIFontCalculator CreateByWindowSpace(FiaTexture tex, Vector2 fontSize, Vector2 fontPos, Vector3 fontGap)
	{
		Vector2 vector = new Vector2(fontPos.x, tex.OrgRect.yMax - fontPos.y);
		return new GUIFontCalculator(tex.UV, tex.OrgRect, fontSize, vector, fontGap);
	}

	public void SetUV(Rect uv)
	{
		this.font_ = uv;
	}

	private void GetLeftBottom(int charIdx, out float x, out float y)
	{
		int num = (int)this.fontGap_.z;
		if (num >= 100)
		{
			int num2 = num - 100;
			int num3;
			int num4;
			if (charIdx < 0)
			{
				num3 = (charIdx + 1) / num2 - 1;
				num4 = num2 - 1 + (charIdx + 1) % num2;
			}
			else
			{
				num3 = charIdx / num2;
				num4 = charIdx % num2;
			}
			x = this.font_.x + (float)num3 * (this.font_.width + this.fontGap_.x);
			y = this.font_.y - (float)num4 * (this.font_.height + this.fontGap_.y);
		}
		else if (num >= 10)
		{
			int num5 = num - 10;
			int num6;
			int num7;
			if (charIdx < 0)
			{
				num6 = (charIdx + 1) / num5 - 1;
				num7 = num5 - 1 + (charIdx + 1) % num5;
			}
			else
			{
				num7 = charIdx % num5;
				num6 = charIdx / num5;
			}
			x = this.font_.x + (float)num7 * (this.font_.width + this.fontGap_.x);
			y = this.font_.y - (float)num6 * (this.font_.height + this.fontGap_.y);
		}
		else
		{
			x = this.font_.x + (((num & 1) == 0) ? 0f : ((float)charIdx * (this.font_.width + this.fontGap_.x)));
			y = this.font_.y - (((num & 2) == 0) ? 0f : ((float)charIdx * (this.font_.height + this.fontGap_.y)));
		}
		if (this.useRevision_ && ScreenController.Instance.IsDisplayingLandscapeRight())
		{
			x -= this.revision_.x;
			y -= this.revision_.y;
		}
	}

	public bool GetUV(int charIdx, ref Vector2[] uvs, int idx)
	{
		Vector2 zero = Vector2.zero;
		this.GetLeftBottom(charIdx, out zero.x, out zero.y);
		uvs[idx] = new Vector2(zero.x, zero.y + this.font_.height);
		uvs[idx + 1] = new Vector2(zero.x + this.font_.width, zero.y + this.font_.height);
		uvs[idx + 2] = new Vector2(zero.x, zero.y);
		uvs[idx + 3] = new Vector2(zero.x + this.font_.width, zero.y);
		return true;
	}

	public static string ToStringVector2(Vector2 t)
	{
		return string.Concat(new string[]
		{
			"[",
			t.x.ToString(),
			" , ",
			t.y.ToString(),
			"]"
		});
	}

	public static Vector3 DEFAULT_GAP = Vector3.forward;

	public static Vector3 X2_GAP = new Vector3(2f, 0f, 1f);

	public static Vector3 Y2_GAP = new Vector3(0f, 2f, 2f);

	private Rect font_ = RectHelper.zero;

	private Vector3 fontGap_ = GUIFontCalculator.DEFAULT_GAP;

	private Vector3 revision_ = Vector3.zero;

	private bool useRevision_;
}
