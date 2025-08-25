using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class GUIStoreListItem : GUIListCtrlItem
{
	public GUIStoreListItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 0f, 0f, 214f, 122f, 2f, 2f, 93f }, fiaTexture, 5, new Vector3(93f, 358f, 14f), 3f);
		this.itemIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 2f, 126f, 216f, 242f }, fiaTexture, 4, new Vector3(2f, 2f, 13f));
		this.alert_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 1f, 1f, 875f, 47f, 960f, 97f }, fiaTexture2, 3, GUIFontCalculator.DEFAULT_GAP);
		this.alert_.SubMeshIndex = 1;
		this.alert_.Visible = false;
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.itemIcon_.MoveRectByWindowPos(x, y);
		this.alert_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.back_);
		manager.RegistGUIInterface(this.itemIcon_);
		manager.RegistGUIInterface(this.alert_);
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_, this.back_);
	}

	public const int CLICK_ITEM = 1;

	protected GUIPanelEx3PartHorz back_;

	protected GUIPanelEx itemIcon_;

	protected GUIPanelEx alert_;
}
