using System;
using UnityEngine;

internal class GUIWifiTrackListItem : GUIListCtrlItem
{
	public GUIWifiTrackListItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.trackIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 2f, 108f, 128f, 212f }, fiaTexture2, 4, new Vector3(2f, 2f, 16f));
		this.trackIcon_.SubMeshIndex = 0;
		this.trackIcon_.SetTouchRegionByWindowRect(new Rect(0f, 0f, 78f, 76f));
		this.trackIcon_.VerticeColor = Color.gray;
		this.borderIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 746f, 280f, 872f, 384f }, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		this.borderIcon_.SubMeshIndex = 1;
		this.borderIcon_.Visible = false;
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_, this.trackIcon_);
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.trackIcon_.MoveRectByWindowPos(x, y);
		this.borderIcon_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.trackIcon_);
		manager.RegistGUIInterface(this.borderIcon_);
	}

	public void SetData(int trackIdx)
	{
		this.trackIdx_ = trackIdx;
		this.trackIcon_.SetUV(trackIdx);
	}

	public override void Update()
	{
		this.trackIcon_.VerticeColor = ((this.listctrl_.SelectedId != this.trackIdx_) ? Color.gray : Color.white);
		int pushedKey = this.mouseManager_.GetPushedKey();
		this.borderIcon_.SetUV((pushedKey != this.id_) ? 0 : 1);
		this.borderIcon_.Visible = this.listctrl_.SelectedId == this.trackIdx_ || pushedKey == this.id_;
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey == this.id_)
		{
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			base.SendMessageToListCtrl(0, this.trackIdx_);
		}
	}

	public const int CLICK_TRACK = 0;

	private GUIPanelEx trackIcon_;

	private GUIPanelEx borderIcon_;

	private int trackIdx_;
}
