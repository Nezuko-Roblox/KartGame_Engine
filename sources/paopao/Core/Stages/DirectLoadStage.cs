using System;
using System.Collections.Generic;
using UnityEngine;

public class DirectLoadStage : MonoBehaviourStage
{
	private void printLoading()
	{
		Debug.Log(string.Concat(new object[]
		{
			"Loading...\n",
			StageController.Instance.IsWaitForReceiveReadyToChangeMessage,
			"\n",
			ResourceLoader.Instance.IsDone(),
			"\n",
			this.isSendReadyToSceneMessage_,
			"\n",
			StageController.Instance.NextStage == StageType.GAME
		}));
	}

	protected override void Awake()
	{
		Debug.Log("Awake Start");
		base.InvokeRepeating("printLoading", 0f, 1f);
		TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
		KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
		CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
		MaterialManager.Instance.Initialize();
		KartOptions.Instance.LoadRegistry();
		MonoBehaviourMessageFactory.Instance.Initialize();
		Statistics.Instance.Initialize();
		CharacterAssetDefinitionManager.Instance.Refresh();
		KartAssetDefinitionManager.Instance.Refresh();
		TrackAssetDefinitionManager.Instance.Refresh();
		this.RegistMonoBehaviour(513);
		this.kartParameter_[0] = null;
		for (int i = 1; i < 6; i++)
		{
			this.kartParameter_[i] = new KartParameter(0, (byte)i, CharacterAssetDefinitionManager.Instance.GetAssetDefinition(i).Name, PlayerType.AI, string.Empty);
		}
		Time.timeScale = 1f;
		for (int j = 1; j < 6; j++)
		{
			if (KartManager.Instance.parameter_.kart_[j] == null)
			{
				this.kartParameter_[j].type_ = PlayerType.NONE;
			}
			else
			{
				this.kartParameter_[j].type_ = KartManager.Instance.parameter_.kart_[j].type_;
			}
		}
		KartOptions.Instance.Controller = 2;
		iOSController.Instance.Type = (iOSControllerType)KartOptions.Instance.Controller;
		this.LaunchGame(0, 0, 0);
		Debug.Log("Awake End");
	}

