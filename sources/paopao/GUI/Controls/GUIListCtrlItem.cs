using System;
using UnityEngine;

public abstract class GUIListCtrlItem
{
	public GUIListCtrlItem(int listIdx, GUIAtlas atlas, Vector2 offset, GUIListCtrl listctrl)
	{
		this.id_ = ++GUIListCtrlItem.LIST_ITEM_COUNTER;
		this.atlas_ = atlas;
		this.offset_ = offset;
		this.listctrl_ = listctrl;
		this.listIdx_ = listIdx;
		this.Initialize();
		this.MoveRectByWindowPos(this.offset_.x, this.offset_.y);
	}

	public GUIListCtrlItem(int listIdx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
	{
		this.id_ = ++GUIListCtrlItem.LIST_ITEM_COUNTER;
		this.meshRenderer_ = _meshRenderer;
		this.offset_ = offset;
		this.listctrl_ = listctrl;
		this.listIdx_ = listIdx;
		this.Initialize();
		this.MoveRectByWindowPos(this.offset_.x, this.offset_.y);
	}

	public int Id
	{
		get
		{
			return this.id_;
		}
	}

	public Vector2 Offset
	{
		get
		{
			return this.offset_;
		}
		set
		{
			this.MoveRectByWindowPos(-this.offset_.x, -this.offset_.y);
			this.offset_ = value;
			this.MoveRectByWindowPos(this.offset_.x, this.offset_.y);
		}
	}

	public int ListIndex
	{
		get
		{
			return this.listIdx_;
		}
	}

	public virtual void RegistMouseManager(MouseManager manager)
	{
		this.mouseManager_ = manager;
	}

	public void SendMessageToListCtrl(int type, int param)
	{
		MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM_TO_CTRL);
		monoBehaviourMessage2Param.Initialize(type, param);
		this.listctrl_.ReceiveMessage(0, monoBehaviourMessage2Param);
	}

	protected virtual void Initialize()
	{
	}

	public virtual void RegistPanelManager(GUIPanelManager manager)
	{
	}

	public virtual void Update()
	{
	}

	public virtual void MoveRectByWindowPos(float x, float y)
	{
	}

	protected GUIAtlas atlas_;

	protected MeshRenderer meshRenderer_;

	protected Vector2 offset_;

	protected GUIListCtrl listctrl_;

	protected int listIdx_;

	protected int id_;

	protected MouseManager mouseManager_;

	private static int LIST_ITEM_COUNTER;
}
