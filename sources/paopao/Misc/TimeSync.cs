using System;
using UnityEngine;

public class TimeSync
{
	public TimeSync()
	{
		this.timeSyncRate_ = 0.2f;
	}

	public virtual void Sync()
	{
		if (this.lastTimeSync_ < Time.time - this.timeSyncRate_)
		{
			switch (this.state_)
			{
			case TimeSyncState.WAITING_FOR_SERVER:
				this.lastTimeSync_ = Time.time;
				return;
			case TimeSyncState.READY_TO_SYNC:
				this.state_ = TimeSyncState.SYNCING;
				this.syncCount_ = 0;
				break;
			case TimeSyncState.SYNCING:
				if (this.syncCount_ > 10)
				{
					this.state_ = TimeSyncState.SYNCED;
					this.timeSyncRate_ = 1000f;
				}
				break;
			}
			this.syncCount_++;
			this.SendPacket();
			this.lastTimeSync_ = Time.time;
		}
	}

	public void Reset()
	{
		this.state_ = TimeSyncState.WAITING_FOR_SERVER;
		this.offset_ = 0f;
		this.syncCount_ = 0;
	}

	public float MakeServerT2LocalT(float tick)
	{
		return tick - this.offset_;
	}

	public float MakeLocalT2ServerT(float tick)
	{
		return tick + this.offset_;
	}

	public void ProcessPacket(TimeSyncPacket packet, string senderID)
	{
		if (packet.catchTick_ == 0f)
		{
			packet.catchTick_ = Time.time;
			NetworkManager.Inst.SendPacket(packet, senderID, SendDataMode.UNRELIABLE);
		}
		else
		{
			float num = packet.recvTick_ - packet.throwTick_;
			if (this.offset_ == 0f)
			{
				this.offset_ = packet.catchTick_ - packet.throwTick_ - num / 2f;
			}
			else if (num < 0.2f)
			{
				if (num < 0.05f)
				{
					this.offset_ = packet.catchTick_ - packet.throwTick_ - num / 2f;
				}
				else
				{
					this.offset_ += packet.catchTick_ - packet.throwTick_ - num / 2f;
					this.offset_ /= 2f;
				}
			}
		}
	}

	private void SendPacket()
	{
		TimeSyncPacket timeSyncPacket = new TimeSyncPacket();
		timeSyncPacket.throwTick_ = Time.time;
		NetworkManager.Inst.SendPacketToServer(timeSyncPacket, SendDataMode.UNRELIABLE);
	}

	public TimeSyncState State
	{
		get
		{
			return this.state_;
		}
		set
		{
			this.state_ = value;
		}
	}

	private const float DEFAULT_TIME_SYNC_RATE = 1000f;

	private const float FAST_TIME_SYNC_RATE = 0.2f;

	public TimeSyncState state_;

	private float offset_;

	private float lastTimeSync_;

	private float timeSyncRate_;

	private int syncCount_;
}
