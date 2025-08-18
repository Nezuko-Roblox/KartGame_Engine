using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIStoreList : GUIListCtrl
{
	public GUIStoreList()
	{
		this.validItemCount_ = 9;
		this.items_ = new GUIStoreListItem[9];
		this.listIdxToProductId_ = new string[9];
	}

	public override void DoInit()
	{
		this.store_ = FiaStore.Inst;
		this.RegistMonoBehaviour(1047);
		this.DISTANCE = this.DISTANCE * (float)Screen.height / 480f;
		this.START_Y = this.START_Y * (float)Screen.height / 480f;
		this.START_X = this.START_X * (float)Screen.width / 800f;
		base.DoInit();
		GUIKartViewer.Instance.Hide();
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.meshRenderer_.materials[0];
		scrollBarInfo.texture_ = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		ScrollBarInfo scrollBarInfo2 = scrollBarInfo;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 188f, 2f, 23f };
		array[0] = 229f * (float)Screen.width / 800f;
		array[1] = 98f * (float)Screen.height / 480f;
		array[2] = 236f * (float)Screen.width / 800f;
		array[3] = 454f * (float)Screen.height / 480f;
		scrollBarInfo2.scrollbarInfo_ = array;
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, 0f, num2 - num);
		return scrollBarInfo;
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		maxRange = (88f + (float)this.validItemCount_ * 118f) * (float)Screen.height / 480f - (float)Screen.height;
		minRange = 0f;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(12f * (float)Screen.width / 800f, 78f * (float)Screen.height / 480f, 226f * (float)Screen.width / 800f, 584f * (float)Screen.height / 480f);
	}

	protected override void InitializeListctrl()
	{
		this.items_[0] = new GUIStoreListRestoreItem(0, this.meshRenderer_, new Vector2(12f * (float)Screen.width / 800f, 78f * (float)Screen.height / 480f), this);
		foreach (Product product in this.store_.ProductInfoList)
		{
			int num = product.Index + 1;
			if (product.Type == ProductType.BUNDLE_SET)
			{
				GUIStoreListUnlockAllItem guistoreListUnlockAllItem = new GUIStoreListUnlockAllItem(num, this.meshRenderer_, new Vector2(12f * (float)Screen.width / 800f, (78f + (float)num * 118f) * (float)Screen.height / 480f), this);
				this.items_[num] = guistoreListUnlockAllItem;
			}
			else
			{
				GUIStoreListUnlockOneItem guistoreListUnlockOneItem = new GUIStoreListUnlockOneItem(num, this.meshRenderer_, new Vector2(12f * (float)Screen.width / 800f, (78f + (float)num * 118f) * (float)Screen.height / 480f), this);
				this.items_[num] = guistoreListUnlockOneItem;
			}
			this.listIdxToProductId_[num] = product.ID;
		}
		base.SelectedId = 1;
		this.UpdateStoreItemInfo();
		this.RefreshItemList();
		for (int i = 0; i < 9; i++)
		{
			this.items_[i].RegistPanelManager(this.panelManager_);
			this.items_[i].RegistMouseManager(this.mouseManager_);
		}
	}

	protected void RefreshItemList()
	{
		foreach (Product product in this.store_.ProductInfoList)
		{
			int num = product.Index + 1;
			if (product.Type == ProductType.BUNDLE_SET)
			{
				GUIStoreListUnlockAllItem guistoreListUnlockAllItem = (GUIStoreListUnlockAllItem)this.items_[num];
				bool flag = true;
				foreach (string text in product.Unlocks)
				{
					if (!this.store_.UnlockedProductList.Contains(text))
					{
						flag = false;
						break;
					}
				}
				guistoreListUnlockAllItem.SetData(product.IconIndex, flag, AlertStateFactory.Instance.GetAlertState(AlertStateType.BUNDLE).DisplayAlert(product.ID));
			}
			else
			{
				((GUIStoreListUnlockOneItem)this.items_[num]).SetData(product.IconIndex, this.store_.UnlockedProductList.Contains(product.ID), AlertStateFactory.Instance.GetAlertState(AlertStateType.BUNDLE).DisplayAlert(product.ID));
			}
		}
	}

	protected void UpdateStoreItemInfo()
	{
		MonoBehaviourMessage1Param<string> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<string>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO);
		base.SendMessage(1046, monoBehaviourMessage1Param.Initialize(this.listIdxToProductId_[base.SelectedId]));
		base.SendMessage(1045, monoBehaviourMessage1Param.Initialize(this.listIdxToProductId_[base.SelectedId]));
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		for (int i = 0; i < 9; i++)
		{
			this.items_[i].Update();
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (base.gameObject.active)
		{
			if (msg.type_ == MonoBehaviourMessageType.ITEM_TO_CTRL)
			{
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
				if (monoBehaviourMessage2Param.lparam_ == 1)
				{
					if (monoBehaviourMessage2Param.rparam_ == 0)
					{
						using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
						{
							androidJavaClass.CallStatic<int>("requestPurchasedHistory", new object[0]);
							return;
						}
					}
					base.SelectedId = monoBehaviourMessage2Param.rparam_;
					AlertStateFactory.Instance.GetAlertState(AlertStateType.BUNDLE).Click(this.listIdxToProductId_[base.SelectedId]);
					this.UpdateStoreItemInfo();
					this.RefreshItemList();
					return;
				}
			}
			else if (msg.type_ == MonoBehaviourMessageType.UPDATE_SHOPLIST)
			{
				this.RefreshItemList();
			}
		}
	}

	public void purchasedList(string msg)
	{
		foreach (string text in msg.Split(new char[] { ',' }))
		{
			this.prevInputAuthority_ = StageController.Instance.InputAutority;
			StageController.Instance.InputAutority = 0;
			if (!Env.IsDesktop)
			{
				iOSEvent.ShowIndicator();
			}
			FiaCoroutine fiaCoroutine = new FiaCoroutine(this.store_.Purchase(text), new OnSuccess(this.RestoreSuccess), new OnFailure(this.RestoreFailure));
			base.StartCoroutine(fiaCoroutine);
		}
	}

	private void RestoreItems()
	{
		this.prevInputAuthority_ = StageController.Instance.InputAutority;
		StageController.Instance.InputAutority = 0;
		if (!Env.IsDesktop)
		{
			iOSEvent.ShowIndicator();
		}
		FiaCoroutine fiaCoroutine = new FiaCoroutine(this.store_.RestorePurchases(), new OnSuccess(this.RestoreSuccess), new OnFailure(this.RestoreFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void RestoreSuccess()
	{
		iOSEvent.Alert("이전 구매 목록을 복원하였습니다.");
		TrackAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		this.RefreshItemList();
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.prevInputAuthority_;
	}

	private void RestoreFailure(Exception ex)
	{
		if (!Env.IsDesktop)
		{
			iOSEvent.HideIndicator();
		}
		StageController.Instance.InputAutority = this.prevInputAuthority_;
	}

	private float START_X = 12f;

	private float START_Y = 78f;

	private float DISTANCE = 118f;

	private const int ITEM_COUNT = 9;

	private int validItemCount_;

	private GUIStoreListItem[] items_;

	private string[] listIdxToProductId_;

	private FiaStore store_;

	private byte prevInputAuthority_;

	protected class sortByIndex : Comparer<Product>
	{
		public override int Compare(Product x, Product y)
		{
			return x.Index.CompareTo(y.Index);
		}
	}
}
