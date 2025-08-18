using System;
using System.Collections.Generic;
using UnityEngine;

public class NetGameStage : GameStageBase, NetStage
{
	protected override void Start()
	{
		NetworkManager.Inst.Session.ClearGameKartPacket();
		base.Start();
		if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
		{
			this.slots_ = new Dictionary<string, Slot>();
			for (int i = 0; i < KartManager.Instance.parameter_.kart_.Length; i++)
			{
				KartParameter kartParameter = KartManager.Instance.parameter_.kart_[i];
				if (kartParameter != null)
				{
					Slot slot = new Slot();
					slot.id_ = kartParameter.id_;
					slot.index_ = i;
					slot.name_ = kartParameter.name_;
					slot.state_ = Slot.State.LOADING;
					this.slots_.Add(slot.id_, slot);
				}
			}
			this.sync_ = new ServerTimeSync();
		}
		else
		{
			this.sync_ = new TimeSync();
		}
		NetworkManager.Inst.sync_ = this.sync_;
		NetworkManager.Inst.Stage = this;
		switch (NetworkManager.Inst.Session.Peers().Length)
		{
		case 1:
			this.FAST_SYNC_RATE = 4;
			break;
		case 2:
			this.FAST_SYNC_RATE = 4;
			break;
		case 3:
			this.FAST_SYNC_RATE = 6;
			break;
		}
	}

	protected override void ProcessRaceOver()
	{
		if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER && !this.sentGameFinishedPacket_ && this.raceFinishTime_ != 0f && (this.raceFinishTime_ + 10f < Time.time || this.AreSlotsInState(Slot.State.GOAL_IN)))
		{
			List<Slot> list = new List<Slot>(this.slots_.Values);
			if (this.removedSlots_ != null)
			{
				foreach (Slot slot in this.removedSlots_)
				{
					slot.raceEndTime_ = 0f;
					list.Add(slot);
				}
			}
			GameResultPacket gameResultPacket = new GameResultPacket(list);
			this.ProcessPacket(gameResultPacket, null);
			NetworkManager.Inst.SendPacketToAll(gameResultPacket, SendDataMode.RELIABLE);
			this.sentGameFinishedPacket_ = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.sync_.Sync();
	}

	private bool AreSlotsInState(Slot.State state)
	{
		bool flag = true;
		foreach (Slot slot in this.slots_.Values)
		{
			if (slot.state_ != state)
			{
				flag = false;
				break;
			}
		}
		return flag;
	}

