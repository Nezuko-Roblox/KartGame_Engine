using System;
using System.Collections.Generic;
using UnityEngine;

public class WaitRoomStage : BaseWaitRoomStage
{
	protected override void Awake()
	{
		base.Awake();
		this.RegistMonoBehaviour(515);
		NetworkManager.Inst.Stage = this;
		PlayerPacket playerPacket = base.CreatePlayerPacket();
		playerPacket.ready_ = false;
		GameParamPacket gameParam_ = LastReceivedPacket.gameParam_;
		if (gameParam_ != null && gameParam_.startRace_)
		{
			foreach (PlayerPacket playerPacket2 in gameParam_.players_)
			{
				playerPacket2.ready_ = false;
			}
			gameParam_.startRace_ = false;
		}
		if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
		{
			this.peers_ = new Dictionary<string, Peer>();
			this.server_ = NetworkManager.Inst.Session.Self;
			base.RefreshPeerList();
			if (gameParam_ != null && gameParam_.players_ != null)
			{
				foreach (PlayerPacket playerPacket3 in gameParam_.players_)
				{
					try
					{
						this.peers_[playerPacket3.id_].packet_ = playerPacket3;
					}
					catch (ArgumentNullException ex)
					{
						if (Debug.isDebugBuild)
						{
							Debug.Log(ex);
						}
					}
					catch (KeyNotFoundException ex2)
					{
						if (Debug.isDebugBuild)
						{
							Debug.Log(ex2);
						}
					}
				}
			}
			NetworkManager.Inst.Session.AcceptConnections();
			FiaUtil.GetRandomList(0, 5, out this.startPositions_);
			this.ProcessPlayerPacket(playerPacket);
		}
		else
		{
			NetworkManager.Inst.SendPacketToServer(playerPacket, SendDataMode.RELIABLE);
		}
	}

