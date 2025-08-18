using System;
using System.IO;
using UnityEngine;

public class LoadingStage : MonoBehaviourStage
{
	private void OnImageUpdateSuccessWith(string fbid_)
	{
		string text = FiaUtil.docPath;
		text = Path.Combine(text, "fbImage");
		string text2 = Path.Combine(text, "_" + fbid_ + ".png");
		text = Path.Combine(text, fbid_ + ".png");
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		File.Move(text2, text);
	}

	private void OnImageUpdateFailure(Exception ex)
	{
	}

	protected override void Start()
	{
		StageController.Instance.SetBgm(StageType.MAIN_LOADING);
		Time.timeScale = 1f;
		base.Start();
		this.RegistMonoBehaviour(512);
		iPhoneSettings.screenCanDarken = false;
		TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
		KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
		CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
		MaterialManager.Instance.Initialize();
		KartOptions.Instance.LoadRegistry();
		MonoBehaviourMessageFactory.Instance.Initialize();
		Statistics.Instance.Initialize();
		if (KartOptions.Instance.Controller == -1)
		{
			KartOptions.Instance.Controller = ((!Env.IsIPad) ? 2 : 3);
		}
		iOSController.Instance.Type = (iOSControllerType)KartOptions.Instance.Controller;
		MockFiaStore.Inst.LoadPurchasedProductList();
		base.StartCoroutine(MockFiaStore.Inst.RequestProductInfo());
		if (MockFiaStore.Inst.ProductInfoList == null)
		{
			MockFiaStore.Inst.LoadDefaultProductInfo();
		}
		CharacterAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		TrackAssetDefinitionManager.Instance.Refresh();
		GUIKartViewer.Instance.ChangeKartCharacter((byte)KartOptions.instance_.Kart, (byte)KartOptions.instance_.Character);
		GUIKartViewer.Instance.Hide();
		this.simpleMessage_ = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		if (Env.IsDesktop)
		{
			this.fb_ = MockFacebook.Inst;
			if (!this.fb_.LoggedIn)
			{
				this.updating_ |= 2;
				FiaCoroutine fiaCoroutine = new FiaCoroutine(this.fb_.Login(), new OnSuccess(this.LoginSuccess), new OnFailure(this.LoginFailure));
				base.StartCoroutine(fiaCoroutine);
			}
			else
			{
				this.UpdateFriendDictAndRanking();
			}
		}
		else
		{
			this.fb_ = Facebook.Inst;
			if (this.fb_.LoggedIn)
			{
				if (Debug.isDebugBuild)
				{
					Debug.Log(this.fb_.ToString());
				}
				this.UpdateRanking();
			}
			else if (this.fb_.FBID != null && FiaAuth.AuthToken != null)
			{
				this.fb_.LoadFriendXML();
				if (Env.IsConnectedToInternet)
				{
					this.UpdateFriendDictAndRanking();
				}
			}
		}
		PlayerPrefs.DeleteKey("SHOWED_TURNOFF_WIFI_MESSAGE");
	}

	private void Update()
	{
		if (!this.isSendMessage_ && this.updating_ == 0)
		{
			base.SendMessage(1024, this.simpleMessage_.Initialize(0, 0));
			this.isSendMessage_ = true;
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			base.ReadyToChangeScene();
		}
	}

	private void UpdateFriendDictAndRanking()
	{
		this.updating_ |= 1;
		FiaCoroutine fiaCoroutine = new FiaCoroutine(Facebook.Inst.FetchFriends(), new OnSuccess(this.UpdateRanking), new OnFailure(this.RankingUpdateFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void UpdateRanking()
	{
		this.updating_ |= 1;
		KartOptions.Instance.UpdateQuestFlag();
		KartOptions.Instance.SaveRegistry();
		RankingUpdater rankingUpdater = new RankingUpdater(this.fb_.FBID, this.fb_.FriendDict, FiaAuth.AuthToken);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(rankingUpdater.Update(), new OnSuccess(this.RankingUpdateSuccess), new OnFailure(this.RankingUpdateFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void RankingUpdateSuccess()
	{
		this.updating_ &= -2;
	}

	private void RankingUpdateFailure(Exception ex)
	{
		this.updating_ &= -2;
		if (ex.GetType() == typeof(FiaAuthException))
		{
			base.StartCoroutine(new FiaCoroutine(this.fb_.Logout(), new OnSuccess(this.LogoutSuccess), new OnFailure(this.LogoutFailure)));
		}
	}

	private void LoginSuccess()
	{
		this.updating_ &= -3;
		this.UpdateRanking();
	}

	private void LoginFailure(Exception ex)
	{
		this.updating_ &= -3;
	}

	private void LogoutSuccess()
	{
	}

	private void LogoutFailure(Exception ex)
	{
	}

	private void RequestProductInfoSuccess()
	{
		foreach (Product product in FiaStore.Inst.ProductInfoList)
		{
		}
	}

	private void RequestProductInfoFailure(Exception ex)
	{
	}

	private MonoBehaviourMessage2Param<int, int> simpleMessage_;

	private bool isSendMessage_;

	private Facebook fb_;

	private int updating_;

	private enum UpdatingFlag
	{
		RANKING = 1,
		LOG_IN
	}
}
