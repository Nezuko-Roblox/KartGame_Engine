using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIPurchaseConfirmPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			this.store_ = FiaStore.Inst;
		}
		else
		{
			this.store_ = MockFiaStore.Inst;
		}
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(272);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 144f, 95f, 2f, 2f, 516f, 305f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.login_message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 177f, 128f, 518f, 184f, 966f, 364f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
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
			413f, 327f, 629f, 377f, 220f, 307f, 295f, 2f, 307f, 218f,
			334f
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
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			int num = this.mouseManager_.GetSelectedKey();
			if (this.backButtonSelected)
			{
				this.backButtonSelected = false;
				num = 1;
			}
			if (num == 0)
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
				{
					int num2 = androidJavaClass.CallStatic<int>("purchaseRequest", new object[] { "popPs1" });
				}
			}
			else if (num == 1)
			{
				StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void purchaseComplete(string msg)
	{
		if (!Env.IsDesktop)
		{
			iOSEvent.ShowIndicator();
		}
		this.Hide();
		FiaCoroutine fiaCoroutine = new FiaCoroutine(this.store_.Purchase("ps1"), new OnSuccess(this.PurchaseSuccess), new OnFailure(this.PurchaseFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void Hide()
	{
		this.back_.Visible = false;
		this.login_message_.Visible = false;
		this.yep_.Visible = false;
		this.cancel_.Visible = false;
	}

	private void PurchaseSuccess()
	{
		if (Debug.isDebugBuild)
		{
			TrackAssetDefinitionManager.Instance.Refresh();
		}
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_SHOPLIST);
		monoBehaviourMessage1Param.Initialize(-1);
		base.SendMessage(1047, monoBehaviourMessage1Param);
		MonoBehaviourMessage1Param<string> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<string>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO);
		base.SendMessage(1046, monoBehaviourMessage1Param2.Initialize("ps1"));
		base.SendMessage(1045, monoBehaviourMessage1Param2.Initialize("ps1"));
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
		base.gameObject.SetActiveRecursively(false);
	}

	private void PurchaseFailure(Exception ex)
	{
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
		base.gameObject.SetActiveRecursively(false);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.PURCHASE_CONFIRM_POPUP_MESSAGE)
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

	private const string PRODUCT_TO_PURCHASE = "ps1";

	private GUIPanelEx back_;

	private GUIPanelEx login_message_;

	private GUIButton yep_;

	private GUIButton cancel_;

	private FiaStore store_;

	private bool backButtonSelected;

	private MouseManager mouseManager_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;
}
