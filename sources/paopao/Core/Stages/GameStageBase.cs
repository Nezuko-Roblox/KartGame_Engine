using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStageBase : MonoBehaviourEx
{
	protected virtual void Awake()
	{
		Time.timeScale = 1f;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (StageController.IsInstantiated())
		{
			StageController.Instance.BeginStage();
		}
		else if (FadeInOut.IsInstantiated())
		{
			FadeInOut.Instance.FadeIn();
		}
		if (GUIKartViewer.IsInstantiated())
		{
			GUIKartViewer.Instance.Hide();
		}
		MonoBehaiourExConst.Initialize();
		this.driveState_ = GameStageBase.DriveState.LOADING;
		CameraManager.Instance.Initialize();
		KartManager.Instance.InitGameData();
		GC.Collect();
		if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
		{
			this.InitializeItems();
		}
		string mainAsset = TrackAssetDefinitionManager.Instance.GetMainAsset((int)KartManager.Instance.parameter_.track_);
		GameObject gameObject = base.gameObject;
		this.InitializeGoCourse(mainAsset);
		this.InitializeKarts(gameObject, mainAsset);
		KartManager.Instance.gameInterface_ = new GameInterface();
		this.gameInterface_ = KartManager.Instance.gameInterface_;
		this.ObjectSetting();
		GameObject gameObjectMainAsset = ResourceLoader.Instance.GetGameObjectMainAsset(mainAsset, true);
		if (gameObjectMainAsset != null)
		{
			gameObjectMainAsset.isStatic = true;
			foreach (object obj in gameObjectMainAsset.transform)
			{
				Transform transform = (Transform)obj;
				if (transform.name != "itemcube")
				{
					transform.gameObject.isStatic = true;
				}
			}
			gameObjectMainAsset.isStatic = true;
			gameObjectMainAsset.name = TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_).Name;
			FiaUtil.AttachChild(ref gameObject, ref gameObjectMainAsset);
			foreach (object obj2 in gameObjectMainAsset.transform)
			{
				Transform transform2 = (Transform)obj2;
				if (((1 << transform2.gameObject.layer) & 2048) != 0)
				{
					GUIMinimapMark.MINIMAP_OFFSET = transform2.localPosition.y;
					break;
				}
			}
		}
		this.RegistMonoBehaviour(1);
		if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED)
		{
			foreach (object obj3 in gameObjectMainAsset.transform)
			{
				Transform transform3 = (Transform)obj3;
				if (transform3.name == "itemcube")
				{
					transform3.gameObject.SetActiveRecursively(false);
				}
			}
		}
		else if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
		{
			this.boosterGauge_.SetActiveRecursively(false);
			foreach (object obj4 in gameObjectMainAsset.transform)
			{
				Transform transform4 = (Transform)obj4;
				if (transform4.name == "itemcube")
				{
					foreach (object obj5 in transform4)
					{
						Transform transform5 = (Transform)obj5;
						transform5.gameObject.AddComponent<ItemBox>();
					}
					break;
				}
			}
		}
		KartManager.Instance.goCourse_.GenerateStartInfos(KartManager.Instance.parameter_.startPositions_);
		string[] array = new string[] { "forest", "desert", "village", "ice", "tomb", "xmas" };
		int[] array2 = new int[] { 3, 4, 4, 3, 3, 2 };
		string text;
		if (mainAsset == "village_i04_r" || mainAsset == "village_r01_r" || mainAsset == "village_i05_r")
		{
			text = "xmas";
		}
		else
		{
			text = mainAsset.Split(new char[] { '_' })[0];
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (text == array[i])
			{
				string text2 = string.Concat(new string[]
				{
					"bgm/",
					text,
					"/",
					text,
					"_",
					global::UnityEngine.Random.Range(1, array2[i] + 1).ToString("D2")
				});
				AudioClip audioClip = (AudioClip)Resources.Load(text2, typeof(AudioClip));
				if (audioClip != null)
				{
					SoundController.CreateAudioSource(base.gameObject, new FxClipSetting(audioClip, true), out this.bgm_, AudioSourceType.BGM);
				}
			}
		}
		if (this.commonBgmClip_.Length > 0)
		{
			this.commonBgmSource_ = new AudioSourceEx(base.gameObject.AddComponent<AudioSource>(), AudioSourceType.BGM);
			this.commonBgmSource_.loop = false;
			this.commonBgmSource_.playOnAwake = false;
		}
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
	}

	protected virtual void InitializeGoCourse(string trackAssetName)
	{
		BinaryAsset binaryAsset = (BinaryAsset)ResourceLoader.Instance.GetAsset(trackAssetName, "pass_plane");
		if (binaryAsset != null)
		{
			if (ResourceLoader.Instance.IsExistAsset(trackAssetName, "pass_plane_sequence"))
			{
				BinaryAsset binaryAsset2 = (BinaryAsset)ResourceLoader.Instance.GetAsset(trackAssetName, "pass_plane_sequence");
				KartManager.Instance.InitGoCourse(binaryAsset, binaryAsset2);
			}
			else
			{
				KartManager.Instance.InitGoCourse(binaryAsset);
			}
			KartManager.Instance.goCourse_.MaxLap = KartManager.Instance.parameter_.maxLap_;
			KartManager.Instance.goCourse_.InitPassPlane();
		}
	}

	private GameStageBase.AIFileInfo GetAIInfoFromFilename(string path)
	{
		char[] array = new char[] { '/', '\\' };
		string[] array2 = path.Split(array, StringSplitOptions.RemoveEmptyEntries);
		string text = array2[array2.Length - 1];
		GameStageBase.AIFileInfo aifileInfo = default(GameStageBase.AIFileInfo);
		aifileInfo.isStart_ = text[0] == 's';
		int num = int.Parse(text.Substring(1));
		aifileInfo.from_ = num / 10000;
		aifileInfo.to_ = num % 10000 / 1000;
		aifileInfo.no_ = num % 1000;
		return aifileInfo;
	}

	private void GenerateAIOrder(StringHolder aiInfo, out GameStageBase.AIFileInfo[,] aiOrders)
	{
		int[] array = new int[13];
		for (int i = 0; i < 13; i++)
		{
			array[i] = int.Parse(aiInfo.content[i]);
		}
		int maxLap = KartManager.Instance.goCourse_.MaxLap;
		int num = 6 / (array[6] - array[0]) + 1;
		int num2 = 6 * (maxLap - 1) / (array[12] - array[6]) + 1;
		aiOrders = new GameStageBase.AIFileInfo[6, maxLap];
		List<int>[] array2 = new List<int>[6];
		for (int j = 0; j < 6; j++)
		{
			array2[j] = null;
			if (array[j] < array[j + 1])
			{
				array2[j] = new List<int>();
				for (int k = 0; k < num; k++)
				{
					List<int> list;
					FiaUtil.GetRandomList(array[j], array[j + 1] - 1, out list);
					array2[j].AddRange(list);
				}
			}
		}
		for (int l = 0; l < 6; l++)
		{
			int num3 = KartManager.Instance.parameter_.startPositions_[l];
			while (array2[num3] == null || array2[num3].Count <= 0)
			{
				num3 = (num3 + 1) % 6;
			}
			aiOrders[l, 0] = this.GetAIInfoFromFilename(aiInfo.content[array2[num3][0]]);
			array2[num3].RemoveAt(0);
		}
		List<int>[] array3 = new List<int>[6];
		for (int m = 6; m < 12; m++)
		{
			array3[m - 6] = null;
			if (array[m] < array[m + 1])
			{
				array3[m - 6] = new List<int>();
				for (int n = 0; n < num2; n++)
				{
					List<int> list2;
					FiaUtil.GetRandomList(array[m], array[m + 1] - 1, out list2);
					array3[m - 6].AddRange(list2);
				}
			}
		}
		for (int num4 = 1; num4 < maxLap; num4++)
		{
			for (int num5 = 0; num5 < 6; num5++)
			{
				int num6 = aiOrders[num5, num4 - 1].to_;
				while (array3[num6] == null || array3[num6].Count <= 0)
				{
					num6 = (num6 + 1) % 6;
				}
				aiOrders[num5, num4] = this.GetAIInfoFromFilename(aiInfo.content[array3[num6][0]]);
				array3[num6].RemoveAt(0);
			}
		}
	}

	protected void GetAiRacingInfoByMiru(out List<GameStageBase.AiRacingInfo> racingInfoList)
	{
		byte track_ = KartManager.Instance.parameter_.track_;
		TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)track_);
		GameMode gameMode_ = KartManager.Instance.parameter_.gameMode_;
		int num = KartOptions.Instance.GetWinCounter(track_)[(int)gameMode_];
		racingInfoList = new List<GameStageBase.AiRacingInfo>();
		if (trackAssetDefinition == null)
		{
			return;
		}
		float num2;
		float num3;
		int num4;
		MedalType medalType;
		if (num == 0)
		{
			num2 = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.BRONZE);
			num3 = 2.5f;
			num4 = global::UnityEngine.Random.Range(0, 2);
			medalType = MedalType.BRONZE;
		}
		else if (num == 1)
		{
			num2 = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.SILVER);
			num3 = Mathf.Min(2.5f, (trackAssetDefinition.GetMedalTime(gameMode_, MedalType.BRONZE) - num2) / 2f);
			num4 = global::UnityEngine.Random.Range(2, 4);
			medalType = MedalType.SILVER;
		}
		else
		{
			num2 = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.GOLD);
			num3 = Mathf.Min(2.5f, (trackAssetDefinition.GetMedalTime(gameMode_, MedalType.SILVER) - num2) / 2f);
			num4 = global::UnityEngine.Random.Range(4, 6);
			medalType = MedalType.GOLD;
		}
		float[] array = new float[6];
		float[] array2 = new float[6];
		List<AssetDefinition> assetDefinitionList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
		float level = ((KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].body_]).Level;
		for (int i = 0; i < 6; i++)
		{
			if (KartManager.Instance.parameter_.kart_[i] != null)
			{
				KartAssetDefinition kartAssetDefinition = (KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[i].body_];
				array[i] = level - kartAssetDefinition.Level;
				array2[i] = kartAssetDefinition.RandomRange;
			}
		}
		int num5 = 0;
		for (int j = 0; j < 6; j++)
		{
			if (j != KartManager.PLAYER_KART_IDX)
			{
				racingInfoList.Add(new GameStageBase.AiRacingInfo(AIControllerType.FIXED_DELTA, num2 + num3 * (float)j + array[j] + FiaUtil.GetRandom(-0.5f, 0.5f + array2[j]), j <= num4, medalType));
				num5++;
			}
		}
	}

	protected void GetAiRacingInfo(out List<GameStageBase.AiRacingInfo> racingInfoList)
	{
		byte track_ = KartManager.Instance.parameter_.track_;
		TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)track_);
		GameMode gameMode_ = KartManager.Instance.parameter_.gameMode_;
		GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(track_, (int)gameMode_);
		racingInfoList = new List<GameStageBase.AiRacingInfo>();
		if (trackAssetDefinition == null)
		{
			return;
		}
		if (bestInfo == null)
		{
			float medalTime = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.BRONZE);
			for (int i = 0; i < 5; i++)
			{
				racingInfoList.Add(new GameStageBase.AiRacingInfo((AIControllerType)i, medalTime + (float)i - 2f + global::UnityEngine.Random.Range(-0.25f, 0.25f), false, MedalType.BRONZE));
			}
		}
		else
		{
			float num = bestInfo.finishTime_;
			float[] medalTime2 = trackAssetDefinition.GetMedalTime(gameMode_);
			MedalType medalType = MedalType.BRONZE;
			if (bestInfo.finishTime_ <= medalTime2[0])
			{
				num = medalTime2[0];
				medalType = MedalType.GOLD;
			}
			else if (bestInfo.finishTime_ > medalTime2[1])
			{
				num = medalTime2[1];
				medalType = MedalType.SILVER;
			}
			float[] array = new float[6];
			float[] array2 = new float[6];
			List<AssetDefinition> assetDefinitionList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
			float level = ((KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].body_]).Level;
			for (int j = 0; j < 6; j++)
			{
				if (KartManager.Instance.parameter_.kart_[j] != null)
				{
					KartAssetDefinition kartAssetDefinition = (KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[j].body_];
					array[j] = level - kartAssetDefinition.Level;
					array2[j] = kartAssetDefinition.RandomRange;
				}
			}
			int num2 = 0;
			for (int k = 0; k < 6; k++)
			{
				if (k != KartManager.PLAYER_KART_IDX)
				{
					racingInfoList.Add(new GameStageBase.AiRacingInfo(AIControllerType.FIXED_DELTA, num + array[k] + (float)num2 * array2[k] + FiaUtil.GetRandom(0f, array2[k]), false, medalType));
					num2++;
				}
			}
		}
	}

	protected virtual void InitializeKarts(GameObject gameStageObject, string trackAssetName)
	{
		Parameter parameter_ = KartManager.Instance.parameter_;
		StringHolder stringHolder = (StringHolder)ResourceLoader.Instance.GetAsset(trackAssetName, "ai_info");
		int maxLap = KartManager.Instance.goCourse_.MaxLap;
		GameStageBase.AIFileInfo[,] array = null;
		List<GameStageBase.AiRacingInfo> list = null;
		if (KartManager.Instance.parameter_.startPositions_ == null)
		{
			FiaUtil.GetRandomList(0, 5, out KartManager.Instance.parameter_.startPositions_);
		}
		if (KartManager.Instance.parameter_.Stage == StageType.GAME)
		{
			if (stringHolder != null)
			{
				this.GenerateAIOrder(stringHolder, out array);
			}
			this.GetAiRacingInfoByMiru(out list);
		}
		if (this.playerPrefab_ != null && this.ghostPrefab_ != null && this.aiPrefab_ != null)
		{
			for (int i = 0; i < 6; i++)
			{
				if (parameter_.kart_[i] != null)
				{
					GameObject gameObject = null;
					if (parameter_.kart_[i].type_ == PlayerType.PLAYER)
					{
						if (this.player_ == null)
						{
							this.player_ = (RigidbodyFPSWalker)global::UnityEngine.Object.Instantiate(this.playerPrefab_);
							gameObject = this.player_.gameObject;
						}
					}
					else if (parameter_.kart_[i].type_ == PlayerType.GHOST)
					{
						this.ghostPrefab_.kartIndex_ = i;
						char[] array2 = new char[] { ';' };
						string[] array3 = KartManager.Instance.parameter_.kart_[i].reserved_.Split(array2, StringSplitOptions.RemoveEmptyEntries);
						this.ghostPrefab_.recordName_ = array3[0];
						this.ghostPrefab_.isResourceDirectory_ = array3[1] == "1";
						GhostController ghostController = (GhostController)global::UnityEngine.Object.Instantiate(this.ghostPrefab_);
						gameObject = ghostController.gameObject;
					}
					else if (parameter_.kart_[i].type_ == PlayerType.AI)
					{
						this.aiPrefab_.kartIndex_ = i;
						this.aiPrefab_.recordName_ = new string[maxLap];
						if (!false && stringHolder != null)
						{
							for (int j = 0; j < maxLap; j++)
							{
								this.aiPrefab_.recordName_[j] = array[i, j].ToString();
							}
							this.aiPrefab_.isRecordInAssetBundle_ = true;
							AIController aicontroller = (AIController)global::UnityEngine.Object.Instantiate(this.aiPrefab_);
							GameStageBase.AiRacingInfo aiRacingInfo = list[0];
							aicontroller.SetAiControllerType(aiRacingInfo.type_, aiRacingInfo.targetTime_, aiRacingInfo.useStartBoost_, aiRacingInfo.medalType_);
							list.RemoveAt(0);
							gameObject = aicontroller.gameObject;
						}
					}
					else if (parameter_.kart_[i].type_ == PlayerType.NET)
					{
						this.netPrefab_.kartIndex_ = i;
						NetController netController = (NetController)global::UnityEngine.Object.Instantiate(this.netPrefab_);
						gameObject = netController.gameObject;
					}
					if (gameObject != null)
					{
						FiaUtil.AttachChild(ref gameStageObject, ref gameObject);
					}
				}
			}
		}
	}

	private void UnlockUserInteraction()
	{
		iOSEvent.UnlockUserInteraction();
	}

	protected virtual void Start()
	{
		this.gameInterface_.Initialize(this);
		this.gameInterface_.SetItemSlotCnt(2);
		this.driveStartHorn_ = 0;
		this.retireCountHorn_ = 0;
		this.retireCountTime_ = 0f;
		this.kartDriveTime_ = 0f;
		this.raceOverTime_ = 0f;
		this.beDoomed_ = false;
		FxClipSetting[] array = new FxClipSetting[]
		{
			new FxClipSetting(this.hornClip_, false),
			new FxClipSetting(this.goClip_, false),
			new FxClipSetting(this.finalLapClip_, false),
			new FxClipSetting(this.lap2Clip_, false),
			new FxClipSetting(this.retireHorn_, false),
			new FxClipSetting(this.resetClip_, false)
		};
		for (int i = 0; i < 6; i++)
		{
			SoundController.CreateAudioSource(base.gameObject, array[i], out this.audioSource_[i]);
		}
		this.gameStageMode_ = GameStageBase.GameStageMode.BEFORE_RACING;
		KartManager.Instance.Stuck = true;
		string text = "ready_camera_ani";
		if (KartManager.Instance.parameter_.startPositions_ != null)
		{
			List<int> startPositions_ = KartManager.Instance.parameter_.startPositions_;
			int num = startPositions_[KartManager.PLAYER_KART_IDX];
			if (num < startPositions_.Count / 2)
			{
				text = "ready_camera_ani2";
			}
		}
		ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
		changeCameraMessage.Initialize(text, KartManager.Instance.goPlayKart_, 0.3f, 1800f, 45f, WrapMode.Once);
		MonoBehaviourExCenter.Instance.SendMessage(0, 2, changeCameraMessage);
		BackgroundNotifier.resign_ = new BackgroundDelegate(this.ResignedActive);
		BackgroundNotifier.active_ = new BackgroundDelegate(this.BecameActive);
		if (this.bgm_ != null)
		{
			this.bgm_.Play();
		}
		base.Invoke("UnlockUserInteraction", 1f);
	}

	protected virtual void Update()
	{
		float time = Time.time;
		this.gameInterface_.Update(time);
		this.ProcessInterfaceData(time);
		if (this.kartResetState_ != 0)
		{
			GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
			if (this.kartResetState_ == 1 && time > this.kartResetTick_ + 0.5f)
			{
				KartManager.Instance.goCourse_.ResetKart(KartManager.PLAYER_KART_IDX);
				this.kartResetState_ = 2;
			}
			if (this.kartResetState_ == 2 && time > this.kartResetTick_ + 1f)
			{
				goPlayKart_.IsInResetState = false;
				this.kartResetState_ = 3;
			}
			if (this.kartResetState_ == 3 && time > this.kartResetTick_ + 3f)
			{
				goPlayKart_.Valid = true;
				this.kartResetState_ = 0;
				this.kartResetTick_ = 0f;
			}
		}
		this.UpdateState(Time.time);
		this.ProcessRaceOver();
	}

	private void ObjectSetting()
	{
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name == "booster_gauge_ipad")
			{
				this.boosterGauge_ = transform.gameObject;
				foreach (object obj2 in transform)
				{
					Transform transform2 = (Transform)obj2;
					if (transform2.name == "booster_gauge_03")
					{
						this.boosterGaugeBar_ = transform2;
					}
				}
			}
			else if (!(transform.name == "gui_all"))
			{
				if (transform.name.Contains("@action"))
				{
					this.gameInterface_.AddAction(transform.name, transform.gameObject);
				}
			}
			if (transform.name.Contains("debug_label"))
			{
				transform.gameObject.SetActiveRecursively(false);
			}
		}
	}

	protected bool IsUpdateHorn(float tick)
	{
		return (double)KartManager.Instance.DriveStartTime <= (double)tick + (3.0 - (double)((float)this.driveStartHorn_ * 1f));
	}

	protected float MakeServerTime2LocalTime(float time)
	{
		return time;
	}

	protected virtual void OnPlayRaceOver()
	{
	}

	protected virtual void OnRacingTimeOver()
	{
	}

	protected virtual void UpdateState(float tick)
	{
		switch (this.driveState_)
		{
		case GameStageBase.DriveState.READY:
			if (KartManager.Instance.DriveStartTime >= 1f && this.IsUpdateHorn(tick))
			{
				if (this.driveStartHorn_ == 0)
				{
					ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
					changeCameraMessage.Initialize("drive", KartManager.Instance.goPlayKart_);
					MonoBehaviourExCenter.Instance.SendMessage(0, 2, changeCameraMessage);
				}
				else if (this.driveStartHorn_ != 1)
				{
					if (this.driveStartHorn_ != 2)
					{
						if (this.driveStartHorn_ == 3)
						{
							KartManager.Instance.Stuck = false;
							if (this.audioSource_[1] != null)
							{
								this.audioSource_[1].Play();
							}
							this.driveState_ = GameStageBase.DriveState.DRIVING;
							this.gameStageMode_ = GameStageBase.GameStageMode.RACING;
						}
					}
				}
				if (this.driveStartHorn_ <= 2 && this.audioSource_[0] != null)
				{
					this.audioSource_[0].Play();
				}
				this.driveStartHorn_++;
			}
			break;
		case GameStageBase.DriveState.DRIVING:
			this.CheckKartReset();
			this.CheckStuckedReset(tick);
			if (KartManager.Instance.goCourse_.IsKartGoalIn(KartManager.PLAYER_KART_IDX))
			{
				KartManager.Instance.goKart_[KartManager.PLAYER_KART_IDX].Stuck = true;
				this.kartDriveTime_ = tick - KartManager.Instance.DriveStartTime;
				if (this.bgm_ != null)
				{
					this.bgm_.Stop();
				}
				if (KartManager.Instance.parameter_.Stage == StageType.GAME)
				{
					this.isWinner_ = KartManager.Instance.goCourse_.GetMyRank() <= 0;
				}
				if (this.isWinner_)
				{
					this.gameInterface_.PlayAction("winner@action", Time.time);
					if (this.commonBgmSource_ != null)
					{
						AudioClip audioClip = this.commonBgmClip_[0];
						if (KartManager.Instance.parameter_.Stage == StageType.GAME)
						{
							int num = KartOptions.Instance.GetWinCounter(KartManager.Instance.parameter_.track_)[(int)KartManager.Instance.parameter_.gameMode_];
							if (num < 3)
							{
								audioClip = this.commonBgmClip_[3];
							}
						}
						if (audioClip != null)
						{
							this.commonBgmSource_.PlayOneShot(audioClip);
						}
					}
				}
				else
				{
					this.gameInterface_.PlayAction("finish@action", Time.time);
					if (this.commonBgmSource_ != null)
					{
						this.commonBgmSource_.PlayOneShot(this.commonBgmClip_[1]);
					}
				}
				MonoBehaviourExCenter.Instance.SendMessage(0, MonoBehaiourExConst.PLAYER_KART, ((MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION)).Initialize(CharacterAnimation.WIN_GAME, WrapMode.Loop));
				this.boosterGauge_.SetActiveRecursively(false);
				this.gameInterface_.ShowBlackBar(true, true);
				MonoBehaviourExCenter.Instance.SendMessage(0, 2, ((ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL)).Initialize("finish_camera_ani", KartManager.Instance.goPlayKart_, 0.3f, 1800f, 45f, WrapMode.PingPong));
				this.driveState_ = GameStageBase.DriveState.RACE_OVER;
				this.RaceOverSetting();
				Statistics.Instance.RaceComplete(this.isWinner_);
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GOAL_IN);
				monoBehaviourMessage1Param.Initialize(KartManager.PLAYER_KART_IDX);
				MonoBehaviourExCenter.Instance.SendMessage(0, 1, monoBehaviourMessage1Param);
			}
			else if (KartManager.Instance.DriveEndTime <= 0f)
			{
				int lap = KartManager.Instance.goCourse_.GetLap(KartManager.PLAYER_KART_IDX);
				int maxLap = KartManager.Instance.goCourse_.MaxLap;
				if (this.playActionLap_ < lap)
				{
					if (lap == maxLap)
					{
						if (this.audioSource_[2] != null)
						{
							this.audioSource_[2].Play();
						}
						this.gameInterface_.PlayAction("lap_final@action", tick);
					}
					else
					{
						string text = "lap" + lap.ToString() + "@action";
						this.gameInterface_.PlayAction(text, tick);
						if (this.audioSource_[3] != null)
						{
							this.audioSource_[3].Play();
						}
					}
					this.playActionLap_ = lap;
				}
				if (KartManager.Instance.GetPlayTime() > 590f)
				{
					this.OnRacingTimeOver();
				}
			}
			if (this.retireCountTime_ > 0f && this.retireCountTime_ + (float)this.retireCountHorn_ * 1f <= tick)
			{
				this.audioSource_[4].Play();
				this.retireCountTime_ += 1f;
			}
			break;
		case GameStageBase.DriveState.RACE_OVER:
			if (this.raceOverTime_ > 0f && this.raceOverTime_ <= tick)
			{
				if (this.beDoomed_)
				{
					this.driveState_ = GameStageBase.DriveState.RESULT;
					if (KartManager.Instance.result_ != null && KartManager.Instance.result_.IsUserWinner())
					{
						Statistics.Instance.IncreaseWinCount();
					}
					KartOptions.Instance.SaveRegistry();
					RaceResult result_ = KartManager.Instance.result_;
					if (result_ == null)
					{
						StageController.Instance.ChangeStage((KartManager.Instance.parameter_.gameMode_ != GameMode.SINGLE_ITEM) ? StageType.SINGLE_SPEED : StageType.SINGLE_ITEM);
					}
					else if (result_.IsMonthlyBestRecord)
					{
						if (Env.IsConnectedToInternet)
						{
							this.uploadTries_ = 1;
							this.UploadRecord();
							MonoBehaviourExCenter.Instance.SendMessage(0, 18, ((MonoBehaviourMessage2Param<float, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NEW_RECORD)).Initialize(KartManager.Instance.goCourse_.GetFinishTime(), true));
						}
						else
						{
							MonoBehaviourExCenter.Instance.SendMessage(0, 18, ((MonoBehaviourMessage2Param<float, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NEW_RECORD)).Initialize(KartManager.Instance.goCourse_.GetFinishTime(), true));
							iOSEvent.Alert("네트워크에 연결할 수 없습니다. 랭킹을 서버에 올리지 못했습니다.");
							base.Invoke("GoResult", 0.9f);
						}
					}
					else if (result_.IsBestRecord)
					{
						MonoBehaviourExCenter.Instance.SendMessage(0, 18, ((MonoBehaviourMessage2Param<float, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NEW_RECORD)).Initialize(KartManager.Instance.goCourse_.GetFinishTime(), true));
						base.Invoke("GoResult", 0.9f);
					}
					else
					{
						bool flag = false;
						if (KartManager.Instance.parameter_.Stage == StageType.GAME && KartManager.Instance.result_.IsUserWinner())
						{
							int num2 = KartOptions.Instance.GetWinCounter(KartManager.Instance.parameter_.track_)[(int)KartManager.Instance.parameter_.gameMode_];
							if (num2 <= 3)
							{
								MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
								MonoBehaviourExCenter.Instance.SendMessage(0, 266, monoBehaviourMessage2Param.Initialize(AssetType.SIZE, num2 - 1));
								flag = true;
							}
						}
						if (!flag)
						{
							MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(true, false));
						}
					}
				}
				else
				{
					if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI)
					{
						this.gameInterface_.PlayAction("raceover@action", this.raceOverTime_);
						this.raceOverTime_ += 4f;
					}
					this.beDoomed_ = true;
					KartManager.Instance.Stuck = true;
					this.OnPlayRaceOver();
				}
			}
			break;
		}
	}

	protected void UploadRecord()
	{
		Record record = new Record();
		Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
		record.fbid_ = facebook.FBID;
		record.time_ = KartManager.Instance.goCourse_.GetFinishTime();
		record.map_ = (int)KartManager.Instance.parameter_.track_;
		record.gameMode_ = KartManager.Instance.parameter_.gameMode_;
		this.recordUploadStartTime_ = Time.time;
		base.StartCoroutine(new FiaCoroutine(record.Upload(FiaAuth.AuthToken, facebook.FriendDict), new OnSuccess(this.RecordUploadSuccess), new OnFailure(this.RecordUploadFailure)));
	}

	protected void ProcessInterfaceData(float tick)
	{
		if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED && !KartManager.Instance.isPaused_)
		{
			GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
			if (goPlayKart_ == null)
			{
				return;
			}
			float driftMaxGauge = goPlayKart_.GetDriftMaxGauge();
			float driftGaugeProgress = goPlayKart_.GetDriftGaugeProgress();
			float driftGauge = goPlayKart_.GetDriftGauge();
			if (driftGauge == driftMaxGauge)
			{
				goPlayKart_.UseDriftGauge(goPlayKart_.GetDriftMaxGauge());
				this.gameInterface_.AddItemSlotItem(0);
				if (this.boosterGauge_ != null)
				{
					this.boosterGauge_.animation.Play("ipad_booster_gauge_ani");
				}
			}
			else if (this.boosterGauge_ != null)
			{
				this.boosterGauge_.active = true;
				if (!this.boosterGauge_.animation.IsPlaying("ipad_booster_gauge_ani"))
				{
					if (!this.boosterGauge_.animation.IsPlaying("ipad_booster_gauge_default"))
					{
						this.boosterGauge_.animation.Play("ipad_booster_gauge_default");
					}
					else if (this.boosterGaugeBar_ != null)
					{
						Vector3 localScale = this.boosterGaugeBar_.localScale;
						localScale.x = driftGaugeProgress / driftMaxGauge;
						this.boosterGaugeBar_.localScale = localScale;
					}
				}
			}
		}
	}

	protected void CheckStuckedReset(float tick)
	{
		GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
		if (goPlayKart_ == null)
		{
			return;
		}
		if (goPlayKart_.CheckStuck())
		{
			this.ResetKart();
			return;
		}
		if (this.stuckedTime_ == 0f && (goPlayKart_.IsAccel() || goPlayKart_.IsBrake()))
		{
			this.stuckedTime_ = tick;
		}
		if (this.stuckedTime_ != 0f)
		{
			if (!goPlayKart_.Valid || goPlayKart_.Forcing || goPlayKart_.IsInResetState || goPlayKart_.m_KartRealVelocity.sqrMagnitude >= 1f)
			{
				this.stuckedTime_ = 0f;
			}
			if (!goPlayKart_.IsAccel() && !goPlayKart_.IsBrake())
			{
				this.stuckedTime_ = 0f;
			}
		}
		if (this.stuckedTime_ != 0f && this.stuckedTime_ + 2f < tick)
		{
			this.stuckedTime_ = 0f;
			this.ResetKart();
		}
	}

	protected void ResetKart()
	{
		if (this.kartResetState_ == 0 || this.kartResetState_ == 3)
		{
			GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
			if (this.audioSource_[5] != null)
			{
				this.audioSource_[5].Play();
			}
			this.kartResetState_ = 1;
			this.kartResetTick_ = Time.time;
			goPlayKart_.Valid = false;
			goPlayKart_.IsInResetState = true;
		}
	}

	protected void CheckKartReset()
	{
		GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
		if (goPlayKart_ == null)
		{
			return;
		}
		if (goPlayKart_.m_kart.transform.position.y < -5f)
		{
			this.ResetKart();
		}
		if (goPlayKart_.NeedReset)
		{
			goPlayKart_.NeedReset = false;
			this.ResetKart();
		}
		if (KartManager.Instance.goCourse_.IsKartNeedReset(KartManager.PLAYER_KART_IDX))
		{
			this.ResetKart();
		}
	}

	protected virtual void ProcessRaceOver()
	{
	}

	protected virtual void UnloadStage()
	{
		BackgroundNotifier.resign_ = null;
		BackgroundNotifier.active_ = null;
	}

	protected void RaceOverSetting()
	{
		MonoBehaviourMessage message = MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RACE_OVER);
		for (int i = 0; i < 6; i++)
		{
			if (KartManager.Instance.goKart_[i] != null)
			{
				MonoBehaviourExCenter.Instance.SendMessage(0, 24 + i, message);
			}
		}
	}

	public void InitializeItems()
	{
		this.items_ = new ItemBasicController[10][];
		ItemBasicController[] array = new ItemBasicController[10];
		array[1] = this.itemBananaPrefab_;
		array[2] = this.itemUFOPrefab_;
		array[3] = this.itemWaterFlyPrefab_;
		array[4] = this.itemWaterBombPrefab_;
		array[5] = this.itemFlipPrefab_;
		array[6] = this.itemDevilPrefab_;
		array[7] = this.itemWaterMissilePrefab_;
		ItemBasicController[] array2 = array;
		for (int i = 0; i < 10; i++)
		{
			if (this.ITEM_MAX_COUNT[i] == 0)
			{
				this.items_[i] = null;
			}
			else
			{
				this.items_[i] = new ItemBasicController[this.ITEM_MAX_COUNT[i]];
				for (int j = 0; j < this.ITEM_MAX_COUNT[i]; j++)
				{
					this.items_[i][j] = (ItemBasicController)global::UnityEngine.Object.Instantiate(array2[i]);
					this.items_[i][j].gameObject.SetActiveRecursively(false);
				}
			}
		}
	}

	public ItemBasicController GetItem(GameItem item)
	{
		if (this.ITEM_MAX_COUNT[(int)item] == 0)
		{
			return null;
		}
		for (int i = 0; i < this.ITEM_MAX_COUNT[(int)item]; i++)
		{
			if (!this.items_[(int)item][i].gameObject.active)
			{
				return this.items_[(int)item][i];
			}
		}
		return null;
	}

	private void RecordUploadSuccess()
	{
		if (PlayerPrefs.HasKey("REFRESH_RANKING"))
		{
			PlayerPrefs.DeleteKey("REFRESH_RANKING");
			Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
			RankingUpdater rankingUpdater = new RankingUpdater(facebook.FBID, facebook.FriendDict, FiaAuth.AuthToken);
			FiaCoroutine fiaCoroutine = new FiaCoroutine(rankingUpdater.Update(), new OnSuccess(this.RankingUpdateSuccess), new OnFailure(this.RankingUpdateFailure));
			base.StartCoroutine(fiaCoroutine);
		}
		else
		{
			this.InvokeGoResult();
		}
	}

	private void InvokeGoResult()
	{
		float num = Time.time - this.recordUploadStartTime_;
		if (num > 0.9f)
		{
			this.GoResult();
		}
		else
		{
			base.Invoke("GoResult", 0.9f - num);
		}
	}

	private void RankingUpdateSuccess()
	{
		this.InvokeGoResult();
	}

	private void RankingUpdateFailure(Exception ex)
	{
		this.InvokeGoResult();
	}

	public void RecordUploadFailure(Exception e)
	{
		Type type = e.GetType();
		if (this.uploadTries_ < 2 && (type == typeof(RequestException) || type == typeof(ServerException)))
		{
			this.uploadTries_++;
			this.UploadRecord();
		}
		else
		{
			if (type == typeof(RequestException))
			{
				iOSEvent.Alert("네트워크에 연결할 수 없습니다. 랭킹을 서버에 올리지 못했습니다.");
			}
			else if (type == typeof(ServerException))
			{
				iOSEvent.Alert(e.Message);
			}
			else if (type == typeof(FiaAuthException))
			{
				iOSEvent.Alert("페이스북 로그인에 실패하였습니다. 로그아웃을 한 후 다시 로그인 해주세요.");
			}
			else if (type == typeof(InvalidRecordException))
			{
			}
			this.InvokeGoResult();
		}
	}

	private void GoResult()
	{
		MonoBehaviourExCenter.Instance.SendMessage(0, 18, ((MonoBehaviourMessage2Param<float, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.NEW_RECORD)).Initialize(KartManager.Instance.goCourse_.GetFinishTime(), false));
	}

	private void ResignedActive()
	{
	}

	private void BecameActive()
	{
		if (this.driveState_ == GameStageBase.DriveState.LOADING || this.driveState_ == GameStageBase.DriveState.RESULT || this.driveState_ == GameStageBase.DriveState.GO_FINAL_STAGE || this.driveState_ == GameStageBase.DriveState.RESTART)
		{
			return;
		}
		if (this.bgm_ != null && !this.bgm_.IsPaused() && !this.bgm_.isPlaying)
		{
			this.bgm_.Play();
		}
	}

	public const int MAX_START_SECTION = 6;

	public GameInterface gameInterface_;

	protected GameObject boosterGauge_;

	protected Transform boosterGaugeBar_;

	protected GameStageBase.GameStageMode gameStageMode_;

	protected int driveStartHorn_;

	protected int retireCountHorn_;

	public AudioClip hornClip_;

	public AudioClip goClip_;

	public AudioClip finalLapClip_;

	public AudioClip lap2Clip_;

	public AudioClip retireHorn_;

	public AudioClip resetClip_;

	protected AudioSourceEx[] audioSource_ = new AudioSourceEx[6];

	protected AudioSourceEx bgm_;

	public AudioClip[] commonBgmClip_;

	protected AudioSourceEx commonBgmSource_;

	public GameStageBase.DriveState driveState_;

	protected float retireCountTime_;

	protected float kartDriveTime_;

	protected float raceOverTime_;

	protected float recordUploadStartTime_;

	protected bool beDoomed_;

	protected bool isWinner_;

	public GhostController ghostPrefab_;

	public RigidbodyFPSWalker playerPrefab_;

	public GUIMinimapMark minimapMark_;

	public AIController aiPrefab_;

	public NetController netPrefab_;

	private RigidbodyFPSWalker player_;

	public GoItemWaterBomb itemWaterBombPrefab_;

	public GoItemWaterFly itemWaterFlyPrefab_;

	public GoItemBanana itemBananaPrefab_;

	public GoItemUFO itemUFOPrefab_;

	public GoItemWaterMissile itemWaterMissilePrefab_;

	public GoItemFlip itemFlipPrefab_;

	public GoItemDevil itemDevilPrefab_;

	private int playActionLap_ = 1;

	private int uploadTries_;

	private float stuckedTime_;

	private int kartResetState_;

	private float kartResetTick_;

	public ItemBasicController[][] items_;

	private int[] ITEM_MAX_COUNT = new int[] { 0, 16, 8, 8, 8, 8, 8, 8, 0, 0 };

	public enum GameStageMode
	{
		BEFORE_RACING,
		RACING,
		AFTER_RACING
	}

	public enum DriveState
	{
		LOADING,
		READY,
		DRIVING,
		RACE_OVER,
		RESULT,
		GO_FINAL_STAGE,
		RESTART
	}

	protected enum FxInGameStage
	{
		HORN,
		GO,
		FINAL_LAP,
		LAP2,
		RETIRE_HORN,
		RESET,
		MAX_SIZE
	}

	protected enum CommonBgm
	{
		WIN,
		END,
		LOSE,
		VICTORY,
		MAX_SIZE
	}

	public struct AIFileInfo
	{
		public override string ToString()
		{
			return string.Format("{0}{1}{2}{3}", new object[]
			{
				(!this.isStart_) ? "n" : "s",
				this.from_,
				this.to_,
				this.no_.ToString("000")
			});
		}

		public bool isStart_;

		public int no_;

		public int from_;

		public int to_;
	}

	public class AiRacingInfo
	{
		public AiRacingInfo(AIControllerType aiType, float targetTime, bool useStartBoost, MedalType medalType)
		{
			this.type_ = aiType;
			this.targetTime_ = targetTime;
			this.useStartBoost_ = useStartBoost;
			this.medalType_ = medalType;
		}

		public AIControllerType type_;

		public float targetTime_;

		public bool useStartBoost_;

		public MedalType medalType_;
	}
}
