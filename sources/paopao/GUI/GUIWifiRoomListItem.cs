using System;
using UnityEngine;

internal class GUIWifiRoomListItem : GUIListCtrlItem
{
	public GUIWifiRoomListItem(int listidx, MeshRenderer _meshRenderer, Vector2 offset, GUIListCtrl listctrl)
		: base(listidx, _meshRenderer, offset, listctrl)
	{
	}

	protected override void Initialize()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 0f, 0f, 446f, 116f, 2f, 2f, 93f }, fiaTexture2, 5, new Vector3(93f, 0f, 1f), 3f);
		this.join_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 313f, 33f, 618f, 280f, 744f, 333f }, fiaTexture2, 2, GUIFontCalculator.Y2_GAP);
		this.roomName_ = new GUIString(new Vector2(23f, 44f), 2, 10, GUIString.Alignment.LEFT, 0, fiaTexture);
		this.roomName_.SubMeshIndex = 1;
	}

	public int GetRoomIdx()
	{
		return this.roomIdx_;
	}

	public override void RegistMouseManager(MouseManager manager)
	{
		base.RegistMouseManager(manager);
		manager.Insert(this.id_, this.back_);
	}

	public void SetData(int roomIdx, string roomName)
	{
		this.roomIdx_ = roomIdx;
		this.back_.Visible = true;
		string text = roomName;
		if (text.Length > 10)
		{
			text = text.Substring(0, 9) + '=';
		}
		this.roomName_.SetString(text);
		this.join_.Visible = true;
	}

	public override void Update()
	{
		this.join_.SetUV((int)(Time.time * 2f % 2f));
		if (this.roomIdx_ != -1)
		{
			int pushedKey = this.mouseManager_.GetPushedKey();
			this.back_.SetUV((pushedKey != this.id_) ? ((this.listctrl_.SelectedId != this.roomIdx_) ? 0 : 1) : 2);
			if (this.mouseManager_.GetSelectedKey() == this.id_)
			{
				base.SendMessageToListCtrl(3, this.roomIdx_);
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
				MonoBehaviourExCenter.Instance.SendMessage(0, 514, monoBehaviourMessage2Param.Initialize(2, this.roomIdx_));
				StageController.Instance.PlaySound(StageController.FxType.SELECT);
			}
		}
	}

	public override void MoveRectByWindowPos(float x, float y)
	{
		this.back_.MoveRectByWindowPos(x, y);
		this.roomName_.MoveRectByWindowPos(x, y);
		this.join_.MoveRectByWindowPos(x, y);
	}

	public override void RegistPanelManager(GUIPanelManager manager)
	{
		manager.RegistGUIInterface(this.back_);
		manager.RegistGUIInterface(this.roomName_);
		manager.RegistGUIInterface(this.join_);
	}

	public void Invisible()
	{
		this.roomIdx_ = -1;
		this.back_.Visible = false;
		this.roomName_.SetString(string.Empty);
		this.join_.Visible = false;
	}

	public void SetEmptyData()
	{
		this.roomIdx_ = -1;
		this.back_.Visible = true;
		this.back_.SetUV(3);
		this.roomName_.SetString(string.Empty);
		this.join_.Visible = false;
	}

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx join_;

	public GUIString roomName_;

	private int roomIdx_;
}
