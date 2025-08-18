using System;
using UnityEngine;

public class GUIPatchSummaryPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(270);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 12f, 12f, 2f, 2f, 458f, 298f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.title_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 22f, 22f, 2f, 1004f, 438f, 1022f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.content_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 22f, 58f, 2f, 342f, 438f, 504f }, fiaTexture, 3, new Vector3(2f, 2f, 102f));
		this.panelManager_.RegistGUIInterface(this.back_);
		this.panelManager_.RegistGUIInterface(this.title_);
		this.panelManager_.RegistGUIInterface(this.content_);
		this.yep_ = new GUIButton(0, new float[]
		{
			174f, 262f, 311f, 302f, 138f, 300f, 225f, 46f, 300f, 96f,
			316f
		}, fiaTexture, 2, new Vector3(89f, 0f, 1f), new Vector3(2f, 2f, 102f));
		this.yep_.SetNameUV(1);
		this.panelManager_.RegistGUIInterface(this.yep_);
		this.mouseManager_.Insert(0, this.yep_);
		base.gameObject.SetActiveRecursively(false);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.mouseManager_.Update();
		if (this.yep_ != null)
		{
			this.yep_.SetPushed(this.mouseManager_.GetPushedKey() == 0);
		}
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected() && this.mouseManager_.GetSelectedKey() == 0)
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			if (this.index_ == 0)
			{
				this.yep_.SetNameUV(0);
				this.content_.SetUV(1);
				this.index_++;
			}
			else if (this.index_ == 1)
			{
				StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.PATCH_SUMMARY_POPUP_MESSAGE)
		{
			if (this.panelManager_.Update())
			{
				this.panelManager_.UpdateMesh(ref this.mesh_);
			}
			this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
			StageController.Instance.InputAutority = this.inputAuthority_;
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public void BackButtonAction(string msg)
	{
	}

	private GUIPanelEx back_;

	private GUIPanelEx title_;

	private GUIPanelEx content_;

	private GUIButton yep_;

	private MouseManager mouseManager_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;

	private int index_;
}
