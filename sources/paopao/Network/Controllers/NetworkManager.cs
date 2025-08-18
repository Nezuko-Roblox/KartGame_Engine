using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : PacketHandler
{
	private NetworkManager()
	{
		this.packetQueue_ = new Queue<NetworkManager.PacketWrapper>();
	}

	private static int _Reachability()
	{
		return 0;
	}

	public void Awake()
	{
		NetworkManager.inst_ = this;
	}

	public Session Session
	{
		get
		{
			return this.session_;
		}
		set
		{
			this.session_ = value;
			this.session_.PacketHandler = this;
		}
	}

	public NetStage Stage
	{
		get
		{
			return this.stage_;
		}
		set
		{
			this.stage_ = value;
			if (this.packetQueue_ != null && this.stage_ != null)
			{
				while (this.packetQueue_.Count > 0)
				{
					if (this.stage_ == null)
					{
						break;
					}
					NetworkManager.PacketWrapper packetWrapper = this.packetQueue_.Dequeue();
					this.stage_.ProcessPacket(packetWrapper.packet_, packetWrapper.sender_);
				}
			}
			if (this.packetQueue_ == null)
			{
				this.packetQueue_ = new Queue<NetworkManager.PacketWrapper>();
			}
		}
	}

	public void Close()
	{
		if (this.session_ != null)
		{
			this.session_.Close();
		}
	}

	public static NetworkManager Inst
	{
		get
		{
			if (NetworkManager.inst_ == null)
			{
				NetworkManager.inst_ = new NetworkManager();
			}
			return NetworkManager.inst_;
		}
	}

	public bool ProcessPacket(byte[] pbyte, string senderID)
	{
		return this.ProcessPacket(PacketFactory.Inst.Deserialize(pbyte), senderID);
	}

	public bool ProcessPacket(Packet packet, string senderID)
	{
		if (packet == null)
		{
			return true;
		}
		if (this.stage_ == null)
		{
			if (this.packetQueue_ == null)
			{
				this.packetQueue_ = new Queue<NetworkManager.PacketWrapper>();
			}
			this.packetQueue_.Enqueue(new NetworkManager.PacketWrapper(packet, senderID));
			return false;
		}
		return this.stage_.ProcessPacket(packet, senderID);
	}

	public void TerminationEvent()
	{
		if (this.Session != null)
		{
			UserLeaveNoticePacket userLeaveNoticePacket = new UserLeaveNoticePacket(this.Session.ID, this.Session.Mode);
			if (this.Session.Mode == SessionMode.SERVER)
			{
				this.SendPacketToAll(userLeaveNoticePacket, SendDataMode.RELIABLE);
			}
			else if (this.Session.Mode == SessionMode.CLIENT)
			{
				this.SendPacketToServer(userLeaveNoticePacket, SendDataMode.RELIABLE);
			}
			this.ProcessPacket(userLeaveNoticePacket, this.Session.ID);
			this.Session.Close();
		}
	}

	public void ConnectionFailed(string serverID)
	{
		this.stage_.ConnectionFailed(serverID);
	}

	public float MakeServerT2LocalT(float tick)
	{
		return tick - this.offset_;
	}

	public float MakeLocalT2ServerT(float tick)
	{
		return tick + this.offset_;
	}

	public void ResetTimeOffset()
	{
		this.offset_ = 0f;
	}

	public void TimeSync(TimeSyncPacket packet, string senderID)
	{
		if (packet.catchTick_ == 0f)
		{
			packet.catchTick_ = Time.time;
			this.SendPacket(packet, senderID, SendDataMode.UNRELIABLE);
		}
		else
		{
			float num = packet.recvTick_ - packet.throwTick_;
			if (this.offset_ == 0f)
			{
				this.offset_ = packet.catchTick_ - packet.throwTick_ - num / 2f;
			}
			else if (num < 0.1f)
			{
				this.offset_ += packet.catchTick_ - packet.throwTick_ - num / 2f;
				this.offset_ /= 2f;
			}
		}
	}

	public void StartTimeSync()
	{
		this.SendPacketToServer(new TimeSyncPacket
		{
			throwTick_ = Time.time
		}, SendDataMode.UNRELIABLE);
	}

	public void SendPacket(Packet packet, string receiverID, SendDataMode mode)
	{
		this.session_.SendPacket(packet.Serialize(mode), receiverID, mode);
	}

	public void SendPacketToServer(Packet packet, SendDataMode mode)
	{
		this.session_.SendPacket(packet.Serialize(mode), this.session_.ConnectedServer, mode);
	}

	public void SendPacketToAll(Packet packet, SendDataMode mode)
	{
		this.session_.SendPacketToAll(packet.Serialize(mode), mode);
	}

	public virtual NetworkStatus NetworkStatus()
	{
		return (NetworkStatus)NetworkManager._Reachability();
	}

	private static NetworkManager inst_;

	private Session session_;

	public NetStage stage_;

	private float offset_;

	public TimeSync sync_;

	private Queue<NetworkManager.PacketWrapper> packetQueue_;

	private struct PacketWrapper
	{
		public PacketWrapper(Packet packet, string sender)
		{
			this.packet_ = packet;
			this.sender_ = sender;
		}

		public Packet packet_;

		public string sender_;
	}
}
