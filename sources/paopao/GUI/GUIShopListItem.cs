using System;
using UnityEngine;

public class GUIShopListItem : GUIListCtrlItem
{
	public GUIShopListItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[5].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture4 = new FiaTexture(this.meshRenderer_.materials[3].mainTexture);
		FiaTexture fiaTexture5 = new FiaTexture(this.meshRenderer_.materials[4].mainTexture);
		FiaTexture fiaTexture6 = new FiaTexture(this.meshRenderer_.materials[6].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 0f, 0f, 446f, 116f, 2f, 2f, 93f }, fiaTexture2, 5, new Vector3(93f, 788f, 14f), 3f);
		this.itemIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 6f, 6f, 2f, 2f, 128f, 106f }, fiaTexture3, 4, new Vector3(2f, 2f, 16f));
		this.itemIcon_.SubMeshIndex = 1;
		this.itemName_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 152f, 8f, 2f, 2f, 254f, 32f }, fiaTexture4, 3, new Vector3(2f, 2f, 12f));
		this.itemName_.SubMeshIndex = 3;
		this.itemDesc_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 152f, 48f, 2f, 2f, 286f, 46f }, fiaTexture5, 3, GUIFontCalculator.Y2_GAP);
		this.itemDesc_.SubMeshIndex = 4;
		this.lockIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 11f, 38f, 881f, 2f, 995f, 42f }, fiaTexture2, 3, GUIFontCalculator.Y2_GAP);
		this.lockIcon_.SubMeshIndex = 2;
		this.itemCost_ = new GUIString(new Vector2(68f, 32f), 2, 1, GUIString.Alignment.LEFT, 4, fiaTexture);
		this.itemCost_.SubMeshIndex = 5;
		this.stat_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 165f, 47f, 188f, 436f, 471f, 498f }, fiaTexture2, 4, GUIFontCalculator.DEFAULT_GAP);
		for (int i = 0; i < 3; i++)
		{
			GUIGauge[] array = this.gauge_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 165f, 0f, 340f, 0f, 300f, 420f, 304f, 434f };
			array2[1] = (float)(48 + i * 22);
			array2[3] = (float)(48 + i * 22 + 14);
			array[num] = new GUIGauge(instance.CreateByWindowSpace(num2, array2, fiaTexture2, 4, GUIFontCalculator.DEFAULT_GAP));
		}
		this.moreItems_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 36f, 2f, 254f, 448f, 298f }, fiaTexture6, 4, GUIFontCalculator.Y2_GAP);
		this.moreItems_.SubMeshIndex = 6;
		this.alert_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 875f, 47f, 960f, 97f }, fiaTexture2, 2, GUIFontCalculator.DEFAULT_GAP);
		this.alert_.SubMeshIndex = 2;
		this.alert_.Visible = false;
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.itemIcon_.MoveRectByWindowPos(x, y);
		this.lockIcon_.MoveRectByWindowPos(x, y);
		this.itemCost_.MoveRectByWindowPos(x, y);
		this.itemName_.MoveRectByWindowPos(x, y);
		this.itemDesc_.MoveRectByWindowPos(x, y);
		this.stat_.MoveRectByWindowPos(x, y);
		foreach (GUIGauge guigauge in this.gauge_)
		{
			guigauge.MoveRectByWindowPos(x, y);
		}
		this.moreItems_.MoveRectByWindowPos(x, y);
		this.alert_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.back_);
		manager.RegistGUIInterface(this.itemIcon_);
		manager.RegistGUIInterface(this.lockIcon_);
		manager.RegistGUIInterface(this.itemCost_);
		manager.RegistGUIInterface(this.itemName_);
		manager.RegistGUIInterface(this.itemDesc_);
		manager.RegistGUIInterface(this.stat_);
		foreach (GUIGauge guigauge in this.gauge_)
		{
			manager.RegistGUIInterface(guigauge.Panel);
		}
		manager.RegistGUIInterface(this.moreItems_);
		manager.RegistGUIInterface(this.alert_);
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_, this.back_);
	}

	public override void Update()
	{
		int pushedKey = this.mouseManager_.GetPushedKey();
		if (this.itemLockType_ == GUIShopListItem.enItemLockType.NONE)
		{
			this.back_.SetUV((pushedKey != this.id_) ? ((this.listctrl_.SelectedId != this.listIdx_) ? 0 : 1) : 2);
			this.itemIcon_.VerticeColor = Color.white;
		}
		else if (this.itemLockType_ == GUIShopListItem.enItemLockType.CASH)
		{
			this.back_.SetUV((pushedKey != this.id_) ? 4 : 5);
		}
		else if (this.itemLockType_ == GUIShopListItem.enItemLockType.QUEST)
		{
			this.back_.SetUV((pushedKey != this.id_) ? 6 : 7);
		}
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey == this.id_)
		{
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			if (this.goToStore_)
			{
				base.SendMessageToListCtrl(-1, -1);
			}
			else
			{
				base.SendMessageToListCtrl(1, this.listIdx_);
			}
		}
	}

	public void SetData(int itemId, string itemName, GUIShopListItem.enItemLockType lockType, string cost_, bool alert)
	{
		this.goToStore_ = false;
		this.itemLockType_ = lockType;
		this.itemId_ = itemId;
		this.back_.Visible = true;
		if (lockType == GUIShopListItem.enItemLockType.NONE)
		{
			this.back_.SetUV((this.listctrl_.SelectedId != this.listIdx_) ? 0 : 1);
		}
		else
		{
			this.back_.SetUV((lockType != GUIShopListItem.enItemLockType.CASH) ? 6 : 4);
		}
		this.itemIcon_.Visible = true;
		this.itemIcon_.VerticeColor = ((this.itemLockType_ != GUIShopListItem.enItemLockType.NONE) ? Color.gray : Color.white);
		this.itemIcon_.SetUV(CharacterAssetDefinitionManager.Instance.GetMainIconIdx(this.itemId_));
		this.lockIcon_.Visible = lockType != GUIShopListItem.enItemLockType.NONE;
		this.lockIcon_.SetUV((lockType != GUIShopListItem.enItemLockType.CASH) ? 0 : 1);
		this.itemCost_.SetString((lockType != GUIShopListItem.enItemLockType.CASH) ? string.Empty : cost_);
		this.itemName_.Visible = true;
		this.itemName_.SetUV(this.itemId_ * 2);
		this.itemDesc_.Visible = true;
		this.itemDesc_.SetUV(this.itemId_);
		this.stat_.Visible = false;
		foreach (GUIGauge guigauge in this.gauge_)
		{
			guigauge.Panel.Visible = false;
		}
		this.moreItems_.Visible = false;
		this.alert_.Visible = alert;
	}

	public void SetData(int itemId, string itemName, float[] stat, GUIShopListItem.enItemLockType lockType, string cost_, bool alert)
	{
		this.goToStore_ = false;
		this.itemLockType_ = lockType;
		this.itemId_ = itemId;
		this.back_.Visible = true;
		if (lockType == GUIShopListItem.enItemLockType.NONE)
		{
			this.back_.SetUV((this.listctrl_.SelectedId != this.listIdx_) ? 0 : 1);
		}
		else
		{
			this.back_.SetUV((lockType != GUIShopListItem.enItemLockType.CASH) ? 6 : 4);
		}
		this.itemIcon_.Visible = true;
		this.itemIcon_.VerticeColor = ((this.itemLockType_ != GUIShopListItem.enItemLockType.NONE) ? Color.gray : Color.white);
		this.itemIcon_.SetUV(KartAssetDefinitionManager.Instance.GetMainIconIdx(this.itemId_));
		this.lockIcon_.Visible = lockType != GUIShopListItem.enItemLockType.NONE;
		this.lockIcon_.SetUV((lockType != GUIShopListItem.enItemLockType.CASH) ? 0 : 1);
		this.itemCost_.SetString((lockType != GUIShopListItem.enItemLockType.CASH) ? string.Empty : cost_);
		this.itemName_.Visible = true;
		this.itemName_.SetUV(this.itemId_ * 2 + 1);
		this.itemDesc_.Visible = false;
		this.itemDesc_.SetUV(this.itemId_);
		this.stat_.Visible = true;
		for (int i = 0; i < this.gauge_.Length; i++)
		{
			this.gauge_[i].Panel.Visible = true;
			this.gauge_[i].SetFactor(stat[i]);
			this.gauge_[i].SetColor((this.itemLockType_ != GUIShopListItem.enItemLockType.NONE) ? Color.gray : Color.white);
		}
		this.moreItems_.Visible = false;
		this.alert_.Visible = alert;
	}

	public void SetData(GUIMoreItemType itemType)
	{
		this.goToStore_ = true;
		this.itemLockType_ = GUIShopListItem.enItemLockType.NONE;
		this.back_.SetUV(0);
		this.back_.Visible = true;
		this.itemIcon_.Visible = false;
		this.itemName_.Visible = false;
		this.itemDesc_.Visible = false;
		this.lockIcon_.Visible = false;
		this.stat_.Visible = false;
		foreach (GUIGauge guigauge in this.gauge_)
		{
			guigauge.Panel.Visible = false;
		}
		this.moreItems_.Visible = true;
		this.moreItems_.SetUV((int)itemType);
		this.alert_.Visible = false;
	}

	public void Invisible()
	{
		this.back_.Visible = false;
		this.itemIcon_.Visible = false;
		this.lockIcon_.Visible = false;
		this.itemName_.Visible = false;
		this.itemDesc_.Visible = false;
		this.moreItems_.Visible = false;
		this.itemCost_.SetString(string.Empty);
		this.stat_.Visible = false;
		foreach (GUIGauge guigauge in this.gauge_)
		{
			guigauge.Panel.Visible = false;
		}
		this.alert_.Visible = false;
	}

	public GUIShopListItem.enItemLockType ItemLockType
	{
		get
		{
			return this.itemLockType_;
		}
		set
		{
			this.itemLockType_ = value;
			this.back_.Visible = true;
			if (this.itemLockType_ == GUIShopListItem.enItemLockType.NONE)
			{
				this.back_.SetUV((this.listctrl_.SelectedId != this.listIdx_) ? 0 : 1);
			}
			else
			{
				this.back_.SetUV((this.itemLockType_ != GUIShopListItem.enItemLockType.CASH) ? 6 : 4);
			}
			this.itemIcon_.Visible = true;
			this.itemIcon_.VerticeColor = ((this.itemLockType_ != GUIShopListItem.enItemLockType.NONE) ? Color.gray : Color.white);
			this.lockIcon_.Visible = this.itemLockType_ != GUIShopListItem.enItemLockType.NONE;
			this.lockIcon_.SetUV((this.itemLockType_ != GUIShopListItem.enItemLockType.CASH) ? 0 : 1);
		}
	}

	public int ItemId
	{
		get
		{
			return this.itemId_;
		}
	}

	public const int CLICK_STORE = -1;

	public const int CLICK_ITEM = 1;

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx itemIcon_;

	private GUIPanelEx itemName_;

	private GUIPanelEx itemDesc_;

	private GUIPanelEx lockIcon_;

	private GUIPanelEx moreItems_;

	private GUIString itemCost_;

	private GUIPanelEx alert_;

	private GUIPanelEx stat_;

	private GUIGauge[] gauge_ = new GUIGauge[3];

	private bool goToStore_;

	private int itemId_;

	private GUIShopListItem.enItemLockType itemLockType_;

	public enum enItemLockType
	{
		NONE,
		CASH,
		QUEST
	}

	private enum enBackUVIdx
	{
		NORMAL,
		SELECTED,
		PUSHED,
		EMPTY,
		CASH,
		CASH_PUSHED,
		QUEST,
		QUEST_PUSHED
	}
}
