using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIRestorePurchasesPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(271);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 144f, 95f, 2f, 2f, 516f, 305f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.login_message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 177f, 128f, 518f, 2f, 966f, 182f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.login_message_);
		this.cancel_ = new GUIButton(0, new float[]
		{
			173f, 327f, 389f, 377f, 220f, 307f, 295f, 2f, 365f, 218f,
			392f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.cancel_);
		this.mouseManager_.Insert(1, this.cancel_);
		this.yep_ = new GUIButton(0, new float[]
		{
			413f, 327f, 629f, 377f, 220f, 307f, 295f, 2f, 336f, 218f,
			363f
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
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.ITEM_TO_CTRL);
				monoBehaviourMessage2Param.Initialize(1, 0);
				base.SendMessage(1047, monoBehaviourMessage2Param);
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
		if (msg.type_ == MonoBehaviourMessageType.RESTORE_PURCHASES_POPUP_MESSAGE)
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
