using System;
using UnityEngine;

public class GUIRankingHeaderInSingle : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(1026);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		this.background_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 70f, 458f, 194f, 2f, 120f, 93f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.background_.RegistPanelManager(this.panelManager_);
		this.trackName_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 153f, 88f, 2f, 2f, 298f, 32f }, fiaTexture3, 3, GUIFontCalculator.Y2_GAP);
		this.trackName_.SubMeshIndex = 2;
		this.trackName_.RegistPanelManager(this.panelManager_);
		this.refresh_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 330f, 138f, 636f, 80f, 758f, 128f }, fiaTexture, 4, GUIFontCalculator.Y2_GAP);
		this.panelManager_.RegistGUIInterface(this.refresh_);
		this.trackIcon_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 18f, 84f, 2f, 108f, 128f, 212f }, fiaTexture2, 4, new Vector3(2f, 2f, 16f));
		this.trackIcon_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.trackIcon_);
		this.typeLabel_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 18f, 194f, 194f, 120f, 628f, 148f }, fiaTexture, 4, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.typeLabel_);
		this.mouseManager_.Insert(0, this.refresh_);
		this.changeEndPos_ = base.transform.localPosition;
		this.changeStartPos_ = this.changeEndPos_ - Vector3.right * 5f;
		base.transform.localPosition = this.changeStartPos_;
		this.changeTimeAccum_ = 0f;
		base.gameObject.SetActiveRecursively(false);
		GUIRankingHeaderInSingle.isMoveToTrack_ = false;
	}

	protected override void BeforePanelUpdate()
	{
		if (GUIRankingHeaderInSingle.isMoveToTrack_ || this.isReturnToRanking_)
		{
			return;
		}
		base.BeforePanelUpdate();
		this.mouseManager_.Update();
		int pushedKey = this.mouseManager_.GetPushedKey();
		this.refresh_.SetUV((pushedKey != 0) ? 0 : 1);
	}

	protected override void AfterPanelUpdate()
	{
		if (GUIRankingHeaderInSingle.isMoveToTrack_ && GUIMode.isRankings_ == IsRankings.YES)
		{
			GUIRankingHeaderInSingle.isMoveToTrack_ = true;
			Vector3 localPosition = base.transform.localPosition;
			this.changeStartPos_ = localPosition;
			this.changeEndPos_ = localPosition - Vector3.right * 5f;
			this.changeTimeAccum_ = 0f;
			GUIMode.isRankings_ = IsRankings.TRANSITION_RANKINGS_OUT;
		}
		if (GUIRankingHeaderInSingle.isMoveToTrack_ || this.isReturnToRanking_)
		{
			return;
		}
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected())
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			int selectedKey = this.mouseManager_.GetSelectedKey();
			if (selectedKey == 1)
			{
				GUIRankingHeaderInSingle.isMoveToTrack_ = true;
				Vector3 localPosition2 = base.transform.localPosition;
				this.changeStartPos_ = localPosition2;
				this.changeEndPos_ = localPosition2 - Vector3.right * 5f;
				this.changeTimeAccum_ = 0f;
			}
			else if (selectedKey == 0)
			{
				if (iPhoneSettings.internetReachability == iPhoneNetworkReachability.NotReachable)
				{
					iOSEvent.Alert("네트워크에 연결할 수 없습니다.");
				}
				else
				{
					Facebook facebook;
					if (Env.IsDesktop)
					{
						facebook = MockFacebook.Inst;
					}
					else
					{
						facebook = Facebook.Inst;
					}
					if (facebook.LoggedIn)
					{
						FiaCoroutine fiaCoroutine = new FiaCoroutine(facebook.FetchFriends(), new OnSuccess(this.FriendDictUpdateSuccess), new OnFailure(this.FriendDictUpdateFailure));
						MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
						MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param.Initialize(0));
						base.StartCoroutine(fiaCoroutine);
					}
					else
					{
						iOSEvent.Alert("페이스북에 로그인 하세요.");
					}
				}
			}
			else if (this.prevRankingType_ != this.rankingType_)
			{
				this.prevRankingType_ = this.rankingType_;
				MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param = ((MonoBehaviourMessage2Param<int, RankingType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RANKING)).Initialize(this.trackIdx_, this.rankingType_);
				base.SendMessage(1027, monoBehaviourMessage2Param);
			}
		}
	}

	private void FriendDictUpdateSuccess()
	{
		Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(new RankingUpdater(facebook.FBID, facebook.FriendDict, FiaAuth.AuthToken).Update(), new OnSuccess(this.RankingUpdateSuccess), new OnFailure(this.RankingUpdateFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void FriendDictUpdateFailure(Exception ex)
	{
		this.CloseLoadingMessage();
		Type type = ex.GetType();
		string text;
		if (type == typeof(RequestException))
		{
			text = "Unable to connect to server. Please try again later.";
		}
		else if (type == typeof(ServerException))
		{
			text = ex.Message;
		}
		else if (type == typeof(FiaAuthException))
		{
			text = "Session verification failed. Please try logging out and back in again.";
		}
		else
		{
			text = "Unable to connect to server. Please try again later....";
		}
		iOSEvent.Alert(text);
	}

	private void RankingUpdateSuccess()
	{
		MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param = ((MonoBehaviourMessage2Param<int, RankingType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RANKING)).Initialize(this.trackIdx_, this.rankingType_);
		base.SendMessage(1027, monoBehaviourMessage2Param);
		this.CloseLoadingMessage();
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 1025, monoBehaviourMessage1Param.Initialize(1));
	}

	private void CloseLoadingMessage()
	{
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param.Initialize(1));
	}

	private void RankingUpdateFailure(Exception ex)
	{
		this.CloseLoadingMessage();
		Type type = ex.GetType();
		string text;
		if (type == typeof(RequestException))
		{
			text = "Unable to connect to server. Please try again later..";
		}
		else if (type == typeof(ServerException))
		{
			text = ex.Message;
		}
		else if (type == typeof(FiaAuthException))
		{
			text = "Session verification failed. Please try logging out and back in again.";
		}
		else
		{
			text = "Unable to connect to server. Please try again later...";
		}
		iOSEvent.Alert(text);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_RANKING && !base.gameObject.active)
		{
			MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, RankingType>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				base.gameObject.SetActiveRecursively(true);
				this.trackIdx_ = monoBehaviourMessage2Param.lparam_;
				RankingType rparam_ = monoBehaviourMessage2Param.rparam_;
				MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param2 = ((MonoBehaviourMessage2Param<int, RankingType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RANKING)).Initialize(this.trackIdx_, rparam_);
				base.SendMessage(1027, monoBehaviourMessage2Param2);
				this.trackName_.SetUV(0);
				if (!TrackAssetDefinitionManager.Instance.IsRandomTrackIndex(this.trackIdx_))
				{
					this.trackName_.SetUV(1 + this.trackIdx_);
				}
				this.trackIcon_.SetUV(this.trackIdx_);
				this.isReturnToRanking_ = true;
				GUIMode.isRankings_ = IsRankings.TRANSITION_RANKINGS_IN;
				this.changeTimeAccum_ = 0f;
				if (this.panelManager_.Update())
				{
					this.panelManager_.UpdateMesh(ref this.mesh_);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (GUIRankingHeaderInSingle.isMoveToTrack_ || this.isReturnToRanking_)
		{
			this.changeTimeAccum_ += Time.deltaTime;
			Vector3 vector = Vector3.Lerp(this.changeStartPos_, this.changeEndPos_, this.changeTimeAccum_ / this.CHANGE_DURATION);
			base.transform.localPosition = vector;
			if (this.changeTimeAccum_ / this.CHANGE_DURATION >= 1f)
			{
				if (GUIRankingHeaderInSingle.isMoveToTrack_)
				{
					base.gameObject.SetActiveRecursively(false);
					Vector3 vector2 = this.changeStartPos_;
					this.changeStartPos_ = this.changeEndPos_;
					this.changeEndPos_ = vector2;
					GUIMode.isRankings_ = IsRankings.TRANSITION_TRACK_IN;
					base.SendMessage(1025, ((MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI)).Initialize(true));
				}
				else
				{
					this.changeStartPos_ = Vector3.zero;
					this.changeEndPos_ = Vector3.zero;
					GUIMode.isRankings_ = IsRankings.YES;
				}
				GUIRankingHeaderInSingle.isMoveToTrack_ = false;
				this.isReturnToRanking_ = false;
				this.changeTimeAccum_ = 0f;
			}
		}
	}

	private GUIPanelEx3PartHorz background_;

	private GUIPanelEx trackName_;

	private GUIPanelEx refresh_;

	private GUIPanelEx trackIcon_;

	private GUIPanelEx typeLabel_;

	private MouseManager mouseManager_ = new MouseManager(false);

	public static bool isMoveToTrack_;

	private bool isReturnToRanking_;

	private float CHANGE_DURATION = 1f;

	private Vector3 changeStartPos_ = Vector3.zero;

	private Vector3 changeEndPos_ = Vector3.zero;

	private float changeTimeAccum_;

	private int trackIdx_ = -1;

	private RankingType rankingType_ = RankingType.THIS_MONTH;

	private RankingType prevRankingType_ = RankingType.THIS_MONTH;

	private enum MouseKey
	{
		REFRESH,
		BACK,
		ALL_TIME,
		THIS_MONTH
	}
}
