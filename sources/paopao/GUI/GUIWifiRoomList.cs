using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIWifiRoomList : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 1;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(12f, 78f, 458f, 474f);
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		maxRange = 78f + (float)this.validRoomCount_ * 120f - (float)Screen.height;
		minRange = 0f;
	}

	protected override void InitializeListctrl()
	{
		for (int i = 0; i < 15; i++)
		{
			this.items_[i] = new GUIWifiRoomListItem(i, this.meshRenderer_, new Vector2(12f, 78f + (float)i * 120f), this);
			if (i < 4)
			{
				this.items_[i].SetEmptyData();
			}
			else
			{
				this.items_[i].Invisible();
			}
			this.items_[i].RegistPanelManager(this.panelManager_);
			this.items_[i].RegistMouseManager(this.mouseManager_);
		}
		this.validRoomCount_ = 4;
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.meshRenderer_.materials[0];
		scrollBarInfo.texture_ = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		scrollBarInfo.scrollbarInfo_ = new float[] { 461f, 98f, 468f, 454f, 560f, 95f, 116f };
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, 0f, num2 - num);
		return scrollBarInfo;
	}

	public override void DoInit()
	{
		this.RegistMonoBehaviour(1064);
		base.DoInit();
	}

	protected override void FirstUpdate()
	{
		MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		if (monoBehaviourMessage2Param != null)
		{
			base.SendMessage(514, monoBehaviourMessage2Param.Initialize(4, 0));
		}
		for (int i = 0; i < 15; i++)
		{
			this.items_[i].Update();
		}
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1)
		{
			base.SelectedId = selectedKey;
		}
		for (int i = 0; i < 15; i++)
		{
			this.items_[i].Update();
		}
	}

	protected override void AfterPanelUpdate()
	{
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST)
		{
			MonoBehaviourMessage2Param<int, Peer[]> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, Peer[]>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				base.SelectedId = monoBehaviourMessage2Param.lparam_;
				this.servers_ = monoBehaviourMessage2Param.rparam_;
				if (base.IsInitialized())
				{
					this.UpdateRoomList();
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.ITEM_TO_CTRL)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param2.lparam_ == 3)
			{
				base.SelectedId = monoBehaviourMessage2Param2.rparam_;
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param3 = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
				if (monoBehaviourMessage2Param3 != null)
				{
					base.SendMessage(514, monoBehaviourMessage2Param3.Initialize(3, base.SelectedId));
				}
			}
		}
	}

	private void UpdateRoomList()
	{
		int num = this.servers_.Length;
		for (int i = 0; i < 15; i++)
		{
			if (i < num)
			{
				this.items_[i].SetData(i, this.servers_[i].name_);
			}
			else if (i < 4)
			{
				this.items_[i].SetEmptyData();
			}
			else
			{
				this.items_[i].Invisible();
			}
		}
		this.validRoomCount_ = Mathf.Max(this.servers_.Length, 4);
		base.Recalculate();
		int num2 = base.SelectedId;
		if (base.SelectedId < 0)
		{
			num2 = 0;
		}
		base.ResetLocalPosition(this.items_[num2].Offset.y - 78f);
	}

	private const int MIN_ROOM_COUNT = 4;

	private const int ROOM_COUNT = 15;

	private const float START_X = 12f;

	private const float START_Y = 78f;

	private const float DISTANCE = 120f;

	private int validRoomCount_;

	private GUIWifiRoomListItem[] items_ = new GUIWifiRoomListItem[15];

	private Peer[] servers_;
}
