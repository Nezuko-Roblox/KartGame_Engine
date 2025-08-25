using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIQuestPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(1050);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 78f, 458f, 474f, 2f, 2f, 93f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.back_.RegistPanelManager(this.panelManager_);
		float[] array = new float[] { 24f, 90f, 446f, 358f, 2f, 2f, 424f, 270f };
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.isVert_ = true;
		scrollBarInfo.viewRegion_ = GUIBase.ConvertWSToUS(array, 0);
		scrollBarInfo.listRegion_ = GUIBase.ConvertWSToUS(array, 4);
		scrollBarInfo.material_ = this.meshRenderer_.material;
		scrollBarInfo.texture_ = fiaTexture;
		scrollBarInfo.scrollbarInfo_ = new float[] { 266f, 142f, 270f, 240f, 183f, 2f, 15f };
		GUIScrollBar guiscrollBar = null;
		if (this.scrollBarPrefab_ != null && scrollBarInfo != null)
		{
			guiscrollBar = (GUIScrollBar)global::UnityEngine.Object.Instantiate(this.scrollBarPrefab_);
			guiscrollBar.transform.parent = base.transform.parent;
			guiscrollBar.info_ = scrollBarInfo;
			guiscrollBar.Initialize();
		}
		this.contents_ = new GUIUVScrollImage(0, array, fiaTexture3, 2, guiscrollBar);
		this.contents_.InputAuthority = this.inputAuthority_;
		this.contents_.SubMeshIndex = 1;
		this.contents_.RegistPanelManager(this.panelManager_);
		this.contents_.SetUV(0);
		this.progressBack_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 158f, 162f, 95f, 141f, 383f, 169f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.progressBack_);
		this.progress_ = new GUIString(new Vector2(436f, 165f), 1, 8, GUIString.Alignment.RIGHT, 1, fiaTexture2);
		this.progress_.SubMeshIndex = 2;
		this.progress_.SetColor(Color.black);
		this.panelManager_.RegistGUIInterface(this.progress_);
		this.yep_ = new GUIButton(0, new float[]
		{
			127f, 416f, 343f, 466f, 95f, 2f, 170f, 95f, 54f, 311f,
			81f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.yep_);
		this.mouseManager_.Insert(0, this.yep_);
		base.gameObject.SetActiveRecursively(false);
		guiscrollBar.gameObject.SetActiveRecursively(false);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.contents_.Update();
		this.mouseManager_.Update();
		this.yep_.SetPushed(this.mouseManager_.GetPushedKey() == 0);
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected())
		{
			StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
			base.gameObject.SetActiveRecursively(false);
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_INFO && !base.gameObject.active)
		{
			MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				int rparam_ = monoBehaviourMessage2Param.rparam_;
				AssetDefinitionManager assetDefinitionManager;
				if (monoBehaviourMessage2Param.lparam_ == AssetType.CHARACTER)
				{
					assetDefinitionManager = CharacterAssetDefinitionManager.Instance;
				}
				else if (monoBehaviourMessage2Param.lparam_ == AssetType.KART)
				{
					assetDefinitionManager = KartAssetDefinitionManager.Instance;
				}
				else
				{
					assetDefinitionManager = TrackAssetDefinitionManager.Instance;
				}
				AssetDefinition assetDefinition = assetDefinitionManager.GetAssetDefinition(rparam_);
				if (assetDefinition.Quest != null)
				{
					string text = string.Format("{0}/{1}", assetDefinition.Quest.GetCurrent(), assetDefinition.Quest.GetGoal());
					this.progress_.SetString(text);
					for (int i = 0; i < this.textures_.Length; i++)
					{
						if (this.assetType_[i] == (int)monoBehaviourMessage2Param.lparam_ && this.assetId_[i] == monoBehaviourMessage2Param.rparam_)
						{
							this.meshRenderer_.materials[1].mainTexture = this.textures_[i];
							break;
						}
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
	}

	public void BackButtonAction(string msg)
	{
	}

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx title_;

	private GUIButton yep_;

	private GUIUVScrollImage contents_;

	private GUIPanelEx progressBack_;

	private GUIString progress_;

	private GUIPanelEx itemIcon_;

	private MouseManager mouseManager_;

	public GUIScrollBar scrollBarPrefab_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;

	public Material subMaterial_;

	public Texture2D[] textures_;

	public int[] assetType_;

	public int[] assetId_;
}