	private void Update()
	{
		if (StageController.Instance.IsWaitForReceiveReadyToChangeMessage && ResourceLoader.Instance.IsDone() && !this.isSendReadyToSceneMessage_ && StageController.Instance.NextStage == StageType.GAME)
		{
			base.ReadyToChangeScene();
			this.isSendReadyToSceneMessage_ = true;
			base.CancelInvoke("printLoading");
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SIMPLE_MESSAGE)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param.lparam_ == 0)
			{
				KartManager.Instance.parameter_.track_ = (byte)monoBehaviourMessage2Param.rparam_;
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			if (StageController.Instance.NextStage == StageType.GAME)
			{
				if (!TrackAssetDefinitionManager.Instance.IsValidIndex((int)KartManager.Instance.parameter_.track_))
				{
					TrackAssetDefinitionManager.Instance.GetRandomAsset(out KartManager.Instance.parameter_.track_, true, StaticVariable.LAST_SELECTED_TRACK);
				}
				StaticVariable.LAST_SELECTED_TRACK = KartManager.Instance.parameter_.track_;
				TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_);
				KartManager.Instance.parameter_.maxLap_ = (int)((byte)trackAssetDefinition.MaxLap);
				byte track_ = KartManager.Instance.parameter_.track_;
				GameMode gameMode_ = KartManager.Instance.parameter_.gameMode_;
				GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(track_, (int)gameMode_);
				List<int> list = new List<int>();
				if (bestInfo == null)
				{
					KartAssetDefinitionManager.Instance.GetRandomAsset(1, global::UnityEngine.Random.Range(1, 3), ref list);
					int num = 5 - list.Count;
					for (int i = 0; i < num; i++)
					{
						list.Add(0);
					}
					if (Debug.isDebugBuild)
					{
						Debug.Log("KartBodySettingType : No User Info");
					}
				}
				else
				{
					float[] medalTime = trackAssetDefinition.GetMedalTime(gameMode_);
					if (bestInfo.finishTime_ <= medalTime[0])
					{
						KartAssetDefinitionManager.Instance.GetRandomAsset(4, global::UnityEngine.Random.Range(3, 6), ref list);
						if (list.Count < 5)
						{
							KartAssetDefinitionManager.Instance.GetRandomAsset(3, global::UnityEngine.Random.Range(1, 6 - list.Count), ref list);
						}
						if (list.Count < 5)
						{
							KartAssetDefinitionManager.Instance.GetRandomAsset(2, 5 - list.Count, ref list);
						}
						if (Debug.isDebugBuild)
						{
							Debug.Log("KartBodySettingType : GOLD");
						}
					}
					else if (bestInfo.finishTime_ <= medalTime[1])
					{
						KartAssetDefinitionManager.Instance.GetRandomAsset(4, global::UnityEngine.Random.Range(1, 3), ref list);
						KartAssetDefinitionManager.Instance.GetRandomAsset(3, global::UnityEngine.Random.Range(0, 2), ref list);
						KartAssetDefinitionManager.Instance.GetRandomAsset(2, global::UnityEngine.Random.Range(0, 2), ref list);
						int num2 = 5 - list.Count;
						KartAssetDefinitionManager.Instance.GetRandomAsset(1, num2, ref list);
						if (Debug.isDebugBuild)
						{
							Debug.Log("KartBodySettingType : SILVER");
						}
					}
					else if (bestInfo.finishTime_ <= medalTime[2])
					{
						KartAssetDefinitionManager.Instance.GetRandomAsset(3, global::UnityEngine.Random.Range(0, 2), ref list);
						KartAssetDefinitionManager.Instance.GetRandomAsset(2, global::UnityEngine.Random.Range(0, 2), ref list);
						KartAssetDefinitionManager.Instance.GetRandomAsset(1, global::UnityEngine.Random.Range(0, 4), ref list);
						int num3 = 5 - list.Count;
						for (int j = 0; j < num3; j++)
						{
							list.Add(0);
						}
						if (Debug.isDebugBuild)
						{
							Debug.Log("KartBodySettingType : BRONZE");
						}
					}
					else
					{
						KartAssetDefinitionManager.Instance.GetRandomAsset(1, global::UnityEngine.Random.Range(1, 3), ref list);
						int num4 = 5 - list.Count;
						for (int k = 0; k < num4; k++)
						{
							list.Add(0);
						}
						if (Debug.isDebugBuild)
						{
							Debug.Log("KartBodySettingType : DEFAULT");
						}
					}
				}
				if (Debug.isDebugBuild)
				{
					string text = "KartBodyIndex : ";
					foreach (int num5 in list)
					{
						text += string.Format(" {0} ", num5);
					}
					Debug.Log(text);
				}
				List<int> list2;
				FiaUtil.GetRandomList(0, CharacterAssetDefinitionManager.Instance.GetAssetCount() - 1, out list2);
				list2.Remove((int)KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].character_);
				int num6 = 0;
				for (int l = 0; l < 6; l++)
				{
					if (l != KartManager.PLAYER_KART_IDX)
					{
						byte b = (byte)list[num6];
						byte b2 = (byte)list2[num6];
						num6++;
						KartManager.Instance.parameter_.kart_[l] = new KartParameter(b, b2, CharacterAssetDefinitionManager.Instance.GetAssetDefinition((int)b2).Name, PlayerType.AI, string.Empty);
					}
				}
				List<string> list3 = new List<string>();
				TrackAssetDefinitionManager.Instance.GetAssets((int)KartManager.Instance.parameter_.track_, ref list3);
				for (int m = 0; m < 6; m++)
				{
					KartParameter kartParameter = KartManager.Instance.parameter_.kart_[m];
					if (kartParameter != null)
					{
						KartAssetDefinitionManager.Instance.GetAssets((int)kartParameter.body_, ref list3);
						CharacterAssetDefinitionManager.Instance.GetAssets((int)kartParameter.character_, ref list3);
					}
				}
				foreach (string text2 in list3)
				{
					ResourceLoader.Instance.RequestAssetBundle(text2);
				}
				if (Debug.isDebugBuild)
				{
					string text3 = string.Empty;
					foreach (string text4 in list3)
					{
						text3 = text3 + text4 + "\n";
					}
					Debug.Log("### LOAD RESOURCE ###\n" + text3);
				}
				this.isSendReadyToSceneMessage_ = false;
			}
			else
			{
				base.ReadyToChangeScene();
			}
		}
	}

	public void LaunchGame(string setting)
	{
		JSONObject jsonobject = new JSONObject(setting);
		byte b = (byte)((JSONObject)jsonobject.list[0]).n;
		byte b2 = (byte)((JSONObject)jsonobject.list[1]).n;
		byte b3 = (byte)((JSONObject)jsonobject.list[2]).n;
		this.LaunchGame(b, b2, b3);
	}

	public void LaunchGame()
	{
		this.LaunchGame((byte)KartOptions.Instance.SelectSpeedTrack, (byte)KartOptions.Instance.Kart, (byte)KartOptions.Instance.Character);
	}

	public void LaunchGame(byte track, byte kart, byte unit)
	{
		KartManager.Instance.parameter_.track_ = track;
		string text = "sublee";
		KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX] = new KartParameter(kart, unit, text, PlayerType.PLAYER, string.Empty);
		KartOptions.Instance.DefaultUserName = "sublee";
		KartOptions.Instance.SaveRegistry();
		KartOptions.Instance.LoadRegistry();
		StageController.Instance.BeginStage();
		KartManager.Instance.parameter_.gameMode_ = GameMode.SINGLE_ITEM;
		StageController.Instance.ChangeStage(StageType.GAME);
	}

	protected KartParameter[] kartParameter_ = new KartParameter[6];

	private bool isSendReadyToSceneMessage_;
}
