using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIAccomplishPopup : FiaGUILayer
{
	protected override void CameraSetting()
	{
		if (KartManager.Instance.parameter_.Stage == StageType.GAME || KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI)
		{
			this.cam_ = CameraManager.Instance.guiCam_;
			for (int i = 0; i < 32; i++)
			{
				if (((this.cam_.cullingMask >> i) & 1) != 0)
				{
					base.gameObject.layer = i;
					break;
				}
			}
		}
		else
		{
			base.CameraSetting();
		}
	}

	public override void DoInit()
	{
		base.DoInit();
		this.mouseManager_ = new MouseManager(this.inputAuthority_);
		this.RegistMonoBehaviour(265);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 47f, 90f, 2f, 209f, 708f, 510f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.back_.RegistPanelManager(this.panelManager_);
		this.title_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 64f, 73f, 2f, 2f, 294f, 52f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.title_.RegistPanelManager(this.panelManager_);
		this.icon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 70f, 182f, 2f, 2f, 128f, 106f }, fiaTexture2, 3, GUIFontCalculator.DEFAULT_GAP);
		this.icon_.SubMeshIndex = 1;
		this.icon_.RegistPanelManager(this.panelManager_);
		this.name_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 350f, 132f, 136f, 8f, 424f, 46f }, fiaTexture2, 3, GUIFontCalculator.Y2_GAP);
		this.name_.SubMeshIndex = 1;
		this.name_.RegistPanelManager(this.panelManager_);
		this.description_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 232f, 194f, 18f, 110f, 424f, 270f }, fiaTexture2, 3, GUIFontCalculator.Y2_GAP);
		this.description_.SubMeshIndex = 1;
		this.description_.RegistPanelManager(this.panelManager_);
		this.yep_ = new GUIButton(0, new float[]
		{
			292f, 320f, 508f, 367f, 296f, 2f, 371f, 450f, 2f, 666f,
			29f
		}, fiaTexture, 2, new Vector3(77f, 0f, 1f));
		this.panelManager_.RegistGUIInterface(this.yep_);
		this.mouseManager_.Insert(0, this.yep_);
		base.gameObject.SetActiveRecursively(false);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_INFO)
		{
			MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				if (monoBehaviourMessage2Param.lparam_ == AssetType.SIZE && monoBehaviourMessage2Param.rparam_ < 0)
				{
					this.textureList_.Clear();
					for (int i = 0; i < this.textures_.Length; i++)
					{
						KartOptions.QuestFlag questFlag = KartOptions.Instance.MapTextureIndexToQuestFlag(i);
						if (this.assetType_[i] != 3 && !KartOptions.Instance.IsQuestFlagOn(questFlag))
						{
							AssetDefinition assetDefinition = AssetDefinitionManager.GetAssetDefinition((AssetType)this.assetType_[i], this.assetId_[i]);
							if (assetDefinition != null && assetDefinition.LockType == AssetDefinition.enLockType.QUEST && !assetDefinition.Lock)
							{
								this.textureList_.Add(i);
							}
						}
					}
					if (this.textureList_.Count > 0)
					{
						if (Debug.isDebugBuild)
						{
							string text = "AccomplishPopup TextureList\n";
							foreach (int num in this.textureList_)
							{
								text = text + num.ToString() + "\n";
							}
						}
						this.TextureSetting(this.textureList_[0]);
						this.textureList_.RemoveAt(0);
						this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
						StageController.Instance.InputAutority = this.inputAuthority_;
						if (this.panelManager_.Update())
						{
							this.panelManager_.UpdateMesh(ref this.mesh_);
						}
						base.gameObject.SetActiveRecursively(true);
					}
				}
				else
				{
					int num2 = -1;
					if (monoBehaviourMessage2Param.lparam_ == AssetType.SIZE && monoBehaviourMessage2Param.rparam_ >= 0)
					{
						num2 = this.textures_.Length + monoBehaviourMessage2Param.rparam_;
					}
					else
					{
						for (int j = 0; j < this.textures_.Length; j++)
						{
							KartOptions.QuestFlag questFlag2 = KartOptions.Instance.MapTextureIndexToQuestFlag(j);
							if (this.assetType_[j] == (int)monoBehaviourMessage2Param.lparam_ && this.assetId_[j] == monoBehaviourMessage2Param.rparam_ && !KartOptions.Instance.IsQuestFlagOn(questFlag2))
							{
								num2 = j;
								break;
							}
						}
					}
					if (num2 >= 0 && this.textureList_.IndexOf(num2) < 0)
					{
						if (!base.gameObject.active)
						{
							this.TextureSetting(num2);
							this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
							StageController.Instance.InputAutority = this.inputAuthority_;
							if (this.panelManager_.Update())
							{
								this.panelManager_.UpdateMesh(ref this.mesh_);
							}
							base.gameObject.SetActiveRecursively(true);
						}
						else
						{
							this.textureList_.Add(num2);
						}
					}
				}
			}
		}
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
		if (this.mouseManager_.IsSelected())
		{
			if (this.mouseManager_.GetSelectedKey() == 0)
			{
				this.DisablePopup();
			}
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
	}

	private void TextureSetting(int textureIdx)
	{
		int num = 0;
		int num2;
		if (textureIdx >= this.textures_.Length)
		{
			num2 = this.textures_.Length - 1;
			num = textureIdx - this.textures_.Length;
		}
		else
		{
			num2 = textureIdx;
			KartOptions.QuestFlag questFlag = KartOptions.Instance.MapTextureIndexToQuestFlag(textureIdx);
			KartOptions.Instance.SetQuestFlag(questFlag, true);
			KartOptions.Instance.SaveRegistry();
		}
		this.meshRenderer_.materials[1].mainTexture = this.textures_[num2];
		this.name_.UV = num;
	}

	private void DisablePopup()
	{
		if (this.textureList_.Count <= 0)
		{
			StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
			base.gameObject.SetActiveRecursively(false);
			if (KartManager.Instance.parameter_.Stage == StageType.GAME)
			{
				MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(true, KartManager.Instance.result_.IsBestRecord));
			}
		}
		else
		{
			this.TextureSetting(this.textureList_[0]);
			this.textureList_.RemoveAt(0);
		}
	}

	public static bool OpenStaticPopup()
	{
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		TrackAssetDefinitionManager.Instance.Refresh();
		MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
		MonoBehaviourExCenter.Instance.SendMessage(0, 265, monoBehaviourMessage2Param.Initialize(AssetType.SIZE, -1));
		return true;
	}

	public void BackButtonAction(string msg)
	{
	}

	private const float POPUP_VISIBLE_TIME = 2f;

	public Texture2D[] textures_;

	public int[] assetType_;

	public int[] assetId_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;

	private MouseManager mouseManager_;

	private GUIPanelEx back_;

	private GUIPanelEx title_;

	private GUIPanelEx icon_;

	private GUIPanelEx name_;

	private GUIPanelEx description_;

	private GUIButton yep_;

	private List<int> textureList_ = new List<int>();
}
