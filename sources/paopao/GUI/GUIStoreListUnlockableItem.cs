using System;
using UnityEngine;

public abstract class GUIStoreListUnlockableItem : GUIStoreListItem
{
	public GUIStoreListUnlockableItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.purchasedIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 2f, 2f, 206f, 2f, 282f, 77f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.itemIcon_.VerticeColor = Color.gray;
		this.purchasedIcon_.VerticeColor = Color.gray;
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		base.RegistPanelManager(manager);
		manager.RegistGUIInterface(this.purchasedIcon_);
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		base.MoveRectByWindowPos(x, y);
		this.purchasedIcon_.MoveRectByWindowPos(x, y);
	}

	public override void Update()
	{
		int pushedKey = this.mouseManager_.GetPushedKey();
		if (pushedKey == this.id_ || this.listctrl_.SelectedId == this.listIdx_)
		{
			this.back_.SetUV(1);
			this.itemIcon_.VerticeColor = Color.white;
			this.purchasedIcon_.VerticeColor = Color.white;
		}
		else
		{
			this.back_.SetUV(0);
			this.itemIcon_.VerticeColor = Color.gray;
			this.purchasedIcon_.VerticeColor = Color.gray;
		}
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey == this.id_)
		{
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			base.SendMessageToListCtrl(1, this.listIdx_);
		}
	}

	public void SetData(int iconIndex, bool purchased, bool alert)
	{
		this.itemIcon_.SetUV(iconIndex);
		this.purchasedIcon_.Visible = purchased;
		this.alert_.Visible = alert;
	}

	protected GUIPanelEx purchasedIcon_;
}
