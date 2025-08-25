using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager
{
	public MouseManager()
	{
		this.isReleaseIfMoved_ = false;
	}

	public MouseManager(bool isReleaseIfMoved)
	{
		this.isReleaseIfMoved_ = isReleaseIfMoved;
	}

	public MouseManager(byte inputAuthority)
	{
		this.inputAuthority_ = inputAuthority;
	}

	public Vector3 Offset
	{
		get
		{
			return this.offset_;
		}
		set
		{
			this.offset_ = value;
		}
	}

	public Rect AvailableTouchRegion
	{
		set
		{
			this.availableTouchRegion_ = value;
		}
	}

	public void Clear()
	{
		this.elems_.Clear();
		this.pushedIndex_ = -1;
		this.selectedIndex_ = -1;
		this.isPushValid_ = false;
		this.fingerIdList_.Clear();
	}

	public void InitSelectionData()
	{
		this.pushedIndex_ = -1;
		this.selectedIndex_ = -1;
		this.isPushValid_ = false;
		this.fingerIdList_.Clear();
	}

	public void Insert(int key, MouseNotifier notifier)
	{
		int num = 0;
		foreach (MouseElem mouseElem in this.elems_)
		{
			if (notifier.GetPriority() > mouseElem.notifier_.GetPriority())
			{
				break;
			}
			num++;
		}
		this.elems_.Insert(num, new MouseElem(key, notifier));
	}

	public void Remove(int key)
	{
		int elem = this.GetElem(key);
		if (elem != -1)
		{
			this.elems_.RemoveAt(elem);
		}
	}

	private void UpdateUsingMouse()
	{
		this.selectedIndex_ = -1;
		this.isPushValid_ = false;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 vector = mousePosition + this.offset_;
		if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
		{
			float num = (float)Screen.width - Input.mousePosition.x;
			float num2 = (float)Screen.height - Input.mousePosition.y;
			float z = Input.mousePosition.z;
			mousePosition = new Vector3(num, num2, z);
			vector = mousePosition - this.offset_;
		}
		if (Input.GetMouseButtonDown(0))
		{
			this.pushedIndex_ = this.GetElem(vector);
			if (this.pushedIndex_ != -1)
			{
				this.isPushValid_ = true;
				this.pushedPos_ = vector;
			}
		}
		else if (this.pushedIndex_ != -1)
		{
			if (!this.elems_[this.pushedIndex_].notifier_.IsEnabled())
			{
				this.pushedIndex_ = -1;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				if (this.availableTouchRegion_.Contains(mousePosition) && this.elems_[this.pushedIndex_].notifier_.Contains(vector))
				{
					this.selectedIndex_ = this.pushedIndex_;
				}
				this.pushedIndex_ = -1;
			}
			else if (Input.GetMouseButton(0))
			{
				if (this.isReleaseIfMoved_)
				{
					this.isPushValid_ = this.pushedPos_ == vector;
					if (!this.isPushValid_)
					{
						this.pushedIndex_ = -1;
					}
				}
				else
				{
					this.isPushValid_ = this.availableTouchRegion_.Contains(mousePosition) && this.elems_[this.pushedIndex_].notifier_.Contains(vector);
				}
			}
		}
	}

	public void Update()
	{
		if (!StageController.IsInputAuthorized(this.inputAuthority_))
		{
			this.InitSelectionData();
			return;
		}
		this.UpdateNotUsingTouchPhase();
	}

	private void UpdateNotUsingTouchPhase()
	{
		this.selectedIndex_ = -1;
		bool flag = this.isPushValid_;
		this.isPushValid_ = false;
		if (this.pushedIndex_ != -1)
		{
			if (this.elems_[this.pushedIndex_].notifier_.IsEnabled())
			{
				bool flag2 = false;
				foreach (Touch touch in Input.touches)
				{
					Vector2 position = touch.position;
					if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
					{
						float num = (float)Screen.width - touch.position.x;
						float num2 = (float)Screen.height - touch.position.y;
						position = new Vector2(num, num2);
					}
					if (touch.fingerId == this.fingerId_)
					{
						flag2 = true;
						this.isPushValid_ = this.availableTouchRegion_.Contains(position) && this.elems_[this.pushedIndex_].notifier_.Contains(position + this.offset_);
						break;
					}
				}
				if (!flag2)
				{
					if (flag)
					{
						this.selectedIndex_ = this.pushedIndex_;
					}
					this.pushedIndex_ = -1;
				}
			}
			else
			{
				this.pushedIndex_ = -1;
			}
		}
		else
		{
			List<int> list = new List<int>();
			if (Input.touchCount > 0)
			{
				foreach (Touch touch2 in Input.touches)
				{
					Vector2 position2 = touch2.position;
					if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
					{
						float num3 = (float)Screen.width - touch2.position.x;
						float num4 = (float)Screen.height - touch2.position.y;
						position2 = new Vector2(num3, num4);
					}
					list.Add(touch2.fingerId);
					if (!this.isPushValid_ && !this.fingerIdList_.Contains(touch2.fingerId) && this.availableTouchRegion_.Contains(position2))
					{
						this.pushedIndex_ = this.GetElem(position2 + this.offset_);
						if (this.pushedIndex_ != -1)
						{
							this.isPushValid_ = true;
							this.fingerId_ = touch2.fingerId;
						}
					}
				}
			}
			this.fingerIdList_.Clear();
			this.fingerIdList_.AddRange(list);
		}
	}

	private int GetElem(int key)
	{
		int num = 0;
		foreach (MouseElem mouseElem in this.elems_)
		{
			if (mouseElem.key_ == key)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private int GetElem(Vector3 pos)
	{
		int num = 0;
		foreach (MouseElem mouseElem in this.elems_)
		{
			if (mouseElem.notifier_.IsEnabled() && mouseElem.notifier_.Contains(pos))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public bool IsSelected()
	{
		return this.selectedIndex_ != -1;
	}

	public bool IsPushed()
	{
		return this.pushedIndex_ != -1 && this.isPushValid_;
	}

	public int GetSelectedKey()
	{
		if (this.IsSelected())
		{
			return this.elems_[this.selectedIndex_].key_;
		}
		return -1;
	}

	public int GetPushedKey()
	{
		if (this.IsPushed())
		{
			return this.elems_[this.pushedIndex_].key_;
		}
		return -1;
	}

	public override string ToString()
	{
		string text = string.Empty;
		foreach (MouseElem mouseElem in this.elems_)
		{
			text = text + mouseElem.key_.ToString() + "\n";
		}
		return text;
	}

	public const int NOT_SELECTED = -1;

	private List<MouseElem> elems_ = new List<MouseElem>();

	private int pushedIndex_ = -1;

	private int selectedIndex_ = -1;

	private bool isPushValid_;

	private bool isReleaseIfMoved_;

	private int fingerId_;

	private Vector3 pushedPos_ = Vector3.zero;

	private Vector3 offset_ = Vector3.zero;

	protected byte inputAuthority_ = 1;

	protected Rect availableTouchRegion_ = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);

	private List<int> fingerIdList_ = new List<int>();
}
