using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIImage : FiaGUILayer
{
	public GUIPanelEx Image
	{
		get
		{
			return this.image_;
		}
	}

	public Rect Rect
	{
		get
		{
			return new Rect(this.pos_.x, this.pos_.y, this.width_, this.height_);
		}
		set
		{
			this.pos_.x = value.xMin;
			this.pos_.y = value.yMin;
			this.width_ = value.width;
			this.height_ = value.height;
		}
	}

	public Vector2 Position
	{
		get
		{
			return this.pos_;
		}
		set
		{
			this.pos_ = value;
			if (this.image_ != null)
			{
				this.image_.SetRectByWindowSpace(this.pos_.x, this.pos_.y);
			}
		}
	}

	public Rect Texture
	{
		get
		{
			return this.tex_;
		}
		set
		{
			this.tex_ = value;
			if (this.width_ == 0f && this.height_ == 0f)
			{
				this.width_ = this.tex_.width;
				this.height_ = this.tex_.height;
			}
			if (this.image_ != null)
			{
				this.image_.SetUV(this.tex_);
			}
		}
	}

	public int Layer
	{
		get
		{
			return this.layer_;
		}
		set
		{
			this.layer_ = value;
			if (this.image_ != null)
			{
				this.image_.Panel.layer_ = this.layer_;
			}
		}
	}

	public Vector3 FontGap
	{
		get
		{
			return this.gap_;
		}
		set
		{
			this.gap_ = value;
			Rect rect = new Rect(0f, 0f, 1f, 1f);
			Rect rect2 = new Rect(0f, 0f, this.tex_.width, this.tex_.height);
			if (this.image_ != null)
			{
				this.image_.FontCalculator = new GUIFontCalculator(rect, rect2, new Vector2(this.tex_.width, this.tex_.height), new Vector2(this.tex_.xMin, this.tex_.yMin), this.gap_);
			}
		}
	}

	public override void DoInit()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		if (this.width_ == 0f && this.height_ == 0f)
		{
			this.width_ = this.tex_.width;
			this.height_ = this.tex_.height;
		}
		this.image_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[]
		{
			this.pos_.x,
			this.pos_.y,
			this.pos_.x + this.width_,
			this.pos_.y + this.height_,
			this.tex_.xMin,
			this.tex_.yMin,
			this.tex_.xMax,
			this.tex_.yMax
		}, fiaTexture, this.layer_, this.gap_);
		this.panelManager_.RegistGUIInterface(this.image_);
	}

	public GUIPanelEx image_;

	public Vector2 pos_ = new Vector2(0f, 0f);

	public float width_;

	public float height_;

	public Rect tex_ = new Rect(0f, 0f, 0f, 0f);

	public int layer_;

	public Vector3 gap_ = new Vector3(0f, 0f, 1f);
}
