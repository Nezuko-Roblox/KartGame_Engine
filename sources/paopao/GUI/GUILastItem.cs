using System;
using UnityEngine;

public class GUILastItem : GUIListCtrlItem
{
	public GUILastItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		this.backId_ = this.id_ * 2;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[4].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 0f, 0f, 446f, 116f, 2f, 2f, 93f }, fiaTexture, 5, new Vector3(93f, 788f, 14f), 3f);
		this.moreImage_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 36f, 2f, 254f, 448f, 298f }, fiaTexture2, 4, GUIFontCalculator.DEFAULT_GAP);
		this.moreImage_.SubMeshIndex = 4;
		this.back_.Visible = true;
		this.moreImage_.Visible = true;
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.moreImage_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.moreImage_);
		manager.RegistGUIInterface(this.back_);
	}

	public override void Update()
	{
		int pushedKey = this.mouseManager_.GetPushedKey();
		this.back_.SetUV((pushedKey != this.backId_) ? ((!this.IsSelectedItem()) ? 0 : 1) : 2);
		if (pushedKey == this.backId_)
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			base.SendMessageToListCtrl(-1, -1);
		}
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_ * 2, this.back_);
	}

	private bool IsSelectedItem()
	{
		return this.listctrl_.SelectedId == this.backId_;
	}

	public void Invisible()
	{
		this.back_.Visible = false;
		this.moreImage_.Visible = false;
	}

	public const int CLICK_STORE = -1;

	private GUIPanelEx moreImage_;

	private GUIPanelEx3PartHorz back_;

	private int backId_;
}
