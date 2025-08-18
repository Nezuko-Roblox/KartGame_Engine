using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUITrackListInSingle : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 1;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(12f * (float)Screen.width / 800f, 78f * (float)Screen.height / 480f, 458f * (float)Screen.width / 800f, (float)Screen.height);
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		maxRange = (78f + (float)this.trackCount_ * 120f) * (float)Screen.height / 480f - (float)Screen.height;
		minRange = 0f;
	}

	protected override void InitializeListctrl()
	{
		int[] assetIdsByListIndexOrder = TrackAssetDefinitionManager.Instance.GetAssetIdsByListIndexOrder();
		List<AssetDefinition> assetDefinitionList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
		this.visibleAssets_ = new List<AssetDefinition>();
		foreach (int num in assetIdsByListIndexOrder)
		{
			AssetDefinition assetDefinition = assetDefinitionList[num];
			this.visibleAssets_.Add(assetDefinition);
		}
		this.trackCount_ = this.visibleAssets_.Count + 1;
		this.items_ = new GUITrackListItem[this.trackCount_];
		if (this.visibleAssets_.Count != assetDefinitionList.Count)
		{
			this.trackCount_++;
			this.lastItem_ = new GUILastItem(this.items_.Length, this.meshRenderer_, new Vector2(12f, 78f + (float)this.items_.Length * 120f), this);
			this.lastItem_.RegistPanelManager(this.panelManager_);
			this.lastItem_.RegistMouseManager(this.mouseManager_);
		}
		for (int j = 0; j < this.items_.Length; j++)
		{
			this.items_[j] = new GUITrackListItem(j, this.meshRenderer_, new Vector2(12f * (float)Screen.width / 800f, (78f + (float)j * 120f) * (float)Screen.height / 480f), this);
			this.items_[j].RegistPanelManager(this.panelManager_);
			this.items_[j].RegistMouseManager(this.mouseManager_);
		}
		this.InitializeTrackList();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[4].mainTexture);
		GUIPanelEx guipanelEx = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 1f, 1f, 1f, 1f, 1f, 1f }, fiaTexture, 0, GUIFontCalculator.DEFAULT_GAP);
		guipanelEx.SubMeshIndex = 4;
		guipanelEx.Visible = false;
		guipanelEx.RegistPanelManager(this.panelManager_);
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
		TrackAssetDefinitionManager.Instance.Refresh();
		this.trackState_ = AlertStateFactory.Instance.GetAlertState(AlertStateType.TRACK);
		this.trackState_.Refresh();
		this.RegistMonoBehaviour(1025);
		if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
		{
			base.SelectedId = KartOptions.Instance.SelectItemTrack;
		}
		else
		{
			base.SelectedId = KartOptions.Instance.SelectSpeedTrack;
		}
		if (!TrackAssetDefinitionManager.Instance.IsValidIndex(base.SelectedId) && Array.IndexOf<int>(TrackAssetDefinitionManager.RANDOM_IDX_ARRAY, base.SelectedId) == -1)
		{
			base.SelectedId = TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[0];
		}
		this.simpleMessage_ = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		base.DoInit();
		for (int i = 0; i < this.items_.Length; i++)
		{
			if (this.items_[i] != null && this.items_[i].TrackIdx == base.SelectedId)
			{
				base.ResetLocalPosition(this.items_[i].Offset.y - 78f * (float)Screen.height / 480f);
				return;
			}
		}
	}

	protected void InitializeTrackList()
	{
		int num = TrackAssetDefinitionManager.RANDOM_IDX_ARRAY.Length;
		int num2 = 0;
		int i = 0;
		while (i < num)
		{
			this.items_[num2].SetData(TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[i], 0, 0, GUITrackListItem.enItemLockType.NONE, false);
			i++;
			num2++;
		}
		int num3 = num2;
		for (int j = 0; j < this.visibleAssets_.Count; j++)
		{
			num2 = num3 + j;
			AssetDefinition assetDefinition = this.visibleAssets_[j];
			GUITrackListItem.enItemLockType enItemLockType = GUITrackListItem.enItemLockType.NONE;
			if (assetDefinition.Lock)
			{
				enItemLockType = GUITrackListItem.enItemLockType.NONE;
			}
			bool flag = this.trackState_.DisplayAlert(assetDefinition.Id.ToString());
			if (Facebook.Inst.LoggedIn)
			{
				MonthlyRanking monthlyRanking = new MonthlyRanking(Facebook.Inst.FBID, KartManager.Instance.parameter_.gameMode_, assetDefinition.Id);
				if (monthlyRanking.Load())
				{
					int num4 = monthlyRanking.CalcRank();
					if (num4 == 0)
					{
						int count = monthlyRanking.Records.Count;
						this.items_[num2].SetData(assetDefinition.Id, count + 1, count + 1, enItemLockType, flag);
					}
					else
					{
						this.items_[num2].SetData(assetDefinition.Id, num4, monthlyRanking.Records.Count, enItemLockType, flag);
					}
				}
				else
				{
					this.items_[num2].SetData(assetDefinition.Id, -1, Facebook.Inst.FriendDict.Count, enItemLockType, flag);
				}
			}
			else
			{
				this.items_[num2].SetData(assetDefinition.Id, j, 99, enItemLockType, flag);
			}
		}
	}

	private void FixedUpdate()
	{
		if (this.isMoveToRanking_ || this.isReturnToTrack_)
		{
			this.changeTimeAccum_ += Time.deltaTime;
			Vector3 vector = Vector3.Lerp(this.changeStartPos_, this.changeEndPos_, this.changeTimeAccum_ / this.CHANGE_DURATION);
			base.transform.localPosition = vector;
			if (this.changeTimeAccum_ / this.CHANGE_DURATION >= 1f)
			{
				if (this.isMoveToRanking_)
				{
					base.gameObject.SetActiveRecursively(false);
					this.mask_.SetActiveRecursively(true);
					Vector3 vector2 = this.changeStartPos_;
					this.changeStartPos_ = this.changeEndPos_;
					this.changeEndPos_ = vector2;
					MonoBehaviourMessage2Param<int, RankingType> monoBehaviourMessage2Param = ((MonoBehaviourMessage2Param<int, RankingType>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RANKING)).Initialize(base.SelectedId, RankingType.THIS_MONTH);
					GUIMode.isRankings_ = IsRankings.TRANSITION_RANKINGS_IN;
					base.SendMessage(1026, monoBehaviourMessage2Param);
				}
				else
				{
					this.mask_.SetActiveRecursively(true);
					this.changeStartPos_ = Vector3.zero;
					this.changeEndPos_ = Vector3.zero;
					GUIMode.isRankings_ = IsRankings.NO;
				}
				this.isMoveToRanking_ = false;
				this.isReturnToTrack_ = false;
				this.changeTimeAccum_ = 0f;
			}
		}
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		for (int i = 0; i < this.items_.Length; i++)
		{
			this.items_[i].Update();
		}
		if (this.lastItem_ != null)
		{
			this.lastItem_.Update();
		}
	}

	private void RankingUpdateSuccess()
	{
		this.updating_ &= -2;
		this.InitializeTrackList();
	}

	private void RankingUpdateFailure(Exception ex)
	{
		this.updating_ &= -2;
	}

	private void UpdateRanking()
	{
		this.updating_ |= 1;
		KartOptions.Instance.UpdateQuestFlag();
		KartOptions.Instance.SaveRegistry();
		GUIAccomplishPopup.OpenStaticPopup();
		Facebook inst = Facebook.Inst;
		RankingUpdater rankingUpdater = new RankingUpdater(inst.FBID, inst.FriendDict, FiaAuth.AuthToken);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(rankingUpdater.Update(), new OnSuccess(this.RankingUpdateSuccess), new OnFailure(this.RankingUpdateFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void LoginSuccess()
	{
		StageController.Instance.InputAutority = 3;
		this.updating_ &= -5;
		this.ChangeUser();
		this.UpdateRanking();
	}

	private void LoginFailure(Exception ex)
	{
		StageController.Instance.InputAutority = 3;
		this.updating_ &= -5;
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FACEBOOK_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param.Initialize(1));
		if (ex.GetType() == typeof(FacebookRequestException))
		{
			iOSEvent.Alert("페이스북에 로그인 할 수 없습니다.");
		}
		else if (ex.GetType() == typeof(RequestException))
		{
			iOSEvent.Alert("서버에 연결할 수 없습니다. 다시 시도해주세요.");
		}
		else if (ex.GetType() == typeof(ServerException))
		{
			iOSEvent.Alert(ex.Message);
		}
		else if (ex.GetType() == typeof(FacebookFQLException))
		{
			iOSEvent.Alert("페이스북에 로그인 할 수 없습니다.");
		}
		else if (ex.GetType() == typeof(FacebookCanceledException))
		{
		}
	}

	private void ChangeUser()
	{
		KartOptions.Instance.LoadRegistry();
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(senderId, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI && !base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
			this.isReturnToTrack_ = true;
			this.changeTimeAccum_ = 0f;
			GUIMode.isRankings_ = IsRankings.TRANSITION_TRACK_IN;
			return;
		}
		if (msg.type_ == MonoBehaviourMessageType.RANKING_LOADING_MESSAGE)
		{
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param != null && monoBehaviourMessage1Param.param_ == 1)
			{
				this.InitializeTrackList();
				return;
			}
		}
		else if (base.gameObject.active)
		{
			if (msg.type_ == MonoBehaviourMessageType.ITEM_TO_CTRL && !this.isMoveToRanking_ && !this.isReturnToTrack_)
			{
				MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
				TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition(monoBehaviourMessage2Param.rparam_);
				trackAssetDefinition.Lock = false;
				if (monoBehaviourMessage2Param.rparam_ == -1)
				{
					StageController.Instance.ChangeStage(StageType.STORE);
					return;
				}
				if (trackAssetDefinition != null && trackAssetDefinition.Lock)
				{
					if (this.trackState_.DisplayAlert(monoBehaviourMessage2Param.rparam_))
					{
						this.trackState_.Click(monoBehaviourMessage2Param.rparam_);
						this.InitializeTrackList();
					}
					MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
					monoBehaviourMessage2Param2.Initialize(AssetType.TRACK, monoBehaviourMessage2Param.rparam_);
					if (trackAssetDefinition.LockType == AssetDefinition.enLockType.CASH)
					{
						base.SendMessage(1050, monoBehaviourMessage2Param2);
						return;
					}
				}
				else
				{
					if (base.SelectedId != monoBehaviourMessage2Param.rparam_)
					{
						base.SelectedId = monoBehaviourMessage2Param.rparam_;
						if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
						{
							KartOptions.Instance.SelectItemTrack = base.SelectedId;
						}
						else
						{
							KartOptions.Instance.SelectSpeedTrack = base.SelectedId;
						}
						base.SendMessage(513, this.simpleMessage_.Initialize(0, monoBehaviourMessage2Param.rparam_));
						return;
					}
					if (monoBehaviourMessage2Param.lparam_ == 0)
					{
						if (Facebook.Inst.LoggedIn)
						{
							this.isMoveToRanking_ = true;
							Vector3 localPosition = base.transform.localPosition;
							this.changeStartPos_ = localPosition;
							this.changeEndPos_ = localPosition - Vector3.right * 5f;
							this.changeTimeAccum_ = 0f;
							GUIMode.isRankings_ = IsRankings.TRANSITION_TRACK_OUT;
							return;
						}
						MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE);
						base.SendMessage(23, monoBehaviourMessage1Param2);
						return;
					}
				}
			}
			else if (msg.type_ == MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE)
			{
				StageController.Instance.InputAutority = 0;
				if (iPhoneSettings.internetReachability == iPhoneNetworkReachability.NotReachable)
				{
					StageController.Instance.InputAutority = 3;
					iOSEvent.Alert("?ㅽ듃?뚰겕 ?곌껐???딄꼈?듬땲??");
					return;
				}
				this.updating_ |= 4;
				FiaCoroutine fiaCoroutine = new FiaCoroutine(Facebook.Inst.Login(), new OnSuccess(this.LoginSuccess), new OnFailure(this.LoginFailure));
				base.StartCoroutine(fiaCoroutine);
				return;
			}
			else if (msg.type_ == MonoBehaviourMessageType.UPDATE_SHOPLIST)
			{
				this.InitializeTrackList();
				base.Recalculate();
				if (this.panelManager_.Update())
				{
					this.panelManager_.UpdateMesh(ref this.mesh_);
					return;
				}
			}
			else if (msg.type_ == MonoBehaviourMessageType.UPDATE_RANKING)
			{
				Facebook inst = Facebook.Inst;
				FiaCoroutine fiaCoroutine2 = new FiaCoroutine(new RankingUpdater(inst.FBID, inst.FriendDict, FiaAuth.AuthToken).Update(), new OnSuccess(this.RankingUpdateSuccessForced), new OnFailure(this.RankingUpdateFailureForced));
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param3 = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
				MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param3.Initialize(0));
				base.StartCoroutine(fiaCoroutine2);
			}
		}
	}

	private void RankingUpdateSuccessForced()
	{
		this.InitializeTrackList();
		this.CloseLoadingMessage();
	}

	private void RankingUpdateFailureForced(Exception ex)
	{
		this.CloseLoadingMessage();
	}

	private void CloseLoadingMessage()
	{
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param.Initialize(1));
	}

	private const int TRACK_COUNT = 23;

	private const float START_X = 12f;

	private const float START_Y = 78f;

	private const float DISTANCE = 120f;

	private int trackCount_;

	private GUITrackListItem[] items_;

	private GUILastItem lastItem_;

	private bool isMoveToRanking_;

	private bool isReturnToTrack_;

	private float CHANGE_DURATION = 1f;

	private Vector3 changeStartPos_ = Vector3.zero;

	private Vector3 changeEndPos_ = Vector3.zero;

	private float changeTimeAccum_;

	public GameObject mask_;

	private AlertState trackState_;

	private int updating_;

	private List<AssetDefinition> visibleAssets_;

	private MonoBehaviourMessage2Param<int, int> simpleMessage_;

	private enum UpdatingFlag
	{
		RANKING = 1,
		DEFAULT_NAME_SETTING,
		LOG_IN = 4,
		LOG_OUT = 8
	}
}
