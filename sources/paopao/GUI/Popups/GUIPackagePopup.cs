using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIPackagePopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.store_ = FiaStore.Inst;
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(1051);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 5f, 61f, 277f, 297f, 1f, 2f, 96f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.back_.RegistPanelManager(this.panelManager_);
		float[] array = new float[] { 24f, 98f, 258f, 250f, 8f, 26f, 242f, 178f };
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.isVert_ = true;
		scrollBarInfo.viewRegion_ = GUIBase.ConvertWSToUS(array, 0);
		scrollBarInfo.listRegion_ = GUIBase.ConvertWSToUS(array, 4);
		scrollBarInfo.material_ = this.meshRenderer_.material;
		scrollBarInfo.texture_ = fiaTexture;
		scrollBarInfo.scrollbarInfo_ = new float[] { 266f, 116f, 270f, 250f, 189f, 2f, 17f };
		GUIScrollBar guiscrollBar = null;
		if (this.scrollBarPrefab_ != null && scrollBarInfo != null)
		{
			guiscrollBar = (GUIScrollBar)global::UnityEngine.Object.Instantiate(this.scrollBarPrefab_);
			guiscrollBar.transform.parent = base.transform.parent;
			guiscrollBar.info_ = scrollBarInfo;
			guiscrollBar.Initialize();
		}
		this.title_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 18f, 74f, 2f, 2f, 82f, 24f }, fiaTexture2, 2, GUIFontCalculator.DEFAULT_GAP);
		this.title_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.title_);
		this.contents_ = new GUIUVScrollImage(0, array, fiaTexture2, 2, guiscrollBar);
		this.contents_.SubMeshIndex = 1;
		this.contents_.InputAuthority = this.inputAuthority_;
		this.contents_.RegistPanelManager(this.panelManager_);
		this.cost_ = new GUIString(new Vector2(250f, 76f), 2, 10, GUIString.Alignment.RIGHT, 1, fiaTexture3);
		this.cost_.SubMeshIndex = 2;
		this.panelManager_.RegistGUIInterface(this.cost_);
		this.cost_.SetColor(FiaColor.black);
		this.cost_.SetString(string.Empty);
		this.costLabel_ = new GUIString(new Vector2(98f, 76f), 2, 10, GUIString.Alignment.LEFT, 1, fiaTexture3);
		this.costLabel_.SubMeshIndex = 2;
		this.panelManager_.RegistGUIInterface(this.costLabel_);
		this.costLabel_.SetColor(FiaColor.black);
		this.costLabel_.SetString("Buy");
		this.costBG_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 94f, 74f, 98f, 220f, 256f, 238f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.costBG_);
		this.cancel_ = new GUIButton(0, new float[]
		{
			24f, 256f, 138f, 286f, 98f, 2f, 145f, 98f, 52f, 213f,
			68f
		}, fiaTexture, 2, new Vector3(49f, 0f, 1f));
		this.cancel_.SetTouchRegionByWindowPos(10f, 250f, 142f, 300f);
		this.panelManager_.RegistGUIInterface(this.cancel_);
		this.mouseManager_.Insert(1, this.cancel_);
		this.yep_ = new GUIButton(0, new float[]
		{
			144f, 256f, 258f, 286f, 98f, 2f, 145f, 98f, 70f, 213f,
			86f
		}, fiaTexture, 2, new Vector3(49f, 0f, 1f));
		this.yep_.SetTouchRegionByWindowPos(142f, 250f, 274f, 300f);
		this.panelManager_.RegistGUIInterface(this.yep_);
		this.mouseManager_.Insert(0, this.yep_);
		base.gameObject.SetActiveRecursively(false);
		guiscrollBar.gameObject.SetActiveRecursively(false);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		if (this.contents_ != null && this.contents_.Visible)
		{
			this.contents_.Update();
		}
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
			int num = this.mouseManager_.GetSelectedKey();
			if (this.backButtonSelected)
			{
				this.backButtonSelected = false;
				num = 1;
			}
			if (num == 0)
			{
				StageController.Instance.InputAutority = 0;
			}
			else if (num == 1)
			{
				StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
				base.gameObject.SetActiveRecursively(false);
			}
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
	}

	private void PurchaseSuccess()
	{
		StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
		base.gameObject.SetActiveRecursively(false);
		TrackAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_SHOPLIST);
		monoBehaviourMessage1Param.Initialize(-1);
		base.SendMessage(1025, monoBehaviourMessage1Param);
		base.SendMessage(1048, monoBehaviourMessage1Param);
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
	}

	private void PurchaseFailure(Exception ex)
	{
		StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
		base.gameObject.SetActiveRecursively(false);
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_INFO && !base.gameObject.active)
		{
			MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)msg;
			AssetType lparam_ = monoBehaviourMessage2Param.lparam_;
			int rparam_ = monoBehaviourMessage2Param.rparam_;
			int num = -1;
			AssetDefinitionManager assetDefinitionManager = null;
			if (monoBehaviourMessage2Param != null)
			{
				if (lparam_ == AssetType.TRACK)
				{
					assetDefinitionManager = TrackAssetDefinitionManager.Instance;
				}
				else if (lparam_ == AssetType.KART)
				{
					assetDefinitionManager = KartAssetDefinitionManager.Instance;
				}
				else if (lparam_ == AssetType.CHARACTER)
				{
					assetDefinitionManager = CharacterAssetDefinitionManager.Instance;
				}
				if (assetDefinitionManager != null)
				{
					AssetDefinition assetDefinition = assetDefinitionManager.GetAssetDefinition(rparam_);
					if (assetDefinition != null)
					{
						this.packageId_ = assetDefinition.ProductIDString;
						num = assetDefinition.ProductID;
					}
				}
				if (num >= 0)
				{
					this.meshRenderer_.materials[1].mainTexture = this.textures_[num];
					string text = this.store_.ProductInfoList[num].PriceString;
					ArrayList currencyCharacterList = FiaUtil.CurrencyCharacterList;
					if (currencyCharacterList != null)
					{
						foreach (object obj in currencyCharacterList)
						{
							ArrayList arrayList = (ArrayList)obj;
							text = text.Replace((string)arrayList[0], (string)arrayList[1]);
						}
					}
					this.cost_.SetString(text);
				}
				this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
				StageController.Instance.InputAutority = this.inputAuthority_;
				if (this.panelManager_.Update())
				{
					this.panelManager_.UpdateMesh(ref this.mesh_);
				}
				base.gameObject.SetActiveRecursively(true);
			}
		}
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	private const int numOfSets = 6;

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx title_;

	private GUIString cost_;

	private GUIString costLabel_;

	private GUIPanelEx costBG_;

	private GUIButton yep_;

	private GUIButton cancel_;

	private GUIUVScrollImage contents_;

	private bool backButtonSelected;

	private MouseManager mouseManager_;

	public GUIScrollBar scrollBarPrefab_;

	public Texture2D[] textures_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;

	protected string packageId_ = string.Empty;

	private FiaStore store_;
}
