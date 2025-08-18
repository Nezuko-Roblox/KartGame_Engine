using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIStorePurchaseButton : FiaGUILayer
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
			this.store_ = FiaStore.Inst;
		}
		this.mouseManager_ = new MouseManager();
		this.RegistMonoBehaviour(1045);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.button_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 351f, 396f, 650f, 2f, 976f, 80f }, fiaTexture, 5, GUIFontCalculator.Y2_GAP);
		this.current_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 600f, 414f, 680f, 252f, 736f, 300f }, fiaTexture, 4, GUIFontCalculator.Y2_GAP);
		this.cost_ = new GUIString(new Vector2(622f, 423f), 2, 10, GUIString.Alignment.RIGHT, 3, fiaTexture2);
		this.cost_.SubMeshIndex = 1;
		this.purchasedIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 351f, 396f, 650f, 162f, 976f, 240f }, fiaTexture, 6, GUIFontCalculator.Y2_GAP);
		this.panelManager_.RegistGUIInterface(this.cost_);
		this.panelManager_.RegistGUIInterface(this.button_);
		this.panelManager_.RegistGUIInterface(this.current_);
		this.panelManager_.RegistGUIInterface(this.purchasedIcon_);
		this.button_.Visible = false;
		this.current_.Visible = false;
		this.purchasedIcon_.Visible = false;
		this.cost_.SetString(string.Empty);
		this.cost_.SetColor(FiaColor.black);
		this.mouseManager_.Insert(this.id_, this.button_);
	}

	protected override void Update()
	{
		base.Update();
		if (this.purchased_ || this.currProduct_ == null)
		{
			this.button_.Visible = false;
			this.current_.Visible = false;
			this.purchasedIcon_.Visible = true;
			return;
		}
		this.button_.Visible = true;
		this.current_.Visible = true;
		this.purchasedIcon_.Visible = false;
		this.mouseManager_.Update();
		if (this.mouseManager_.GetPushedKey() == this.id_)
		{
			this.button_.SetUV(1);
			StageController.Instance.PlaySound(StageController.FxType.SELECT);
			if (this.currProduct_.ID == this.CONFIRM_PURCHASE_ON && this.store_.PurchasedProductList.Count > 0)
			{
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.PURCHASE_CONFIRM_POPUP_MESSAGE);
				monoBehaviourMessage1Param.Initialize(-1);
				base.SendMessage(272, monoBehaviourMessage1Param);
			}
			else
			{
				using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
				{
					int num = androidJavaClass.CallStatic<int>("purchaseRequest", new object[] { this.currProduct_.ID });
				}
			}
		}
		else
		{
			this.button_.SetUV(0);
		}
	}

	public void requestPurchasedHistory()
	{
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
		{
			int num = androidJavaClass.CallStatic<int>("requestPurchasedHistory", new object[0]);
		}
	}

	public void purchaseComplete(string msg)
	{
		this.prevInputAuthority_ = StageController.Instance.InputAutority;
		StageController.Instance.InputAutority = 0;
		if (!Env.IsDesktop)
		{
			iOSEvent.ShowIndicator();
		}
		FiaCoroutine fiaCoroutine = new FiaCoroutine(this.store_.Purchase(msg), new OnSuccess(this.PurchaseSuccess), new OnFailure(this.PurchaseFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	protected void SetData(Product p)
	{
		this.cost_.SetString(p.PriceString);
		this.currProduct_ = p;
		this.purchased_ = true;
		foreach (string text in p.Unlocks)
		{
			if (!this.store_.UnlockedProductList.Contains(text))
			{
				this.purchased_ = false;
				break;
			}
		}
		if (this.purchased_ || this.currProduct_ == null)
		{
			this.button_.Visible = false;
			this.current_.Visible = false;
			this.purchasedIcon_.Visible = true;
			this.cost_.SetString(string.Empty);
			return;
		}
	}

	private void PurchaseSuccess()
	{
		TrackAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_SHOPLIST);
		monoBehaviourMessage1Param.Initialize(-1);
		base.SendMessage(1047, monoBehaviourMessage1Param);
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.prevInputAuthority_;
		this.SetData(this.currProduct_);
	}

	private void PurchaseFailure(Exception ex)
	{
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.prevInputAuthority_;
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (base.gameObject.active && msg.type_ == MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO)
		{
			MonoBehaviourMessage1Param<string> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<string>)msg;
			if (monoBehaviourMessage1Param != null && monoBehaviourMessage1Param.param_ != null)
			{
				this.SetData(this.store_.FindProductWithID(monoBehaviourMessage1Param.param_));
			}
		}
	}

	private GUIPanelEx button_;

	private GUIPanelEx current_;

	private GUIPanelEx purchasedIcon_;

	private GUIString cost_;

	private FiaStore store_;

	private Product currProduct_;

	private byte prevInputAuthority_;

	private bool purchased_;

	private string CONFIRM_PURCHASE_ON = "ps1";

	private MouseManager mouseManager_;
}
