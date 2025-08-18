using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIUVScrollImage : GUIInterface
{
	public GUIUVScrollImage(int type, float[] f, FiaTexture tex, int layer, GUIScrollBar scrollBar)
	{
		this.Initialize(type, f, tex, layer, scrollBar);
	}

	public bool Visible
	{
		get
		{
			return this.visible_;
		}
		set
		{
			this.image_.Visible = value;
			this.scrollbar_.Visible = value;
			this.visible_ = value;
		}
	}

	protected void Initialize(int type, float[] f, FiaTexture tex, int layer, GUIScrollBar scrollBar)
	{
		if (f.Length % 4 != 0 && f.Length < 8)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogError("Invalid f length : " + f.Length.ToString());
			}
			return;
		}
		this.view_ = new float[4];
		Array.Copy(f, 0, this.view_, 0, 4);
		this.uvRect_ = new float[f.Length - 4];
		Array.Copy(f, 4, this.uvRect_, 0, f.Length - 4);
		int num = this.uvRect_.Length / 4;
		this.calc_ = new GUIFontCalculator[num];
		this.maxScroll_ = new float[num];
		float num2 = this.view_[3] - this.view_[1];
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = new Vector2(this.uvRect_[i * 4 + 2] - this.uvRect_[i * 4], this.uvRect_[i * 4 + 3] - this.uvRect_[i * 4 + 1]);
			if (vector.y > num2)
			{
				this.maxScroll_[i] = vector.y - num2;
				vector.y = num2;
			}
			else
			{
				this.maxScroll_[i] = 0f;
			}
			Vector2 vector2 = new Vector2(this.uvRect_[i * 4], tex.OrgRect.height - this.uvRect_[i * 4 + 1]);
			this.calc_[i] = new GUIFontCalculator(tex.UV, tex.OrgRect, vector, vector2, new Vector3(0f, -vector.y + 1f, 2f));
		}
		this.image_ = new GUIPanelEx(this.calc_[0], layer);
		if (scrollBar != null)
		{
			if (!scrollBar.IsInitialized())
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("scrollBar is not initialized");
				}
				return;
			}
			this.scrollbar_ = scrollBar;
		}
		this.SetUV(0);
	}

	public bool SetUV(int idx)
	{
		if (idx < 0 || idx >= this.calc_.Length)
		{
			return false;
		}
		if (this.image_ == null)
		{
			return false;
		}
		if (idx == this.uvIdx_)
		{
			return false;
		}
		this.uvIdx_ = idx;
		this.image_.FontCalculator = this.calc_[this.uvIdx_];
		this.image_.SetUV(0);
		this.image_.Dirty = 5;
		float[] array = new float[4];
		Array.Copy(this.view_, array, 4);
		float num = this.uvRect_[4 * this.uvIdx_ + 3] - this.uvRect_[4 * this.uvIdx_ + 1];
		float num2 = array[3] - array[1];
		if (num2 >= num)
		{
			array[3] = array[1] + num;
		}
		this.image_.SetRectByWindowSpace(array[0], array[1], array[2], array[3]);
		if (this.scrollbar_ != null)
		{
			this.scrollbar_.info_.listRegion_ = new Rect(0f, 0f, 0f, this.maxScroll_[this.uvIdx_]);
			this.scrollbar_.Recalculate();
		}
		return true;
	}

	public int SubMeshIndex
	{
		get
		{
			return this.image_.SubMeshIndex;
		}
		set
		{
			this.image_.SubMeshIndex = value;
		}
	}

	public void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistPanel(this.image_);
		foreach (GUIPanelEx guipanelEx in this.panels_)
		{
			manager.RegistPanel(guipanelEx);
		}
	}

	public void MoveRectByWindowPos(float x, float y)
	{
		this.image_.MoveRectByWindowPos(x, y);
		foreach (GUIPanelEx guipanelEx in this.panels_)
		{
			guipanelEx.MoveRectByWindowPos(x, y);
		}
	}

	public void RegistGUIPanel(GUIPanelEx panel)
	{
		this.panels_.Add(panel);
	}

	public void SetRectByWindowSpace(float x, float y)
	{
		Vector2 leftTopByWindowPos = this.GetLeftTopByWindowPos();
		this.MoveRectByWindowPos(x - leftTopByWindowPos.x, y - leftTopByWindowPos.y);
	}

	public Vector2 GetLeftTopByWindowPos()
	{
		return this.image_.GetLeftTopByWindowPos();
	}

	public bool SetScroll(int idx)
	{
		int num = Mathf.Clamp(idx, 0, (int)this.maxScroll_[this.uvIdx_]);
		return this.image_.SetUV(num);
	}

	public bool AddScroll(int value)
	{
		int num = Mathf.Clamp(this.image_.UV + value, 0, (int)this.maxScroll_[this.uvIdx_]);
		return this.image_.SetUV(num);
	}

	public bool IsScrollable()
	{
		return this.maxScroll_[this.uvIdx_] > 0f;
	}

	public byte InputAuthority
	{
		get
		{
			return this.inputAuthority_;
		}
		set
		{
			this.inputAuthority_ = value;
		}
	}

	public void Update()
	{
		if (this.image_ == null)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		int num = 1;
		if (StageController.IsInputAuthorized(this.inputAuthority_) && this.IsScrollable())
		{
			if (this.buttonUpTime_ > 0f)
			{
				Vector3 vector2 = Vector3.zero;
				if (this.scrollVelocity_ != Vector3.zero)
				{
					this.slideTimeAccum_ += Time.deltaTime;
					float num2 = this.slideTimeAccum_ / this.inertiaDuration_;
					vector2 = Vector3.Lerp(this.scrollVelocity_, Vector3.zero, num2);
					vector = vector2 * Time.deltaTime;
					if (MathHelper.IsBetweenII(vector[num], -1f, 1f))
					{
						this.scrollVelocity_ = Vector3.zero;
					}
				}
			}
			Vector3 mousePosition = Input.mousePosition;
			if (Input.GetMouseButton(0) && this.image_.Contains(mousePosition))
			{
				if (this.buttonUpTime_ > 0f)
				{
					GUIUVScrollImage.MOUSE_PRESS_COUNT++;
					if (vector != Vector3.zero)
					{
						this.slideMousePressId_ = GUIUVScrollImage.MOUSE_PRESS_COUNT;
					}
					this.lastPos_ = mousePosition;
					this.scrollVelocity_ = Vector3.zero;
					vector = Vector3.zero;
					this.buttonUpTime_ = 0f;
				}
				else
				{
					vector = mousePosition - this.lastPos_;
					this.scrollVelocity_ = vector / Time.deltaTime * 0.25f;
					this.lastPos_ = mousePosition;
					if (vector != Vector3.zero)
					{
						this.slideMousePressId_ = GUIUVScrollImage.MOUSE_PRESS_COUNT;
					}
				}
			}
			else if (this.buttonUpTime_ == 0f)
			{
				this.buttonUpTime_ = Time.time;
				this.slideTimeAccum_ = 0f;
			}
			this.AddScroll((int)vector[num]);
			if (this.scrollbar_ != null)
			{
				if ((this.buttonUpTime_ > 0f && (int)vector[num] == 0) || this.slideMousePressId_ != GUIUVScrollImage.MOUSE_PRESS_COUNT)
				{
					if (this.scrollbar_.gameObject.active)
					{
						this.scrollbar_.gameObject.SetActiveRecursively(false);
					}
				}
				else if (!this.scrollbar_.gameObject.active)
				{
					this.scrollbar_.gameObject.SetActiveRecursively(true);
				}
				this.scrollbar_.SetScrollPosition(0f, this.maxScroll_[this.uvIdx_], (float)this.image_.UV, 1f);
			}
		}
		else if (this.scrollbar_ != null && this.scrollbar_.gameObject.active)
		{
			this.scrollbar_.gameObject.SetActiveRecursively(false);
		}
	}

	protected const float VELOCITY_SCALE = 0.25f;

	private GUIPanelEx image_;

	private float[] view_;

	private float[] uvRect_;

	private GUIFontCalculator[] calc_;

	private float[] maxScroll_;

	private int uvIdx_ = -1;

	private GUIScrollBar scrollbar_;

	private bool visible_ = true;

	private List<GUIPanelEx> panels_ = new List<GUIPanelEx>();

	protected float buttonUpTime_;

	protected Vector3 lastPos_ = Vector3.zero;

	protected Vector3 scrollVelocity_ = Vector3.zero;

	public static int MOUSE_PRESS_COUNT;

	protected int slideMousePressId_ = -1;

	protected float slideTimeAccum_;

	protected float inertiaDuration_ = 1f;

	protected byte inputAuthority_ = 1;
}
