using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIShopList : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 1;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(12f * (float)Screen.width / 800f, 78f * (float)Screen.height / 480f, 458f * (float)Screen.width / 800f, 474f * (float)Screen.height / 480f);
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		maxRange = (78f + (float)this.validItemCount_ * 120f) * (float)Screen.height / 480f - (float)Screen.height;
		minRange = 0f;
	}

	protected override void InitializeListctrl()
	{
		for (int i = 0; i < 15; i++)
		{
			this.items_[i] = new GUIShopListItem(i, this.meshRenderer_, new Vector2(12f * (float)Screen.width / 800f, (78f + (float)i * 120f) * (float)Screen.height / 480f), this);
			this.items_[i].RegistPanelManager(this.panelManager_);
			this.items_[i].RegistMouseManager(this.mouseManager_);
		}
		this.validItemCount_ = 0;
	}

	private void UpdateData(bool isRecal)
	{
		if (this.garageMode_ == AssetType.CHARACTER)
		{
			this.UpdateCharacterData();
		}
		else
		{
			this.UpdateKartData();
		}
		if (isRecal)
		{
			base.Recalculate();
			base.ResetLocalPosition(this.items_[base.SelectedId].Offset.y - 78f);
		}
	}

	private void UpdateCharacterData()
	{
		base.SelectedId = 1;
		List<AssetDefinition> assetDefinitionList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
		int num = 0;
		for (int i = 0; i < assetDefinitionList.Count; i++)
		{
			CharacterAssetDefinition characterAssetDefinition = (CharacterAssetDefinition)CharacterAssetDefinitionManager.Instance.GetAssetDefinition(i);
			if (characterAssetDefinition.Id == KartOptions.Instance.Character)
			{
				base.SelectedId = num;
			}
			GUIShopListItem.enItemLockType enItemLockType = GUIShopListItem.enItemLockType.NONE;
			string text = string.Empty;
			if (enItemLockType == GUIShopListItem.enItemLockType.CASH && characterAssetDefinition.ProductIDs.Length != 0)
			{
				text = FiaStore.Inst.FindProductWithID(characterAssetDefinition.ProductIDString).LocalizedTitle;
				text = text.Replace("Bundle ", string.Empty);
			}
			bool flag = this.characterState_.DisplayAlert(characterAssetDefinition.Id);
			this.items_[num].SetData(i, assetDefinitionList[i].Name, enItemLockType, text, flag);
			num++;
		}
		this.validItemCount_ = num;
		bool flag2 = KartManager.Instance.parameter_.GaragePrevStage == StageType.WAITROOM_HOST || KartManager.Instance.parameter_.GaragePrevStage == StageType.WAITROOM_HOST;
		if (this.validItemCount_ != assetDefinitionList.Count && !flag2)
		{
			this.items_[num].SetData(GUIMoreItemType.CHARACTERS);
			num++;
			this.validItemCount_++;
		}
	}

	private void UpdateKartData()
	{
		base.SelectedId = 1;
		List<AssetDefinition> assetDefinitionList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
		int num = 0;
		for (int i = 0; i < assetDefinitionList.Count; i++)
		{
			KartAssetDefinition kartAssetDefinition = (KartAssetDefinition)assetDefinitionList[i];
			GUIShopListItem.enItemLockType enItemLockType = GUIShopListItem.enItemLockType.NONE;
			if (kartAssetDefinition.Id == KartOptions.Instance.Kart)
			{
				base.SelectedId = num;
			}
			float[] array = new float[] { kartAssetDefinition.Speed, kartAssetDefinition.Acceleration, kartAssetDefinition.Handling };
			string text = string.Empty;
			if (enItemLockType == GUIShopListItem.enItemLockType.CASH && kartAssetDefinition.ProductIDs.Length != 0)
			{
				text = FiaStore.Inst.FindProductWithID(kartAssetDefinition.ProductIDString).LocalizedTitle;
				text = text.Replace("Bundle ", string.Empty);
			}
			bool flag = this.kartState_.DisplayAlert(kartAssetDefinition.Id);
			this.items_[num].SetData(i, kartAssetDefinition.Name, array, enItemLockType, text, flag);
			num++;
		}
		this.validItemCount_ = num;
		bool flag2 = KartManager.Instance.parameter_.GaragePrevStage == StageType.WAITROOM_HOST || KartManager.Instance.parameter_.GaragePrevStage == StageType.WAITROOM_HOST;
		if (this.validItemCount_ != assetDefinitionList.Count && !flag2)
		{
			this.items_[num].SetData(GUIMoreItemType.KARTS);
			num++;
			this.validItemCount_++;
		}
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.meshRenderer_.materials[0];
		scrollBarInfo.texture_ = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		ScrollBarInfo scrollBarInfo2 = scrollBarInfo;
		float[] array = new float[] { 461f, 0f, 468f, 0f, 560f, 95f, 116f };
		array[1] = 98f * (float)Screen.height / 480f;
		array[3] = 454f * (float)Screen.height / 480f;
		scrollBarInfo2.scrollbarInfo_ = array;
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, 0f, num2 - num);
		return scrollBarInfo;
	}

	public override void DoInit()
	{
		this.RegistMonoBehaviour(1048);
		CharacterAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		base.DoInit();
		this.kartState_ = AlertStateFactory.Instance.GetAlertState(AlertStateType.KART);
		this.characterState_ = AlertStateFactory.Instance.GetAlertState(AlertStateType.CHARACTER);
		this.kartState_.Refresh();
		this.characterState_.Refresh();
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		for (int i = 0; i < 15; i++)
		{
			this.items_[i].Update();
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (base.gameObject.active)
		{
			if (msg.type_ == MonoBehaviourMessageType.UPDATE_SHOPLIST)
			{
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)msg;
				if (monoBehaviourMessage1Param != null)
				{
					if (monoBehaviourMessage1Param.param_ == 0 || monoBehaviourMessage1Param.param_ == 1)
					{
						this.garageMode_ = (AssetType)monoBehaviourMessage1Param.param_;
						this.UpdateData(true);
					}
					else
					{
						this.UpdateData(false);
					}
				}
			}
			else if (msg.type_ == MonoBehaviourMessageType.ITEM_TO_CTRL)
			{
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
				if (monoBehaviourMessage2Param.lparam_ == 1)
				{
					int itemId = this.items_[monoBehaviourMessage2Param.rparam_].ItemId;
					if (this.garageMode_ == AssetType.CHARACTER)
					{
						if (this.characterState_.DisplayAlert(itemId))
						{
							this.characterState_.Click(itemId);
							this.UpdateCharacterData();
						}
					}
					else if (this.kartState_.DisplayAlert(itemId))
					{
						this.kartState_.Click(itemId);
						this.UpdateKartData();
					}
					switch (this.items_[monoBehaviourMessage2Param.rparam_].ItemLockType)
					{
					case GUIShopListItem.enItemLockType.NONE:
						base.SelectedId = monoBehaviourMessage2Param.rparam_;
						if (this.garageMode_ == AssetType.CHARACTER)
						{
							KartOptions.Instance.Character = this.items_[monoBehaviourMessage2Param.rparam_].ItemId;
						}
						else
						{
							KartOptions.Instance.Kart = this.items_[monoBehaviourMessage2Param.rparam_].ItemId;
						}
						GUIKartViewer.Instance.ChangeKartCharacter((byte)KartOptions.Instance.Kart, (byte)KartOptions.Instance.Character);
						break;
					case GUIShopListItem.enItemLockType.CASH:
					{
						MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
						base.SendMessage(1051, monoBehaviourMessage2Param2.Initialize((this.garageMode_ != AssetType.CHARACTER) ? AssetType.KART : AssetType.CHARACTER, itemId));
						break;
					}
					case GUIShopListItem.enItemLockType.QUEST:
					{
						AssetDefinition assetDefinition;
						if (this.garageMode_ == AssetType.CHARACTER)
						{
							assetDefinition = CharacterAssetDefinitionManager.Instance.GetAssetDefinition(itemId);
						}
						else
						{
							assetDefinition = KartAssetDefinitionManager.Instance.GetAssetDefinition(itemId);
						}
						if (assetDefinition == null)
						{
							Debug.LogError("definition is null ");
						}
						else if (assetDefinition.Quest == null)
						{
							Debug.LogError("definition quest is null");
						}
						else
						{
							MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param3 = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
							base.SendMessage(1050, monoBehaviourMessage2Param3.Initialize((this.garageMode_ != AssetType.CHARACTER) ? AssetType.KART : AssetType.CHARACTER, itemId));
						}
						break;
					}
					}
				}
				else if (monoBehaviourMessage2Param.lparam_ == -1)
				{
					StageController.Instance.ChangeStage(StageType.STORE);
				}
			}
		}
	}

	private const int ITEM_COUNT = 15;

	private const float START_X = 12f;

	private const float START_Y = 78f;

	private const float DISTANCE = 120f;

	private int validItemCount_ = 15;

	private GUIShopListItem[] items_ = new GUIShopListItem[15];

	private AssetType garageMode_;

	private AlertState kartState_;

	private AlertState characterState_;
}
