using System;
using UnityEngine;

public class GUIListCtrl : FiaGUILayer
{
	protected virtual void Awake()
	{
	}

	protected virtual int GetCompareIndex()
	{
		return 1;
	}

	protected virtual Rect GetAvailableRegion()
	{
		return RectHelper.zero;
	}

	protected virtual void InitializeListctrl()
	{
	}

	protected virtual ScrollBarInfo GetScrollbarInfo()
	{
		return null;
	}

	protected virtual void GetMinMaxRange(out float minRange, out float maxRange)
	{
		minRange = 0f;
		maxRange = 0f;
	}

	public int SelectedId
	{
		get
		{
			return this.selectedId_;
		}
		set
		{
			this.selectedId_ = value;
		}
	}

	public override void DoInit()
	{
		int compareIndex = this.GetCompareIndex();
		Vector3 vector = this.cam_.ScreenToWorldPoint(new Vector3(0f, 0f, this.cam_.nearClipPlane));
		Vector3 vector2 = this.cam_.ScreenToWorldPoint((compareIndex != 1) ? new Vector3(1f, 0f, this.cam_.nearClipPlane) : new Vector3(0f, 1f, this.cam_.nearClipPlane));
		this.moveFactor_ = vector2 - vector;
		this.standardPos_ = this.cam_.ScreenToWorldPoint(new Vector3(0f, 0f, this.cam_.nearClipPlane));
		this.InitializeListctrl();
		this.CalculateMinMaxPos();
		ScrollBarInfo scrollbarInfo = this.GetScrollbarInfo();
		if (this.scrollbarPrefab_ != null && scrollbarInfo != null)
		{
			this.scrollbar_ = (GUIScrollBar)global::UnityEngine.Object.Instantiate(this.scrollbarPrefab_);
			this.scrollbar_.transform.parent = base.transform.parent;
			this.scrollbar_.info_ = scrollbarInfo;
			this.scrollbar_.Initialize();
			if (!this.isAvailableScroll_)
			{
				this.scrollbar_.gameObject.SetActiveRecursively(false);
			}
		}
		this.availableRegion_ = this.GetAvailableRegion();
		this.mouseManager_.AvailableTouchRegion = this.availableRegion_;
	}

	protected void Recalculate()
	{
		this.CalculateMinMaxPos();
		if (this.scrollbar_ != null)
		{
			this.scrollbar_.gameObject.SetActiveRecursively(false);
			if (this.isAvailableScroll_)
			{
				this.scrollbar_.info_ = this.GetScrollbarInfo();
				this.scrollbar_.Recalculate();
			}
		}
	}

	private void CalculateMinMaxPos()
	{
		int compareIndex = this.GetCompareIndex();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		Vector3 vector = ((compareIndex != 1) ? new Vector3(num2, 0f, this.cam_.nearClipPlane) : new Vector3(0f, num2, this.cam_.nearClipPlane));
		Vector3 vector2 = ((compareIndex != 1) ? new Vector3(num, 0f, this.cam_.nearClipPlane) : new Vector3(0f, num, this.cam_.nearClipPlane));
		this.minPos_ = this.cam_.ScreenToWorldPoint(vector2) - this.standardPos_;
		this.maxPos_ = this.cam_.ScreenToWorldPoint(vector) - this.standardPos_;
		this.isAvailableScroll_ = this.minPos_[compareIndex] < this.maxPos_[compareIndex];
	}

	private int IsOutOfRange(Vector3 v)
	{
		int compareIndex = this.GetCompareIndex();
		if (v[compareIndex] < this.minPos_[compareIndex])
		{
			return -1;
		}
		if (v[compareIndex] > this.maxPos_[compareIndex])
		{
			return 1;
		}
		return 0;
	}

	private int IsOutOfRange()
	{
		int compareIndex = this.GetCompareIndex();
		if (base.transform.localPosition[compareIndex] < this.minPos_[compareIndex])
		{
			return -1;
		}
		if (base.transform.localPosition[compareIndex] > this.maxPos_[compareIndex])
		{
			return 1;
		}
		return 0;
	}

	private int IsOutOfRangeIncludingDir(Vector3 delta)
	{
		int compareIndex = this.GetCompareIndex();
		float num = base.transform.localPosition[compareIndex];
		if (num < this.minPos_[compareIndex] && delta[compareIndex] < 0f)
		{
			return -1;
		}
		if (num > this.maxPos_[compareIndex] && delta[compareIndex] > 0f)
		{
			return 1;
		}
		return 0;
	}

	private void ResetLocalPosition()
	{
		int compareIndex = this.GetCompareIndex();
		Vector3 localPosition = base.transform.localPosition;
		localPosition[compareIndex] = Mathf.Clamp(localPosition[compareIndex], this.minPos_[compareIndex], this.maxPos_[compareIndex]);
		base.transform.localPosition = localPosition;
	}

	protected void ResetLocalPosition(float value)
	{
		if (this.isAvailableScroll_)
		{
			int compareIndex = this.GetCompareIndex();
			Vector3 vector = Vector3.zero;
			vector[compareIndex] = value;
			vector.z = this.cam_.nearClipPlane;
			vector = this.cam_.ScreenToWorldPoint(vector) - this.standardPos_;
			Vector3 localPosition = base.transform.localPosition;
			localPosition[compareIndex] = Mathf.Clamp(vector[compareIndex], this.minPos_[compareIndex], this.maxPos_[compareIndex]);
			base.transform.localPosition = localPosition;
		}
		else
		{
			base.transform.localPosition = Vector3.zero;
		}
	}