	protected override void UpdateState(float tick)
	{
		switch (this.driveState_)
		{
		case GameStageBase.DriveState.LOADING:
			if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER || this.sync_.State == TimeSyncState.SYNCED)
			{
				this.driveState_ = GameStageBase.DriveState.READY;
				KartManager.Instance.DriveStartTime = -1f;
				GameControlPacket gameControlPacket = new GameControlPacket(GameControlPacket.Control.LOADING_DONE);
				if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
				{
					this.ProcessPacket(gameControlPacket, NetworkManager.Inst.Session.ID);
					NetworkManager.Inst.SendPacketToAll(gameControlPacket, SendDataMode.RELIABLE);
				}
				else
				{
					NetworkManager.Inst.SendPacketToServer(gameControlPacket, SendDataMode.RELIABLE);
				}
			}
			break;
		case GameStageBase.DriveState.READY:
			if (this.frameCount_ % this.SLOW_SYNC_RATE == 0)
			{
				this.SendGameKartPacket(tick);
				this.PollGameKartPacket(tick);
			}
			break;
		case GameStageBase.DriveState.DRIVING:
			if (KartManager.Instance.goCourse_.IsKartGoalIn(KartManager.PLAYER_KART_IDX))
			{
				if (!this.didSendFinishReport)
				{
					if (KartManager.Instance.DriveEndTime <= 0f)
					{
						this.isWinner_ = true;
					}
					GameControlPacket gameControlPacket2 = new GameControlPacket(GameControlPacket.Control.FINISH_REPORT, this.sync_.MakeLocalT2ServerT(tick));
					if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER)
					{
						this.ProcessPacket(gameControlPacket2, NetworkManager.Inst.Session.ID);
					}
					else
					{
						NetworkManager.Inst.SendPacketToServer(gameControlPacket2, SendDataMode.RELIABLE);
					}
					this.didSendFinishReport = true;
					this.SendGameKartPacket(tick);
				}
				else if (this.frameCount_ % this.SLOW_SYNC_RATE == 0)
				{
					this.SendGameKartPacket(tick);
					this.PollGameKartPacket(tick);
				}
			}
			else if (this.frameCount_ % this.FAST_SYNC_RATE == 0)
			{
				this.SendGameKartPacket(tick);
			}
			else
			{
				this.PollGameKartPacket(tick);
			}
			break;
		case GameStageBase.DriveState.RACE_OVER:
			if (this.frameCount_ % this.SLOW_SYNC_RATE == 0)
			{
				this.SendGameKartPacket(tick);
				this.PollGameKartPacket(tick);
			}
			break;
		}
		base.UpdateState(tick);
		this.frameCount_++;
	}

	private void SendGameKartPacket(float tick)
	{
		GoPlayKart goPlayKart = (GoPlayKart)KartManager.Instance.goKart_[KartManager.PLAYER_KART_IDX];
		GameKartPacket gameKartPacket = new GameKartPacket();
		gameKartPacket.position_ = goPlayKart.m_kart.transform.position;
		gameKartPacket.velocity_ = goPlayKart.m_KartRealVelocity;
		gameKartPacket.rotation_ = goPlayKart.m_kart.transform.rotation;
		gameKartPacket.tick_ = this.sync_.MakeLocalT2ServerT(tick);
		gameKartPacket.slot_ = KartManager.PLAYER_KART_IDX;
		gameKartPacket.characterAnimation_ = goPlayKart.CharacterAnim;
		gameKartPacket.kartBodyAnimation_ = goPlayKart.KartBodyAnim;
		gameKartPacket.rankValue_ = KartManager.Instance.goCourse_.GetInternalRankValue(KartManager.PLAYER_KART_IDX);
		NetworkManager.Inst.SendPacketToAll(gameKartPacket, SendDataMode.UNRELIABLE);
	}

	protected void PollGameKartPacket(float tick)
	{
		NetworkManager.Inst.Session.PollGameKartPacket();
	}

	public bool ProcessPacket(Packet packet, string senderID)
	{
		try
		{
			Type type = packet.GetType();
			if (type == typeof(GameKartPacket))
			{
				GameKartPacket gameKartPacket = (GameKartPacket)packet;
				if (KartManager.Instance.goKart_[gameKartPacket.slot_] != null)
				{
					((GoNetKart)KartManager.Instance.goKart_[gameKartPacket.slot_]).SetPacket(gameKartPacket);
				}
				return true;
			}
			if (type == typeof(TimeSyncPacket))
			{
				this.sync_.ProcessPacket((TimeSyncPacket)packet, senderID);
				return true;
			}
			if (type == typeof(ItemPacket))
			{
				this.ProcessItemMessage(((ItemPacket)packet).ToItemMessage());
				return true;
			}
			if (type == typeof(ItemSuccessPacket))
			{
				MonoBehaviourMessage2Param<CharacterAnimation, WrapMode> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION);
				monoBehaviourMessage2Param.Initialize(CharacterAnimation.ATTACK_SUCCESS, WrapMode.Default);
				base.SendMessage(MonoBehaiourExConst.PLAYER_KART, monoBehaviourMessage2Param);
				return true;
			}
			if (type == typeof(ShieldEffectPacket))
			{
				ShieldEffectPacket shieldEffectPacket = (ShieldEffectPacket)packet;
				try
				{
					KartManager.Instance.goKart_[shieldEffectPacket.slot_].controller_.UseItem(GameItem.SHIELD);
				}
				catch (Exception ex)
				{
				}
				return true;
			}
			if (type == typeof(GuardEffectPacket))
			{
				GuardEffectPacket guardEffectPacket = (GuardEffectPacket)packet;
				try
				{
					KartManager.Instance.goKart_[guardEffectPacket.slot_].controller_.UseItem(GameItem.GUARD);
				}
				catch (Exception ex2)
				{
				}
				return true;
			}
			if (type == typeof(DestroyBananaPacket))
			{
				DestroyBananaPacket destroyBananaPacket = (DestroyBananaPacket)packet;
				foreach (ItemBasicController itemBasicController in this.items_[1])
				{
					GoItemBanana goItemBanana = (GoItemBanana)itemBasicController;
					if ((float)goItemBanana.id_ == destroyBananaPacket.id_)
					{
						goItemBanana.state_ = GoItemBanana.State.DESTROY;
						return true;
					}
				}
			}
			else
			{
				if (type == typeof(GameControlPacket))
				{
					GameControlPacket gameControlPacket = (GameControlPacket)packet;
					switch (gameControlPacket.control_)
					{
					case GameControlPacket.Control.LOADING_DONE:
						if (NetworkManager.Inst.Session.Mode == SessionMode.CLIENT)
						{
							this.sync_.State = TimeSyncState.READY_TO_SYNC;
						}
						else
						{
							if (!this.slots_.ContainsKey(senderID))
							{
								throw new ArgumentException("this is an invalid id " + senderID);
							}
							this.slots_[senderID].state_ = Slot.State.LOADING_DONE;
							this.CheckAndSendRacingStartPacket();
						}
						break;
					case GameControlPacket.Control.RACING_START:
						if (this.driveState_ != GameStageBase.DriveState.READY)
						{
							throw new ArgumentException("driveState_ should be READY but instead is " + this.driveState_);
						}
						KartManager.Instance.DriveStartTime = this.sync_.MakeServerT2LocalT(gameControlPacket.time_);
						this.gameInterface_.PlayAction("start123@action", KartManager.Instance.DriveStartTime - 3f);
						break;
					case GameControlPacket.Control.FINISH_REPORT:
						if (this.slots_.ContainsKey(senderID))
						{
							this.slots_[senderID].raceEndTime_ = gameControlPacket.time_ - KartManager.Instance.DriveStartTime;
							this.slots_[senderID].state_ = Slot.State.GOAL_IN;
						}
						if (this.raceFinishTime_ <= 0f)
						{
							this.raceFinishTime_ = gameControlPacket.time_;
							GameControlPacket gameControlPacket2 = new GameControlPacket(GameControlPacket.Control.FINISH_NOTICE, gameControlPacket.time_ + 10f);
							this.ProcessPacket(gameControlPacket2, null);
							NetworkManager.Inst.SendPacketToAll(gameControlPacket2, SendDataMode.RELIABLE);
						}
						else if (this.raceFinishTime_ > gameControlPacket.time_)
						{
							this.raceFinishTime_ = gameControlPacket.time_;
						}
						break;
					case GameControlPacket.Control.FINISH_NOTICE:
						KartManager.Instance.DriveEndTime = this.sync_.MakeServerT2LocalT(gameControlPacket.time_);
						if (this.driveState_ == GameStageBase.DriveState.DRIVING)
						{
							this.retireCountHorn_ = 0;
							this.retireCountTime_ = KartManager.Instance.DriveEndTime - 10f;
							this.gameInterface_.PlayAction("finish_count@action", this.retireCountTime_);
						}
						break;
					default:
						throw new ArgumentException("GameControlPacket cannot have state " + gameControlPacket.control_);
					}
					return true;
				}
				if (type == typeof(GameResultPacket))
				{
					KartManager.Instance.result_ = ((GameResultPacket)packet).ToRaceResult();
					this.raceOverTime_ = Time.time + 3f;
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
						MonoBehaviourMessage2Param<CharacterAnimation, WrapMode> monoBehaviourMessage2Param2 = (MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION);
						monoBehaviourMessage2Param2.Initialize(CharacterAnimation.LOSE_GAME, WrapMode.Loop);
						MonoBehaviourExCenter.Instance.SendMessage(0, MonoBehaiourExConst.PLAYER_KART, monoBehaviourMessage2Param2);
						this.gameInterface_.PlayAction("retire@action", Time.time);
						this.driveState_ = GameStageBase.DriveState.RACE_OVER;
						KartManager.Instance.goKart_[KartManager.PLAYER_KART_IDX].Stuck = true;
						base.RaceOverSetting();
					}
					base.Invoke("BackToReadyStage", 17f);
					return true;
				}
				if (type == typeof(UserLeaveNoticePacket))
				{
					iOSEvent.UnlockUserInteraction();
					UserLeaveNoticePacket userLeaveNoticePacket = (UserLeaveNoticePacket)packet;
					if (userLeaveNoticePacket.id_ == NetworkManager.Inst.Session.ID)
					{
						iOSEvent.Alert("게임이 백그라운드로 들어가면 접속이 끊깁니다.");
						this.BackToLobbyStage();
					}
					else
					{
						if (NetworkManager.Inst.Session.Mode == SessionMode.SERVER && this.slots_.ContainsKey(userLeaveNoticePacket.id_))
						{
							this.removedSlots_.Add(this.slots_[userLeaveNoticePacket.id_]);
							this.slots_.Remove(userLeaveNoticePacket.id_);
							if (this.driveState_ == GameStageBase.DriveState.READY)
							{
								this.CheckAndSendRacingStartPacket();
							}
						}
						if (NetworkManager.Inst.Session.Peers().Length == 0)
						{
							iOSEvent.Alert("다른 플레이어들의 네트워크 연결이 끊겼습니다.");
							this.BackToLobbyStage();
						}
					}
					return true;
				}
			}
		}
		catch (Exception ex3)
		{
		}
		return false;
	}

	public void BackToReadyStage()
	{
		this.UnloadStage();
		string connectedServer = NetworkManager.Inst.Session.ConnectedServer;
		string id = NetworkManager.Inst.Session.ID;
		if (connectedServer == id)
		{
			StageController.Instance.ChangeStage(StageType.WAITROOM_HOST);
		}
		else
		{
			StageController.Instance.ChangeStage(StageType.WAITROOM_CLIENT);
		}
		NetworkManager.Inst.Stage = null;
	}

	public void BackToLobbyStage()
	{
		this.UnloadStage();
		NetworkManager.Inst.Session.Close();
		StageController.Instance.ChangeStage(StageType.WIFI);
		NetworkManager.Inst.Stage = null;
	}

	public void ConnectionFailed(string serverID)
	{
		throw new InvalidOperationException("We don't make connections in gamestage");
	}

	public void CheckAndSendRacingStartPacket()
	{
		if (this.AreSlotsInState(Slot.State.LOADING_DONE))
		{
			GameControlPacket gameControlPacket = new GameControlPacket(GameControlPacket.Control.RACING_START, Time.time + 10f);
			this.ProcessPacket(gameControlPacket, null);
			NetworkManager.Inst.SendPacketToAll(gameControlPacket, SendDataMode.RELIABLE);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.ITEM)
		{
			MonoBehaviourMessage2Param<GameItem, ItemParam> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<GameItem, ItemParam>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				NetworkManager.Inst.SendPacketToAll(new ItemPacket(monoBehaviourMessage2Param.lparam_, monoBehaviourMessage2Param.rparam_), SendDataMode.RELIABLE);
				ItemBasicController item = base.GetItem(monoBehaviourMessage2Param.lparam_);
				if (item != null)
				{
					item.Initialize(monoBehaviourMessage2Param.rparam_);
				}
			}
		}
	}

	private void ProcessItemMessage(MonoBehaviourMessage2Param<GameItem, ItemParam> derivedMsg)
	{
		if (derivedMsg != null)
		{
			ItemBasicController item = base.GetItem(derivedMsg.lparam_);
			if (item != null)
			{
				item.Initialize(derivedMsg.rparam_);
			}
		}
	}

	private float raceFinishTime_;

	private Dictionary<string, Slot> slots_;

	private bool sentGameFinishedPacket_;

	private TimeSync sync_;

	private bool didSendFinishReport;

	private int FAST_SYNC_RATE = 4;

	private int SLOW_SYNC_RATE = 6;

	private List<Slot> removedSlots_ = new List<Slot>();

	private int frameCount_;
}