	protected override void SetReady(bool _ready)
	{
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.WAITING_PLAYERS_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 268, monoBehaviourMessage1Param.Initialize((!_ready) ? 1 : 0));
		this.ready_ = _ready;
	}

	protected override void Ready()
	{
		this.SetReady(!this.ready_);
		PlayerPacket playerPacket = base.CreatePlayerPacket();
		playerPacket.ready_ = this.ready_;
		if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
		{
			this.ProcessPlayerPacket(playerPacket);
		}
		else
		{
			NetworkManager.Inst.SendPacketToServer(playerPacket, SendDataMode.RELIABLE);
		}
	}

	protected override void UpdateStatus(GameParamPacket packet)
	{
		MonoBehaviourMessage1Param<GameParamPacket> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<GameParamPacket>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_WAITROOM);
		if (monoBehaviourMessage1Param != null)
		{
			string connectedServer = NetworkManager.Inst.Session.ConnectedServer;
			string id = NetworkManager.Inst.Session.ID;
			if (connectedServer == id || id == "mock id")
			{
				base.SendMessage(1032, monoBehaviourMessage1Param.Initialize(packet));
			}
			else
			{
				base.SendMessage(1034, monoBehaviourMessage1Param.Initialize(packet));
			}
		}
	}

	private void ProcessPlayerPacket(PlayerPacket packet)
	{
		base.RefreshPeerList();
		if (this.peers_.ContainsKey(packet.id_))
		{
			this.peers_[packet.id_].packet_ = packet;
		}
		else if (Debug.isDebugBuild)
		{
			Debug.Log("WARNING: could not find peer for packet " + packet);
		}
		GameParamPacket gameParamPacket = base.CreateGameParamPacket();
		NetworkManager.Inst.SendPacketToAll(gameParamPacket, SendDataMode.RELIABLE);
		this.ProcessPacket(gameParamPacket, NetworkManager.Inst.Session.ID);
	}

	protected override void LoadResources(GameParamPacket param)
	{
		NetworkManager.Inst.Session.RejectConnections();
		KartManager.Instance.parameter_.track_ = param.availableTrack_;
		if (Debug.isDebugBuild)
		{
			Debug.Log("GameReadyStage:LoadResources / param = " + param);
		}
		for (int i = 0; i < 6; i++)
		{
			KartManager.Instance.parameter_.kart_[i] = null;
		}
		foreach (PlayerPacket playerPacket in param.players_)
		{
			Debug.Log("GameReadyStage:LoadResources / player = " + playerPacket);
			PlayerType playerType = ((!(playerPacket.id_ == NetworkManager.Inst.Session.ID)) ? PlayerType.NET : PlayerType.PLAYER);
			Debug.Log("GameReadyStage:LoadResources / pType = " + playerType);
			string text = string.Empty;
			if (playerType == PlayerType.NET)
			{
				text = string.Empty;
			}
			if (playerType == PlayerType.PLAYER)
			{
				KartManager.PLAYER_KART_IDX = playerPacket.slot_;
			}
			KartManager.Instance.parameter_.kart_[playerPacket.slot_] = new KartParameter(playerPacket.kartName_, playerPacket.characterName_, playerPacket.name_, playerType, text, playerPacket.id_);
		}
		KartManager.Instance.parameter_.gameMode_ = param.gameMode_;
		KartManager.Instance.parameter_.driveOption_ = DriveOption.DEVICE;
		KartManager.Instance.parameter_.startPositions_ = param.startPositions_;
		bool flag = true;
		if (flag)
		{
			TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_);
			KartManager.Instance.parameter_.maxLap_ = (int)((byte)trackAssetDefinition.MaxLap);
		}
		List<string> list = new List<string>();
		TrackAssetDefinitionManager.Instance.GetAssets((int)KartManager.Instance.parameter_.track_, ref list);
		for (int j = 0; j < 6; j++)
		{
			KartParameter kartParameter = KartManager.Instance.parameter_.kart_[j];
			if (kartParameter != null)
			{
				KartAssetDefinitionManager.Instance.GetAssets((int)kartParameter.body_, ref list);
				CharacterAssetDefinitionManager.Instance.GetAssets((int)kartParameter.character_, ref list);
			}
		}
		string text2 = string.Empty;
		foreach (string text3 in list)
		{
			ResourceLoader.Instance.RequestAssetBundle(text3);
			text2 = text2 + text3 + "\n";
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log("### LOAD RESOURCE ###\n" + text2);
		}
		this.isSendReadyToSceneMessage_ = false;
	}

	public override void ConnectionFailed(string serverID)
	{
	}

	protected void Update()
	{
		if (StageController.Instance.IsWaitForReceiveReadyToChangeMessage && ResourceLoader.Instance.IsDone() && !this.isSendReadyToSceneMessage_)
		{
			base.ReadyToChangeScene();
			this.isSendReadyToSceneMessage_ = true;
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
		{
			MonoBehaviourMessage1Param<StageType> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<StageType>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				NetworkManager.Inst.Stage = null;
				if (monoBehaviourMessage1Param.param_ == StageType.WIFI)
				{
					NetworkManager.Inst.Session.Close();
					base.ReadyToChangeScene();
				}
				else if (monoBehaviourMessage1Param.param_ == StageType.GARAGE)
				{
					base.ReadyToChangeScene();
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.SIMPLE_MESSAGE)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				if (monoBehaviourMessage2Param.lparam_ == 1)
				{
					this.Ready();
				}
				else if (monoBehaviourMessage2Param.lparam_ == 4)
				{
					this.UpdateStatus(LastReceivedPacket.gameParam_);
				}
				else
				{
					Debug.Log("WaitRoomStage:ReceiveMessage / THE UNKNOWN MSG IS " + msg);
					if (monoBehaviourMessage2Param.lparam_ == 3)
					{
						AssetSelection.selection_[2] = monoBehaviourMessage2Param.rparam_;
					}
					else if (monoBehaviourMessage2Param.lparam_ == 2)
					{
						KartManager.Instance.parameter_.gameMode_ = (GameMode)monoBehaviourMessage2Param.rparam_;
					}
					GameParamPacket gameParamPacket = base.CreateGameParamPacket();
					NetworkManager.Inst.SendPacketToAll(gameParamPacket, SendDataMode.RELIABLE);
					this.ProcessPacket(gameParamPacket, NetworkManager.Inst.Session.ID);
				}
			}
		}
	}

	private const int PEER_LIST_UPDATE_RATE = 60;

	private bool isSendReadyToSceneMessage_;
}