	protected override void BeforePanelUpdate()
	{
		Vector3 localPosition = base.transform.localPosition;
		Vector3 vector = Vector3.zero;
		int compareIndex = this.GetCompareIndex();
		if (this.isAvailableScroll_ && StageController.IsInputAuthorized(this.inputAuthority_))
		{
			if (this.buttonUpTime_ > 0f)
			{
				Vector3 vector2 = Vector3.zero;
				if (this.scrollVelocity_ != Vector3.zero)
				{
					this.slideTimeAccum_ += Time.deltaTime * ((this.IsOutOfRange() == 0) ? 1f : 5f);
					float num = this.slideTimeAccum_ / this.inertiaDuration_;
					vector2 = Vector3.Lerp(this.scrollVelocity_, Vector3.zero, num);
					vector = vector2 * Time.deltaTime;
					if (MathHelper.IsBetweenII(vector[compareIndex], -this.moveFactor_[compareIndex], this.moveFactor_[compareIndex]))
					{
						this.scrollVelocity_ = Vector3.zero;
					}
				}
				if (vector == Vector3.zero)
				{
					int num2 = this.IsOutOfRange(localPosition);
					if (num2 != 0)
					{
						Vector3 vector3 = -localPosition + ((num2 != -1) ? this.maxPos_ : this.minPos_);
						if (MathHelper.IsBetweenII(vector3[compareIndex], -this.moveFactor_[compareIndex] * 0.5f, this.moveFactor_[compareIndex] * 0.5f))
						{
							vector = vector3;
						}
						else
						{
							vector = vector3 * 0.2f;
						}
					}
				}
			}
			Vector3 mousePosition = Input.mousePosition;
			if (Input.GetMouseButton(0) && this.availableRegion_.Contains(mousePosition))
			{
				if (this.buttonUpTime_ > 0f)
				{
					GUIListCtrl.MOUSE_PRESS_COUNT++;
					if (this.IsOutOfRange() != 0 || vector != Vector3.zero)
					{
						this.slideMousePressId_ = GUIListCtrl.MOUSE_PRESS_COUNT;
					}
					this.lastPos_ = mousePosition;
					this.scrollVelocity_ = Vector3.zero;
					vector = Vector3.zero;
					this.buttonUpTime_ = 0f;
					this.ResetLocalPosition();
				}
				else
				{
					vector = (mousePosition - this.lastPos_)[compareIndex] * this.moveFactor_;
					this.scrollVelocity_ = vector / Time.deltaTime * 0.25f;
					if (this.IsOutOfRangeIncludingDir(vector) != 0)
					{
						vector *= 0.5f;
						this.scrollVelocity_ = Vector3.zero;
					}
					this.lastPos_ = mousePosition;
					if (vector != Vector3.zero)
					{
						this.slideMousePressId_ = GUIListCtrl.MOUSE_PRESS_COUNT;
					}
				}
			}
			else if (this.buttonUpTime_ == 0f)
			{
				this.buttonUpTime_ = Time.time;
				this.slideTimeAccum_ = 0f;
			}
			base.transform.localPosition += vector;
			if (this.scrollbar_ != null)
			{
				if ((this.buttonUpTime_ > 0f && localPosition == base.transform.localPosition) || this.slideMousePressId_ != GUIListCtrl.MOUSE_PRESS_COUNT)
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
			}
		}
		else if (this.scrollbar_.gameObject.active)
		{
			this.scrollbar_.gameObject.SetActiveRecursively(false);
		}
		if (StageController.IsInputAuthorized(this.inputAuthority_) && (!this.isAvailableScroll_ || (localPosition == base.transform.localPosition && this.slideMousePressId_ != GUIListCtrl.MOUSE_PRESS_COUNT)))
		{
			Vector3 zero = Vector3.zero;
			zero[compareIndex] = -localPosition[compareIndex] / this.moveFactor_[compareIndex];
			this.mouseManager_.Offset = zero;
			this.mouseManager_.Update();
		}
		else
		{
			if (this.scrollbar_ != null && this.isAvailableScroll_)
			{
				this.scrollbar_.SetScrollPosition(this.minPos_[compareIndex], this.maxPos_[compareIndex], base.transform.localPosition[compareIndex], this.moveFactor_[compareIndex]);
			}
			this.mouseManager_.InitSelectionData();
		}
	}

	private void OnEnable()
	{
		if (this.scrollbar_ != null && this.isAvailableScroll_)
		{
			this.scrollbar_.gameObject.SetActiveRecursively(true);
		}
	}

	private void OnDisable()
	{
		if (this.scrollbar_ != null && this.isAvailableScroll_)
		{
			this.scrollbar_.gameObject.SetActiveRecursively(false);
		}
	}

	protected const float VELOCITY_SCALE = 0.25f;

	protected const float OUT_OF_RANGE_TIME_FACTOR = 5f;

	protected MouseManager mouseManager_ = new MouseManager(true);

	public GUIScrollBar scrollbarPrefab_;

	protected GUIScrollBar scrollbar_;

	protected Vector3 moveFactor_ = Vector3.zero;

	protected Vector3 minPos_;

	protected Vector3 maxPos_;

	protected Vector3 lastPos_ = Vector3.zero;

	protected Vector3 scrollVelocity_ = Vector3.zero;

	protected float buttonUpTime_;

	protected float inertiaDuration_ = 1f;

	protected float slideTimeAccum_;

	protected Rect availableRegion_;

	public static int MOUSE_PRESS_COUNT;

	protected int slideMousePressId_ = -1;

	protected int selectedId_ = -1;

	private Vector3 standardPos_ = Vector3.zero;

	private bool isAvailableScroll_ = true;

	protected byte inputAuthority_ = 1;
}
