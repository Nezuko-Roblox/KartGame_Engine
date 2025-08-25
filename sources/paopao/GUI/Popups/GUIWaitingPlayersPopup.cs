using System;
using UnityEngine;

public class GUIWaitingPlayersPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(268);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 143f, 115f, 508f, 791f, 1022f, 1022f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 176f, 148f, 502f, 180f, 950f, 252f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.message_);
		this.cancel_ = new GUIButton(0, new float[]
		{
			292f, 275f, 508f, 325f, 354f, 972f, 429f, 2f, 995f, 218f,
			1022f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.cancel_);
		this.mouseManager_.Insert(0, this.cancel_);
		base.gameObject.SetActiveRecursively(false);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.mouseManager_.Update();
		if (this.cancel_ != null)
		{
			this.cancel_.SetPushed(this.mouseManager_.GetPushedKey() == 0);
		}
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected() || this.backButtonSelected)
		{
			int num = this.mouseManager_.GetSelectedKey();
			if (this.backButtonSelected)
			{
				this.backButtonSelected = false;
				num = 0;
			}
			if (num == 0)
			{
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
				MonoBehaviourExCenter.Instance.SendMessage(0, 515, monoBehaviourMessage2Param.Initialize(1, 0));
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
				StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg != null && msg.type_ == MonoBehaviourMessageType.WAITING_PLAYERS_MESSAGE)
		{
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param.param_ == 0)
			{
				this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
				StageController.Instance.InputAutority = this.inputAuthority_;
				if (this.panelManager_.Update())
				{
					this.panelManager_.UpdateMesh(ref this.mesh_);
				}
				base.gameObject.SetActiveRecursively(true);
			}
			else if (monoBehaviourMessage1Param.param_ == 1 && base.gameObject.active)
			{
				StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	private GUIPanelEx back_;

	private GUIPanelEx message_;

	private GUIButton cancel_;

	private bool backButtonSelected;

	private MouseManager mouseManager_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;
}
