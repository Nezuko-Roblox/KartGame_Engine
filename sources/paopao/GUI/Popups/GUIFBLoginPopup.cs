using System;
using UnityEngine;

public class GUIFBLoginPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(23);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 143f, 115f, 508f, 791f, 1022f, 1022f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.login_message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 176f, 148f, 2f, 497f, 450f, 569f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.login_message_);
		this.cancel_ = new GUIButton(0, new float[]
		{
			172f, 275f, 388f, 325f, 354f, 972f, 429f, 2f, 854f, 218f,
			881f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.cancel_);
		this.mouseManager_.Insert(1, this.cancel_);
		this.yep_ = new GUIButton(0, new float[]
		{
			412f, 275f, 628f, 325f, 354f, 972f, 429f, 2f, 883f, 218f,
			992f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
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
		if (this.cancel_ != null)
		{
			this.cancel_.SetPushed(this.mouseManager_.GetPushedKey() == 1);
		}
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected() || this.backButtonSelected)
		{
			StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
			int num = this.mouseManager_.GetSelectedKey();
			if (this.backButtonSelected)
			{
				this.backButtonSelected = false;
				num = 1;
			}
			if (num == 0)
			{
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE);
				base.SendMessage(1025, monoBehaviourMessage1Param);
			}
			else if (num == 1)
			{
			}
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE)
		{
			this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
			StageController.Instance.InputAutority = this.inputAuthority_;
			if (this.panelManager_.Update())
			{
				this.panelManager_.UpdateMesh(ref this.mesh_);
			}
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	private GUIPanelEx back_;

	private GUIPanelEx login_message_;

	private GUIButton yep_;

	private GUIButton cancel_;

	private bool backButtonSelected;

	private MouseManager mouseManager_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;
}
