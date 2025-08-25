using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWaitRoomStage : MonoBehaviourStage, NetStage
{
	public virtual void ConnectionFailed(string serverID)
	{
	}

	protected override void Awake()
	{
		base.Awake();
		if (NetworkManager.Inst.Session != null && NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
		{
			this.peers_ = new Dictionary<string, Peer>();
			this.server_ = NetworkManager.Inst.Session.Self;
			FiaUtil.GetRandomList(0, 5, out this.startPositions_);
			this.RefreshPeerList();
			GameParamPacket gameParam_ = LastReceivedPacket.gameParam_;
			if (gameParam_ != null && gameParam_.players_ != null)
			{
				foreach (PlayerPacket playerPacket in gameParam_.players_)
				{
					try
					{
						this.peers_[playerPacket.id_].packet_ = playerPacket;
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
			else
			{
				KartManager.Instance.parameter_.gameMode_ = GameMode.SINGLE_ITEM;
			}
			base.InvokeRepeating("RefreshPeerList", 1f, 3f);
		}
		NetworkManager.Inst.Stage = this;
	}

	protected PlayerPacket CreatePlayerPacket()
	{
		return new PlayerPacket
		{
			id_ = NetworkManager.Inst.Session.ID,
			name_ = NetworkManager.Inst.Session.Name,
			characterName_ = (byte)KartOptions.Instance.Character,
			kartName_ = (byte)KartOptions.Instance.Kart
		};
	}

	protected GameParamPacket CreateGameParamPacket()
	{
		this.RefreshPeerList();
		GameParamPacket gameParamPacket = new GameParamPacket();
		gameParamPacket.selectedTrack_ = (sbyte)AssetSelection.selection_[2];
		if (TrackAssetDefinitionManager.Instance.IsRandomTrackIndex((int)gameParamPacket.selectedTrack_))
		{
			TrackAssetDefinitionManager.Instance.GetRandomAsset(out gameParamPacket.availableTrack_, true);
		}
		else
		{
			gameParamPacket.availableTrack_ = (byte)gameParamPacket.selectedTrack_;
		}
		gameParamPacket.players_ = new List<PlayerPacket>();
		int num = 0;
		List<string> list = new List<string>(this.peers_.Keys);
		list.Sort();
		foreach (string text in list)
		{
			if (this.peers_[text].packet_ != null)
			{
				this.peers_[text].packet_.slot_ = num;
				gameParamPacket.players_.Add(this.peers_[text].packet_);
				num++;
			}
		}
		gameParamPacket.gameMode_ = KartManager.Instance.parameter_.gameMode_;
		if (this.peers_.Count == 1)
		{
			gameParamPacket.startRace_ = false;
			this.SetReady(false);
			foreach (Peer peer in this.peers_.Values)
			{
				if (peer.packet_ != null)
				{
					peer.packet_.ready_ = false;
				}
			}
		}
		else
		{
			gameParamPacket.startRace_ = true;
			foreach (Peer peer2 in this.peers_.Values)
			{
				if (peer2.packet_ == null || !peer2.packet_.ready_)
				{
					gameParamPacket.startRace_ = false;
					break;
				}
			}
		}
		if (this.startPositions_ == null)
		{
			FiaUtil.GetRandomList(0, 5, out this.startPositions_);
		}
		gameParamPacket.startPositions_ = this.startPositions_;
		return gameParamPacket;
	}

	protected void RefreshPeerList()
	{
		List<string> list = new List<string>();
		foreach (Peer peer in NetworkManager.Inst.Session.Peers())
		{
			if (!this.peers_.ContainsKey(peer.id_))
			{
				this.peers_[peer.id_] = peer;
				NetworkManager.Inst.SendPacket(new AppVersionPacket(KartOptions.Instance.ProgramVersion, KartOptions.Instance.MultiplayerVersion), peer.id_, SendDataMode.RELIABLE);
			}
			list.Add(peer.id_);
		}
		this.peers_[this.server_.id_] = this.server_;
		list.Add(this.server_.id_);
		List<string> list2 = new List<string>();
		foreach (string text in this.peers_.Keys)
		{
			if (!list.Contains(text))
			{
				list2.Add(text);
			}
		}
		foreach (string text2 in list2)
		{
			this.peers_.Remove(text2);
		}
	}

	private void ProcessPlayerPacket(PlayerPacket packet)
	{
		this.RefreshPeerList();
		if (this.peers_.ContainsKey(packet.id_))
		{
			this.peers_[packet.id_].packet_ = packet;
		}
		GameParamPacket gameParamPacket = this.CreateGameParamPacket();
		NetworkManager.Inst.SendPacketToAll(gameParamPacket, SendDataMode.RELIABLE);
		this.ProcessPacket(gameParamPacket, NetworkManager.Inst.Session.ID);
	}

	public bool ProcessPacket(Packet packet, string senderID)
	{
		if (packet.GetType() == typeof(PlayerPacket))
		{
			this.ProcessPlayerPacket((PlayerPacket)packet);
			return true;
		}
		if (packet.GetType() == typeof(GameParamPacket))
		{
			LastReceivedPacket.gameParam_ = (GameParamPacket)packet;
			this.gameParamUpdated_ = true;
			this.UpdateStatus(LastReceivedPacket.gameParam_);
			if (LastReceivedPacket.gameParam_.startRace_)
			{
				NetworkManager.Inst.Stage = null;
				this.LoadResources(LastReceivedPacket.gameParam_);
				StageController.Instance.ChangeStage(StageType.GAME_WIFI);
			}
			return true;
		}
		if (packet.GetType() == typeof(UserLeaveNoticePacket))
		{
			UserLeaveNoticePacket userLeaveNoticePacket = (UserLeaveNoticePacket)packet;
			if (userLeaveNoticePacket.id_ == NetworkManager.Inst.Session.ID)
			{
				iOSEvent.Alert("게임이 백그라운드로 들어가면 접속이 끊깁니다.");
				NetworkManager.Inst.Session.Close();
				StageController.Instance.ChangeStage(StageType.WIFI);
			}
			else if (NetworkManager.Inst.Session.Mode == SessionMode.CLIENT && NetworkManager.Inst.Session.Peers().Length == 0)
			{
				iOSEvent.Alert("네트워크 연결이 끊겼습니다.");
				NetworkManager.Inst.Session.Close();
				StageController.Instance.ChangeStage(StageType.WIFI);
			}
			else if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
			{
				GameParamPacket gameParamPacket = this.CreateGameParamPacket();
				NetworkManager.Inst.SendPacketToAll(gameParamPacket, SendDataMode.RELIABLE);
				this.ProcessPacket(gameParamPacket, NetworkManager.Inst.Session.ID);
			}
			return true;
		}
		return false;
	}

	protected abstract void SetReady(bool _ready);

	protected abstract void Ready();

	protected abstract void LoadResources(GameParamPacket p);

	protected abstract void UpdateStatus(GameParamPacket p);

	protected Dictionary<string, Peer> peers_;

	protected Peer server_;

	protected List<int> startPositions_;

	public bool ready_;

	protected bool gameParamUpdated_;
}
