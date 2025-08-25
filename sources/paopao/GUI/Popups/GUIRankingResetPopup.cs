using System;
using UnityEngine;

public class GUIRankingResetPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(267);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 143f, 115f, 508f, 791f, 1022f, 1022f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 176f, 148f, 2f, 571f, 450f, 679f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.message_);
		this.yep_ = new GUIButton(0, new float[]
		{
			292f, 275f, 508f, 325f, 354f, 972f, 429f, 2f, 854f, 218f,
			881f
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
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected() && this.mouseManager_.GetSelectedKey() == 0)
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
			base.gameObject.SetActiveRecursively(false);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.RANKING_LOADING_MESSAGE)
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
	}

	private GUIPanelEx back_;

	private GUIPanelEx message_;

	private GUIButton yep_;

	private MouseManager mouseManager_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;
}
