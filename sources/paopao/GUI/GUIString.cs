using System;
using UnityEngine;

public class GUIString : GUIInterface
{
	public GUIString(Vector2 pos, int layer, int maxSize, GUIString.Alignment alignment, int fontType, FiaTexture tex)
	{
		this.calc_ = GUIFontManager.Instance.GetFont(fontType, tex);
		if (Env.IsIPad2)
		{
			this.calc_.UseRevision = false;
		}
		else
		{
			this.calc_.UseRevision = true;
		}
		this.panels_ = new GUIPanelEx[maxSize];
		this.pos_ = pos;
		this.alignment_ = alignment;
		this.str_ = string.Empty;
		for (int i = 0; i < maxSize; i++)
		{
			this.panels_[i] = new GUIPanelEx(this.calc_, layer);
		}
	}

	public int SubMeshIndex
	{
		get
		{
			return this.subMeshIndex_;
		}
		set
		{
			this.subMeshIndex_ = value;
			this.ResetSubMeshIndex();
		}
	}

	private void ResetSubMeshIndex()
	{
		foreach (GUIPanelEx guipanelEx in this.panels_)
		{
			guipanelEx.SubMeshIndex = this.subMeshIndex_;
		}
	}

	public Rect Rect
	{
		get
		{
			if (this.str_ == null || this.str_.Length < 1)
			{
				return new Rect(this.pos_.x, this.pos_.y, 0f, 0f);
			}
			float xMin = this.panels_[0].Rect.xMin;
			float yMin = this.panels_[0].Rect.yMin;
			float xMax = this.panels_[this.str_.Length - 1].Rect.xMax;
			float yMax = this.panels_[this.str_.Length - 1].Rect.yMax;
			return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
		}
	}

	public void SetString(string str)
	{
		if (this.str_ == str)
		{
			return;
		}
		int length = str.Length;
		float num = ((this.alignment_ != GUIString.Alignment.LEFT) ? ((this.alignment_ != GUIString.Alignment.RIGHT) ? (this.pos_.x - this.calc_.GetWidth(str) / 2f) : (this.pos_.x - this.calc_.GetWidth(str))) : this.pos_.x);
		float y = this.pos_.y;
		float num2 = this.pos_.y + this.calc_.GetHeight();
		for (int i = 0; i < this.panels_.Length; i++)
		{
			if (i < length)
			{
				this.panels_[i].Visible = true;
				int num3 = this.calc_.GetIndex(str[i]);
				if (num3 < 0)
				{
					num3 = this.calc_.GetLastFontIndex();
				}
				this.panels_[i].UV = num3;
				float num4 = num + this.calc_.GetWidth(num3);
				this.panels_[i].SetRectByWindowSpace(num, y, num4, num2);
				num = num4;
			}
			else
			{
				this.panels_[i].Visible = false;
			}
		}
		this.str_ = str;
		this.ResetSubMeshIndex();
	}

	public void RegistPanelManager(GUIPanelManager manager)
	{
		for (int i = 0; i < this.panels_.Length; i++)
		{
			manager.RegistPanel(this.panels_[i]);
		}
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		for (int i = 0; i < this.panels_.Length; i++)
		{
			this.panels_[i].MoveRectByWindowPos(x, y);
		}
		this.pos_.x = this.pos_.x + x;
		this.pos_.y = this.pos_.y + y;
	}

	public void SetRectByWindowSpace(float x, float y)
	{
		this.MoveRectByWindowPos(x - this.pos_.x, y - this.pos_.y);
	}

	public Vector2 GetLeftTopByWindowPos()
	{
		return this.pos_;
	}

	public FontCalculatorInterface FontCalculator
	{
		get
		{
			return this.calc_;
		}
	}

	public void SetColor(Color clr)
	{
		foreach (GUIPanelEx guipanelEx in this.panels_)
		{
			guipanelEx.VerticeColor = clr;
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < this.panels_.Length; i++)
		{
			text = text + this.panels_[i].Dirty.ToString() + " ";
		}
		return text;
	}

	protected int subMeshIndex_;

	private GUIPanelEx[] panels_;

	private string str_ = string.Empty;

	private GUIFontCalculatorEx calc_;

	private Vector2 pos_;

	private GUIString.Alignment alignment_;

	public enum Alignment
	{
		LEFT,
		RIGHT,
		CENTER
	}

	public enum FontType
	{
		PT_34,
		PT_30,
		PT_28,
		PT_26,
		PT_22,
		PT_16,
		PT_40
	}
}
