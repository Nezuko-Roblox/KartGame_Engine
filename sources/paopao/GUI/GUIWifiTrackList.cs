using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIWifiTrackList : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 0;
	}

	protected override void Awake()
	{
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(this.START_X, this.START_Y, 452f, 188f);
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		Rect availableRegion = this.GetAvailableRegion();
		maxRange = 0f;
		minRange = availableRegion.xMax - (this.START_X + (float)this.items_.Length * this.DISTANCE);
	}

	protected override void InitializeListctrl()
	{
		List<int> list = new List<int>();
		foreach (int num in TrackAssetDefinitionManager.RANDOM_IDX_ARRAY)
		{
			list.Add(num);
		}
		int[] assetIdsByListIndexOrder = TrackAssetDefinitionManager.Instance.GetAssetIdsByListIndexOrder();
		List<AssetDefinition> assetDefinitionList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
		foreach (int num2 in assetIdsByListIndexOrder)
		{
			AssetDefinition assetDefinition = assetDefinitionList[num2];
			if (!assetDefinition.Lock)
			{
				list.Add(num2);
			}
		}
		this.items_ = new GUIWifiTrackListItem[list.Count];
		for (int k = 0; k < this.items_.Length; k++)
		{
			this.items_[k] = new GUIWifiTrackListItem(k, this.meshRenderer_, new Vector2(this.START_X + (float)k * this.DISTANCE, this.START_Y), this);
			this.items_[k].SetData(list[k]);
			this.items_[k].RegistPanelManager(this.panelManager_);
			this.items_[k].RegistMouseManager(this.mouseManager_);
		}
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.meshRenderer_.materials[1];
		scrollBarInfo.texture_ = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		scrollBarInfo.scrollbarInfo_ = new float[] { 18f, 191f, 452f, 198f, 569f, 95f, 590f };
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, num2 - num, 0f);
		return scrollBarInfo;
	}

	public override void DoInit()
	{
		TrackAssetDefinitionManager.Instance.Refresh();
		this.RegistMonoBehaviour(1033);
		base.DoInit();
	}

	protected override void FirstUpdate()
	{
		base.SelectedId = AssetSelection.selection_[2];
		for (int i = 0; i < this.items_.Length; i++)
		{
			this.items_[i].Update();
		}
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		for (int i = 0; i < this.items_.Length; i++)
		{
			this.items_[i].Update();
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (msg.type_ == MonoBehaviourMessageType.ITEM_TO_CTRL)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param.lparam_ == 0)
			{
				base.SelectedId = monoBehaviourMessage2Param.rparam_;
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
				base.SendMessage(515, monoBehaviourMessage2Param2.Initialize(3, base.SelectedId));
			}
		}
	}

	private GUIWifiTrackListItem[] items_;

	private float START_X = 18f;

	private float START_Y = 84f;

	private float DISTANCE = 130f;
}
