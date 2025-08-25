using System;
using UnityEngine;

public class GUIPanel
{
	public GUIPanel(float left, float top, float right, float bottom, int layer)
	{
		this.left_ = left;
		this.top_ = top;
		this.right_ = right;
		this.bottom_ = bottom;
		this.layer_ = layer;
	}

	public GUIPanel(int layer)
	{
		this.left_ = 0f;
		this.top_ = 0f;
		this.right_ = 0f;
		this.bottom_ = 0f;
		this.layer_ = layer;
	}

	public Rect Rect
	{
		get
		{
			return new Rect(this.left_, this.top_, this.right_ - this.left_, this.top_ - this.bottom_);
		}
	}

	public static GUIPanel CreateByWindowSpace(float left, float top, float right, float bottom, int layer)
	{
		return new GUIPanel(left, (float)Screen.height - top, right, (float)Screen.height - bottom, layer);
	}

	public void SetRect(float left, float top, float right, float bottom)
	{
		this.left_ = left;
		this.top_ = top;
		this.right_ = right;
		this.bottom_ = bottom;
	}

	public void SetXYByWindowSpace(float x, float y)
	{
		this.right_ = x + this.right_ - this.left_;
		this.bottom_ = (float)Screen.height - y + this.top_ - this.bottom_;
		this.left_ = x;
		this.top_ = (float)Screen.height - y;
	}

	public Rect GetRectWS()
	{
		return new Rect(this.left_, (float)Screen.height - this.top_, this.right_ - this.left_, this.top_ - this.bottom_);
	}

	public float GetWidth()
	{
		return this.right_ - this.left_;
	}

	public float GetHeight()
	{
		return this.top_ - this.bottom_;
	}

	public void SetRectByWindowSpace(float left, float top, float right, float bottom)
	{
		this.left_ = left;
		this.top_ = (float)Screen.height - top;
		this.right_ = right;
		this.bottom_ = (float)Screen.height - bottom;
	}

	public void MoveRect(float x, float y)
	{
		this.left_ += x;
		this.right_ += x;
		this.top_ += y;
		this.bottom_ += y;
	}

	public void SetRectByWindowSpace(float[] f)
	{
		this.SetRectByWindowSpace(f[0], f[1], f[2], f[3]);
	}

	public void GetVertices(out Vector3[] v)
	{
		v = new Vector3[4];
		float num = 0.1f * (float)this.layer_;
		v[0] = new Vector3(this.left_, this.top_, num);
		v[1] = new Vector3(this.right_, this.top_, num);
		v[2] = new Vector3(this.left_, this.bottom_, num);
		v[3] = new Vector3(this.right_, this.bottom_, num);
		if (!ScreenController.Instance.NeedToBeForced() || ScreenController.Instance.IsDisplayingLandscapeRight())
		{
		}
	}

	public void GetVertices(ref Vector3[] v, int idx)
	{
		float num = 0.1f * (float)this.layer_;
		v[idx] = new Vector3(this.left_, this.top_, num);
		v[idx + 1] = new Vector3(this.right_, this.top_, num);
		v[idx + 2] = new Vector3(this.left_, this.bottom_, num);
		v[idx + 3] = new Vector3(this.right_, this.bottom_, num);
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			this.left_.ToString(),
			" ",
			this.top_.ToString(),
			" ",
			this.right_.ToString(),
			" ",
			this.bottom_.ToString(),
			" ",
			this.layer_.ToString()
		});
	}

	public bool Contains(Vector3 pos)
	{
		return MathHelper.IsBetweenII(pos.x, this.left_, this.right_) && MathHelper.IsBetweenII(pos.y, this.bottom_, this.top_);
	}

	private const float Z_OFFSET = 0.1f;

	public float left_;

	public float top_;

	public float right_;

	public float bottom_;

	public int layer_ = 1;
}
