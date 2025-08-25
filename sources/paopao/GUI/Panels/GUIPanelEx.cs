using System;
using UnityEngine;

public class GUIPanelEx : GUIInterface, MouseNotifier
{
	public GUIPanelEx()
	{
	}

	public GUIPanelEx(FontCalculatorInterface fontInterface, int layer)
	{
		this.panel_ = new GUIPanel(layer);
		this.calc_ = fontInterface;
	}

	public GUIPanelEx(ArrayEx<float> f, Texture tex, int layer)
	{
		this.Initialize(f, new Rect(0f, 0f, 1f, 1f), new Rect(0f, 0f, (float)tex.width, (float)tex.height), layer, GUIFontCalculator.DEFAULT_GAP);
	}

	public GUIPanelEx(ArrayEx<float> f, Rect uvRect, Rect orgRect, int layer)
	{
		this.Initialize(f, uvRect, orgRect, layer, GUIFontCalculator.DEFAULT_GAP);
	}

	public Rect Rect
	{
		get
		{
			return this.panel_.Rect;
		}
	}

	public Rect TouchRegion
	{
		get
		{
			return this.touchRegion_;
		}
		set
		{
			this.touchRegion_ = value;
			this.useTouchRegion = true;
		}
	}

	public void SetTouchRegionByWindowRect(Rect rect_)
	{
		rect_.y = (float)Screen.height - rect_.y - rect_.height;
		this.TouchRegion = rect_;
	}

	public void SetTouchRegionByWindowPos(float _left, float _top, float _right, float _bottom)
	{
		Rect rect = new Rect(_left, _top, _right - _left, _bottom - _top);
		rect.y = (float)Screen.height - rect.y - rect.height;
		this.TouchRegion = rect;
	}

	public FontCalculatorInterface FontCalculator
	{
		get
		{
			return this.calc_;
		}
		set
		{
			this.calc_ = value;
		}
	}

	public GUIPanel Panel
	{
		get
		{
			return this.panel_;
		}
	}

	public int Layer
	{
		get
		{
			return this.panel_.layer_;
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
		}
	}

	public int SubMeshCounter
	{
		get
		{
			return this.subMeshCounter_;
		}
		set
		{
			this.subMeshCounter_ = value;
		}
	}

	public bool Visible
	{
		get
		{
			return this.visible_;
		}
		set
		{
			this.visible_ = value;
			this.dirtyFlag_ |= 1;
		}
	}

	public float Alpha
	{
		get
		{
			return this.color_.a;
		}
		set
		{
			this.color_.a = value;
			this.dirtyFlag_ |= 2;
		}
	}

	public Color VerticeColor
	{
		get
		{
			return this.color_;
		}
		set
		{
			if (this.color_ != value)
			{
				this.color_ = value;
				this.dirtyFlag_ |= 2;
			}
		}
	}

	public int Dirty
	{
		get
		{
			return this.dirtyFlag_;
		}
		set
		{
			this.dirtyFlag_ = value;
		}
	}

	public int UV
	{
		get
		{
			return this.uv_;
		}
		set
		{
			this.uv_ = value;
			this.dirtyFlag_ |= 4;
		}
	}

	public bool SetUV(int idx)
	{
		if (this.uv_ == idx)
		{
			return false;
		}
		this.uv_ = idx;
		this.dirtyFlag_ |= 4;
		return true;
	}

	public void SetUV(Rect uv)
	{
		this.calc_.SetUV(uv);
		this.dirtyFlag_ |= 4;
	}

	public virtual void Initialize(ArrayEx<float> f, Rect uvRect, Rect orgRect, int layer, Vector3 fontGap)
	{
		if (f.Length == 8)
		{
			this.panel_ = new GUIPanel(f[0], f[1], f[2], f[3], layer);
			this.calc_ = new GUIFontCalculator(uvRect, orgRect, new Vector2(f[6] - f[4], f[5] - f[7]), new Vector2(f[4], f[5]), fontGap);
		}
		else if (f.Length == 6)
		{
			Vector2 vector = new Vector2(f[4] - f[2], f[3] - f[5]);
			this.panel_ = new GUIPanel(f[0], f[1], f[0] + vector.x, f[1] - vector.y, layer);
			this.calc_ = new GUIFontCalculator(uvRect, orgRect, vector, new Vector2(f[2], f[3]), fontGap);
		}
	}

	public void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistPanel(this);
	}

	public virtual void GetVerticesUVs(ref Vector3[] vertices, ref Vector2[] uvs, int idx)
	{
		this.panel_.GetVertices(ref vertices, idx);
		this.calc_.GetUV(this.uv_, ref uvs, idx);
	}

	public void SetRectByWindowSpace(float left, float top, float right, float bottom)
	{
		this.panel_.SetRectByWindowSpace(left, top, right, bottom);
		this.dirtyFlag_ |= 8;
	}

	public void SetRectByWindowSpace(float left, float top)
	{
		float width = this.panel_.GetWidth();
		float height = this.panel_.GetHeight();
		this.panel_.SetRectByWindowSpace(left, top, left + width, top + height);
		this.dirtyFlag_ |= 8;
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		this.panel_.MoveRect(x, -y);
		if (this.useTouchRegion)
		{
			this.touchRegion_.xMin = this.touchRegion_.xMin + x;
			this.touchRegion_.xMax = this.touchRegion_.xMax + x;
			this.touchRegion_.yMin = this.touchRegion_.yMin - y;
			this.touchRegion_.yMax = this.touchRegion_.yMax - y;
		}
		this.dirtyFlag_ |= 8;
	}

	public Vector2 GetLeftTopByWindowPos()
	{
		return new Vector2(this.panel_.left_, (float)Screen.height - this.panel_.top_);
	}

	public bool Contains(Vector3 pos)
	{
		return (!this.useTouchRegion) ? this.panel_.Contains(pos) : this.touchRegion_.Contains(pos);
	}

	public int GetPriority()
	{
		return -this.panel_.layer_;
	}

	public bool IsEnabled()
	{
		return this.Visible;
	}

	public override string ToString()
	{
		string text = string.Empty;
		text = text + "panel : " + this.panel_.ToString() + "\n";
		text = text + "calc : " + this.calc_.ToString() + "\n";
		text = text + "visible : " + this.visible_.ToString() + "\n";
		text = text + "calc : " + this.color_.ToString() + "\n";
		text = text + "dirty : " + this.dirtyFlag_.ToString() + "\n";
		return text + "uv : " + this.uv_.ToString() + "\n";
	}

	public void SetExpansionTouchRegion(float width, float height)
	{
		float num = (this.panel_.left_ + this.panel_.right_ - width) * 0.5f;
		float num2 = (this.panel_.top_ + this.panel_.bottom_ - height) * 0.5f;
		this.touchRegion_ = new Rect(num, num2, width, height);
		this.useTouchRegion = true;
	}

	protected GUIPanel panel_;

	protected FontCalculatorInterface calc_;

	protected bool visible_ = true;

	protected Color color_ = Color.white;

	protected int dirtyFlag_;

	protected int uv_;

	protected bool useTouchRegion;

	protected Rect touchRegion_;

	protected int subMeshIndex_;

	protected int subMeshCounter_;

	public enum DirtyFlag
	{
		VISIBLE = 1,
		COLOR,
		UVS = 4,
		POSITION = 8
	}
}
