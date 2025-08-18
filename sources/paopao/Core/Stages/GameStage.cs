using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : GameStageBase
{
	protected override void Start()
	{
		iOSEvent.LockUserInteraction();
		base.Start();
		this.CollisionLayerSetting();
		if (KartManager.Instance.parameter_.driveOption_ == DriveOption.RECORD_AI)
		{
			this.aiStartSection_ = new List<int>[6];
			for (int i = 0; i < 6; i++)
			{
				this.aiStartSection_[i] = new List<int>();
			}
			this.aiRecord_ = new KartAIRecord[KartManager.Instance.goCourse_.MaxLap];
			for (int j = 0; j < this.aiRecord_.Length; j++)
			{
				this.aiRecord_[j] = new KartAIRecord();
			}
			AudioListener component = Camera.mainCamera.GetComponent<AudioListener>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		KartJobQueue.Instance.Clear();
		KartJobQueue.Instance.Push(new JobRacingStart());
		KartManager.Instance.currGameStage_ = this;
	}

	private void CollisionLayerSetting()
	{
		Physics.IgnoreLayerCollision(13, 16, KartManager.Instance.parameter_.driveOption_ != DriveOption.RECORD_AI);
	}

	protected void FirstUpdate()
	{
		if (ScreenController.Instance.NeedToBeForced() && ScreenController.Instance.IsDisplayingLandscapeRight())
		{
			ScreenController.Instance.RestoreDisplayingOrientation();
		}
	}

	protected override void Update()
	{
		if (this.isFirstUpdate)
		{
			this.FirstUpdate();
			this.isFirstUpdate = false;
		}
		base.Update();
		this.ProcessJobQueue();
	}

	private void ShowTutorial()
	{
		ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
		changeCameraMessage.Initialize("drive", KartManager.Instance.goPlayKart_);
		MonoBehaviourExCenter.Instance.SendMessage(0, 2, changeCameraMessage);
		this.ReceiveMessage(0, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.TUTORIAL));
	}

	private void ShowTutorial2()
	{
		ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
		changeCameraMessage.Initialize("drive", KartManager.Instance.goPlayKart_);
		MonoBehaviourExCenter.Instance.SendMessage(0, 2, changeCameraMessage);
		this.ReceiveMessage(0, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.TUTORIAL2));
	}

	protected void ProcessJobQueue()
	{
		KartJob kartJob;
		while ((kartJob = KartJobQueue.Instance.Pop()) != null)
		{
			switch (kartJob.type_)
			{
			case KartJobType.RACING_START:
			{
				JobRacingStart jobRacingStart = (JobRacingStart)kartJob;
				if (jobRacingStart != null)
				{
					KartManager.Instance.DriveStartTime = Time.time + jobRacingStart.GetStartTime();
					InGameStatistics.Instance.Initialize();
					this.gameInterface_.PlayAction("start123@action", KartManager.Instance.DriveStartTime - 3f);
					if (!KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.TUTORIAL))
					{
						base.Invoke("ShowTutorial", jobRacingStart.GetStartTime() - 4f);
					}
					else if (KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.TUTORIAL) && !KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.TUTORIAL2))
					{
						base.Invoke("ShowTutorial2", jobRacingStart.GetStartTime() - 4f);
					}
				}
				break;
			}
			case KartJobType.FINISH_NOTICES:
			{
				JobFinishNotices jobFinishNotices = (JobFinishNotices)kartJob;
				KartManager.Instance.DriveEndTime = jobFinishNotices.GetDriveEndTime();
				if (this.driveState_ == GameStageBase.DriveState.DRIVING)
				{
					this.retireCountHorn_ = 0;
					this.retireCountTime_ = KartManager.Instance.DriveEndTime - 10f;
					this.gameInterface_.PlayAction("finish_count@action", this.retireCountTime_);
				}
				break;
			}
			case KartJobType.RACE_OVER:
			{
				JobRacingOver jobRacingOver = (JobRacingOver)kartJob;
				if (jobRacingOver != null)
				{
					this.raceOverTime_ = jobRacingOver.GetRaceOverTime();
					if (this.driveState_ == GameStageBase.DriveState.DRIVING)
					{
						if (this.bgm_ != null)
						{
							this.bgm_.Stop();
						}
						if (this.commonBgmSource_ != null)
						{
							this.commonBgmSource_.PlayOneShot(this.commonBgmClip_[2]);
						}
						this.boosterGauge_.SetActiveRecursively(false);
						this.gameInterface_.ShowBlackBar(true, true);
						MonoBehaviourMessage2Param<CharacterAnimation, WrapMode> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION);
						monoBehaviourMessage2Param.Initialize(CharacterAnimation.LOSE_GAME, WrapMode.Loop);
						MonoBehaviourExCenter.Instance.SendMessage(0, MonoBehaiourExConst.PLAYER_KART, monoBehaviourMessage2Param);
						this.gameInterface_.PlayAction("retire@action", Time.time);
						this.driveState_ = GameStageBase.DriveState.RACE_OVER;
						KartManager.Instance.goKart_[KartManager.PLAYER_KART_IDX].Stuck = true;
						base.RaceOverSetting();
					}
				}
				break;
			}
			case KartJobType.TIME_OVER:
			{
				JobTimeOver jobTimeOver = (JobTimeOver)kartJob;
				KartManager.Instance.DriveEndTime = jobTimeOver.GetDriveEndTime();
				if (this.driveState_ == GameStageBase.DriveState.DRIVING)
				{
					this.retireCountHorn_ = 0;
					this.retireCountTime_ = KartManager.Instance.DriveEndTime - 10f;
					this.gameInterface_.PlayAction("finish_count@action", this.retireCountTime_);
				}
				break;
			}
			}
		}
	}

	protected override void OnPlayRaceOver()
	{
		bool flag = false;
		bool flag2 = false;
		KartManager.Instance.result_ = KartManager.Instance.goCourse_.GetRaceResult();
		GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(KartManager.Instance.parameter_.track_, (int)KartManager.Instance.parameter_.gameMode_);
		if (!KartManager.Instance.result_.IsRetire(KartManager.PLAYER_KART_IDX))
		{
			if (bestInfo != null)
			{
				float finishTime = KartManager.Instance.goCourse_.GetFinishTime();
				flag = finishTime > 0f && finishTime < bestInfo.finishTime_;
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			GhostFilenameInfo ghostFilenameInfo = new GhostFilenameInfo();
			ghostFilenameInfo.trackIdx_ = KartManager.Instance.parameter_.track_;
			ghostFilenameInfo.kartIdx_ = (byte)KartOptions.Instance.Kart;
			ghostFilenameInfo.characterIdx_ = (byte)KartOptions.Instance.Character;
			ghostFilenameInfo.finishTime_ = KartManager.Instance.goCourse_.GetFinishTime();
			ghostFilenameInfo.serialzedTime_ = DateTime.Now.ToBinary();
			string text = ghostFilenameInfo.GenerateFilename();
			ghostFilenameInfo.filePath_ = text;
			KartOptions.Instance.SetBestInfo(ghostFilenameInfo, (int)KartManager.Instance.parameter_.gameMode_);
		}
		if (!KartManager.Instance.result_.IsRetire(KartManager.PLAYER_KART_IDX))
		{
			Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
			if (facebook.LoggedIn)
			{
				flag = false;
				MonthlyRanking monthlyRanking = new MonthlyRanking(facebook.FBID, KartManager.Instance.parameter_.gameMode_, (int)KartManager.Instance.parameter_.track_);
				if (monthlyRanking.Load())
				{
					Record record = monthlyRanking.FindRecordWithID(facebook.FBID);
					if (record != null && KartManager.Instance.goCourse_.GetFinishTime() < record.time_)
					{
						flag2 = true;
					}
				}
				else
				{
					flag2 = true;
				}
			}
		}
		KartManager.Instance.result_.IsMonthlyBestRecord = flag2;
		KartManager.Instance.result_.IsBestRecord = flag;
		if (this.aiRecord_ != null)
		{
			string text2 = DateTime.Now.ToString("MMddHHmmss");
			for (int i = 0; i < this.aiRecord_.Length; i++)
			{
				if (this.aiRecord_[i] != null)
				{
					string text3 = string.Format("ai_{0}_{1}_{2}_{3}_{4}.bin", new object[]
					{
						TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_).Name,
						text2,
						i,
						(i != 0) ? this.aiStartSection_[i - 1][this.aiStartSection_[i - 1].Count - 1] : StaticOption.startPosition_,
						this.aiStartSection_[i][this.aiStartSection_[i].Count - 1]
					});
					this.aiRecord_[i].SerializeToBin(text3);
				}
			}
		}
		AlertStateFactory.Instance.GetAlertState(AlertStateType.TRACK).Ride((int)KartManager.Instance.parameter_.track_);
		if (KartManager.Instance.IsTestDrive())
		{
			this.ChangeTestLobbyStage();
		}
	}

	protected override void OnRacingTimeOver()
	{
		KartJobQueue.Instance.Push(new JobTimeOver(Time.time + 10f));
	}

	protected override void UpdateState(float tick)
	{
		base.UpdateState(tick);
		GameStageBase.DriveState driveState_ = this.driveState_;
		if (driveState_ == GameStageBase.DriveState.LOADING)
		{
			this.driveState_ = GameStageBase.DriveState.READY;
		}
	}

	private void Test()
	{
		if (Input.GetKeyDown(KeyCode.X))
		{
			this.gameInterface_.PlayAction("winner@action", Time.time);
		}
		if (Input.GetKeyDown(KeyCode.Z))
		{
			this.gameInterface_.PlayAction("retire@action", Time.time);
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			this.ShowTutorial();
		}
		if (Input.GetKeyDown(KeyCode.Q))
		{
			this.ShowTutorial2();
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			this.gameInterface_.ShowBlackBar(true, true);
			int goKartCount = KartManager.Instance.GetGoKartCount();
			RaceResult raceResult = new RaceResult(goKartCount);
			for (int i = 0; i < raceResult.elems_.Length; i++)
			{
				raceResult.SetResult(goKartCount - i - 1, i, 83.45f, "Kaiser" + i.ToString());
			}
			KartManager.Instance.result_ = raceResult;
			MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(true, true));
		}
		else if (Input.GetKeyDown(KeyCode.B))
		{
			this.gameInterface_.HideBlackBar(true, false);
			KartManager.Instance.result_ = null;
			MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(false, true));
		}
		else if (Input.GetKeyDown(KeyCode.C))
		{
			this.gameInterface_.ShowBlackBar(true, true);
			MonoBehaviourExCenter.Instance.SendMessage(0, 18, ((MonoBehaviourMessage2Param<float, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NEW_RECORD)).Initialize(83.45f, true));
			base.Invoke("GoResult", 0.9f);
		}
		else if (Input.GetKeyDown(KeyCode.D))
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
			monoBehaviourMessage1Param.Initialize(true);
			MonoBehaviourExCenter.Instance.SendMessage(0, 19, monoBehaviourMessage1Param);
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
			monoBehaviourMessage1Param2.Initialize(false);
			MonoBehaviourExCenter.Instance.SendMessage(0, 19, monoBehaviourMessage1Param2);
		}
		else if (Input.GetKeyDown(KeyCode.F))
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param3 = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
			monoBehaviourMessage1Param3.Initialize(true);
			MonoBehaviourExCenter.Instance.SendMessage(1, 20, monoBehaviourMessage1Param3);
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
			MonoBehaviourExCenter.Instance.SendMessage(0, 266, monoBehaviourMessage2Param.Initialize(AssetType.SIZE, 1));
		}
		bool flag = false;
		if (Input.GetKeyDown(KeyCode.T))
		{
			this.cameraMode_ = "topview";
			flag = true;
		}
		else if (Input.GetKeyDown(KeyCode.D))
		{
			this.cameraMode_ = "drive";
			flag = true;
		}
		else if (Input.GetKeyDown(KeyCode.Tab))
		{
			for (int j = this.cameraFocus_ + 1; j < this.cameraFocus_ + 6; j++)
			{
				if (KartManager.Instance.goKart_[j % 6] != null)
				{
					this.cameraFocus_ = j % 6;
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
			changeCameraMessage.Initialize(this.cameraMode_, KartManager.Instance.goKart_[this.cameraFocus_]);
			MonoBehaviourExCenter.Instance.SendMessage(0, 2, changeCameraMessage);
		}
	}

	protected override void ProcessRaceOver()
	{
		if (KartManager.Instance.DriveEndTime > 0f && KartManager.Instance.DriveEndTime <= Time.time && this.raceOverTime_ <= 0f)
		{
			float num = Time.time;
			if (KartManager.Instance.goCourse_.GetFinishTime() <= 0f)
			{
				num += 3f;
			}
			KartJobQueue.Instance.Push(new JobRacingOver(num));
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.GOAL_IN)
		{
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				KartManager.Instance.kartState_[monoBehaviourMessage1Param.param_] = KartState.GOAL_IN;
				if (KartManager.Instance.DriveEndTime <= 0f && monoBehaviourMessage1Param.param_ == KartManager.PLAYER_KART_IDX)
				{
					KartJobQueue.Instance.Push(new JobFinishNotices(Time.time + 5f));
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.ENTER_USER_SECTION)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param != null && this.aiRecord_ != null)
			{
				int lap = KartManager.Instance.goCourse_.GetLap(KartManager.PLAYER_KART_IDX);
				int maxLap = KartManager.Instance.goCourse_.MaxLap;
				if (monoBehaviourMessage2Param.lparam_ == 1)
				{
					int latestPassingPlane = KartManager.Instance.goCourse_.GetLatestPassingPlane();
					int num = KartManager.Instance.goCourse_.passPlane_.Length / 2;
					if (!KartManager.Instance.goCourse_.passPlaneSequence_.IsLastPlane(latestPassingPlane) && latestPassingPlane > num)
					{
						if (MathHelper.IsBetweenIE(lap - 1, 0, this.aiStartSection_.Length))
						{
							this.aiStartSection_[lap - 1].Add(monoBehaviourMessage2Param.rparam_);
						}
						if (MathHelper.IsBetweenIE(lap, 0, this.aiStartSection_.Length))
						{
							this.aiStartSection_[lap].Add(monoBehaviourMessage2Param.rparam_);
						}
					}
					else
					{
						if (MathHelper.IsBetweenIE(lap - 1, 0, this.aiStartSection_.Length))
						{
							this.aiStartSection_[lap - 1].Add(monoBehaviourMessage2Param.rparam_);
						}
						if (MathHelper.IsBetweenIE(lap - 2, 0, this.aiStartSection_.Length))
						{
							this.aiStartSection_[lap - 2].Add(monoBehaviourMessage2Param.rparam_);
						}
					}
				}
				else if (lap > 0 && lap <= maxLap)
				{
					this.aiRecord_[lap - 1].AddUserSection(monoBehaviourMessage2Param.rparam_);
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.ITEM)
		{
			MonoBehaviourMessage2Param<GameItem, ItemParam> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<GameItem, ItemParam>)msg;
			if (monoBehaviourMessage2Param2 != null)
			{
				ItemBasicController item = base.GetItem(monoBehaviourMessage2Param2.lparam_);
				if (item != null)
				{
					item.Initialize(monoBehaviourMessage2Param2.rparam_);
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.GAMESTAGE_COMMAND)
		{
			MonoBehaviourMessage1Param<GameStageCommand> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<GameStageCommand>)msg;
			if (monoBehaviourMessage1Param2 != null)
			{
				if (monoBehaviourMessage1Param2.param_ == GameStageCommand.PAUSE || monoBehaviourMessage1Param2.param_ == GameStageCommand.TUTORIAL || monoBehaviourMessage1Param2.param_ == GameStageCommand.TUTORIAL2)
				{
					Time.timeScale = 0f;
					if (this.bgm_ != null && monoBehaviourMessage1Param2.param_ == GameStageCommand.PAUSE)
					{
						this.bgm_.Pause();
					}
					if (this.commonBgmSource_ != null)
					{
						this.commonBgmSource_.Pause();
					}
					for (int i = 0; i < 6; i++)
					{
						if (this.audioSource_[i] != null)
						{
							this.audioSource_[i].Pause();
						}
					}
					this.boosterGauge_.SetActiveRecursively(false);
					GC.Collect();
					MonoBehaviourMessageType monoBehaviourMessageType = MonoBehaviourMessageType.PAUSE;
					if (monoBehaviourMessage1Param2.param_ == GameStageCommand.PAUSE)
					{
						monoBehaviourMessageType = MonoBehaviourMessageType.PAUSE;
					}
					else if (monoBehaviourMessage1Param2.param_ == GameStageCommand.TUTORIAL)
					{
						monoBehaviourMessageType = MonoBehaviourMessageType.SHOW_TUTORIAL;
					}
					else if (monoBehaviourMessage1Param2.param_ == GameStageCommand.TUTORIAL2)
					{
						monoBehaviourMessageType = MonoBehaviourMessageType.SHOW_TUTORIAL2;
					}
					base.BroadcastMessage(new MonoBehaviourMessage(monoBehaviourMessageType));
					KartManager.Instance.isPaused_ = true;
				}
				else if (monoBehaviourMessage1Param2.param_ == GameStageCommand.RESUME)
				{
					Time.timeScale = 1f;
					if (this.bgm_ != null)
					{
						this.bgm_.Resume();
					}
					if (this.commonBgmSource_ != null)
					{
						this.commonBgmSource_.Resume();
					}
					for (int j = 0; j < 6; j++)
					{
						if (this.audioSource_[j] != null)
						{
							this.audioSource_[j].Resume();
						}
					}
					if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED)
					{
						this.boosterGauge_.SetActiveRecursively(true);
					}
					base.BroadcastMessage(new MonoBehaviourMessage(MonoBehaviourMessageType.RESUME));
					KartManager.Instance.isPaused_ = false;
				}
				else if (monoBehaviourMessage1Param2.param_ == GameStageCommand.QUIT)
				{
					Time.timeScale = 1f;
					this.driveState_ = GameStageBase.DriveState.GO_FINAL_STAGE;
					if (KartManager.Instance.IsTestDrive())
					{
						this.ChangeTestLobbyStage();
					}
					else if (StageController.IsInstantiated())
					{
						StageController.Instance.ChangeStage((KartManager.Instance.parameter_.gameMode_ != GameMode.SINGLE_ITEM) ? StageType.SINGLE_SPEED : StageType.SINGLE_ITEM);
					}
				}
				else if (monoBehaviourMessage1Param2.param_ == GameStageCommand.RESTART)
				{
					Time.timeScale = 1f;
					this.Restart();
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			this.UnloadStage();
		}
	}

	protected override void UnloadStage()
	{
		KartManager.Instance.parameter_.startPositions_ = null;
		KartManager.Instance.currGameStage_ = null;
	}

	private void FixedUpdate()
	{
		if (this.aiRecord_ != null && KartManager.Instance.DriveStartTime >= 0f && (this.driveState_ == GameStageBase.DriveState.READY || this.driveState_ == GameStageBase.DriveState.DRIVING || this.driveState_ == GameStageBase.DriveState.RACE_OVER))
		{
			int lap = KartManager.Instance.goCourse_.GetLap(KartManager.PLAYER_KART_IDX);
			int maxLap = KartManager.Instance.goCourse_.MaxLap;
			if (this.aiRecordLap_ < lap)
			{
				float lapTime = KartManager.Instance.goCourse_.GetLapTime(KartManager.PLAYER_KART_IDX, this.aiRecordLap_ - 1);
				this.aiRecord_[this.aiRecordLap_ - 1].AddRecord(KartManager.Instance.GetPlayTime() - lapTime);
				this.aiRecordLap_ = lap;
			}
			if (lap > 0 && lap <= maxLap)
			{
				float lapTime2 = KartManager.Instance.goCourse_.GetLapTime(KartManager.PLAYER_KART_IDX, lap - 1);
				this.aiRecord_[lap - 1].AddRecord(KartManager.Instance.GetPlayTime() - lapTime2);
			}
		}
	}

	private void Restart()
	{
		if (ScreenController.Instance != null)
		{
			ScreenController.Instance.DisplayingOrientation = ScreenController.Instance.OriginalOrientation;
			StageController.Instance.ChangeStage(StageType.GAME);
		}
	}

	private void ChangeTestLobbyStage()
	{
		MonoBehaviourExCenter.Instance.UnloadStage();
		GUIFontManager.Instance.Clear();
		GC.Collect();
		Application.LoadLevel(StaticOption.GetLoadStageName());
	}

	protected KartAIRecord[] aiRecord_;

	protected List<int>[] aiStartSection_;

	protected int aiRecordLap_ = 1;

	private bool isFirstUpdate = true;

	private int cameraFocus_;

	private string cameraMode_ = "drive";
}
