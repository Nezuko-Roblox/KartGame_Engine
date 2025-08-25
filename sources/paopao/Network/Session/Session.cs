using System;

public class Session
{
	public Session()
	{
	}

	public Session(string name, SessionMode mode)
	{
		if (Session.inst_ != null)
		{
			Session.inst_.Close();
		}
		if (mode == SessionMode.SERVER)
		{
			Session._StartServer(name, 3);
			this.connectedServer_ = Session._GetID();
		}
		else if (mode == SessionMode.CLIENT)
		{
			Session._StartClient(name, 3);
		}
		this.self_ = new Peer(Session._GetID(), name);
		this.mode_ = mode;
		Session.inst_ = this;
		this.state_ = SessionState.AVAILABLE;
	}

	private static void _StartServer(string name, int maxPeers)
	{
	}

	private static void _StartClient(string name, int maxPeers)
	{
	}

	private static void _Close()
	{
	}

	private static string _GetID()
	{
		return string.Empty;
	}

	private static string _Peers()
	{
		return string.Empty;
	}

	private static void _SendPacket(byte[] packet, int size, string receiverID, SendDataMode mode)
	{
	}

	private static void _SendPacketToAll(byte[] packet, int size, SendDataMode mode)
	{
	}

	private static void _RejectConnections()
	{
	}

	private static void _AcceptConnections()
	{
	}

	private static void _Kick(string peerID)
	{
	}

	private static string _AvailableServers()
	{
		return string.Empty;
	}

	private static void _Connect(string peerID)
	{
	}

	private static object[] _PollGameKartPacket()
	{
		return null;
	}

	private static void _ClearGameKartPacket()
	{
	}

	public SessionMode Mode
	{
		get
		{
			return this.mode_;
		}
	}

	public string Name
	{
		get
		{
			return this.self_.name_;
		}
		set
		{
			this.self_.name_ = value;
		}
	}

	public SessionState State
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

	public string ID
	{
		get
		{
			return this.self_.id_;
		}
	}

	public Peer Self
	{
		get
		{
			return this.self_;
		}
		set
		{
			this.self_ = value;
		}
	}

	public PacketHandler PacketHandler
	{
		get
		{
			return this.packetHandler_;
		}
		set
		{
			this.packetHandler_ = value;
		}
	}

	public string ConnectedServer
	{
		get
		{
			return this.connectedServer_;
		}
	}

	public virtual void SendPacket(byte[] packet, string receiverID, SendDataMode mode)
	{
		Session._SendPacket(packet, packet.Length, receiverID, mode);
	}

	public virtual void SendPacketToAll(byte[] packet, SendDataMode mode)
	{
		Session._SendPacketToAll(packet, packet.Length, mode);
	}

	public static void ProcessPacket(byte[] packet, string senderID)
	{
		Session.inst_.PacketHandler.ProcessPacket(packet, senderID);
	}

	public static void PeerDisconnected(string peerID)
	{
		SessionMode sessionMode;
		if (Session.inst_.connectedServer_ == peerID)
		{
			sessionMode = SessionMode.SERVER;
		}
		else
		{
			sessionMode = SessionMode.CLIENT;
		}
		UserLeaveNoticePacket userLeaveNoticePacket = new UserLeaveNoticePacket(peerID, sessionMode);
		Session.inst_.PacketHandler.ProcessPacket(userLeaveNoticePacket, peerID);
	}

	public static void TerminationEvent()
	{
		Session.inst_.PacketHandler.TerminationEvent();
	}

	public static void WifiBluetoothOff()
	{
		iOSEvent.Alert("네트워크에 연결할 수 없습니다. 블루투스를 켜주세요.");
	}

	public static void ConnectionFailed(string serverID)
	{
		Session.inst_.State = SessionState.AVAILABLE;
		Session.inst_.PacketHandler.ConnectionFailed(serverID);
	}

	public static void ConnectionSuccess(string serverID)
	{
		Session.inst_.State = SessionState.CONNECTED;
	}

	public virtual void Kick(string peerID)
	{
		Session._Kick(peerID);
	}

	public virtual void Kick(Peer peer)
	{
		Session._Kick(peer.id_);
	}

	public virtual void Connect(Peer server)
	{
		this.Connect(server.id_);
	}

	public virtual void Connect(string peerID)
	{
		Session._Connect(peerID);
		this.connectedServer_ = peerID;
		this.state_ = SessionState.CONNECTING;
	}

	public virtual void PollGameKartPacket()
	{
		object[] array = Session._PollGameKartPacket();
		if (array != null)
		{
			foreach (object obj in array)
			{
				Session.ProcessPacket((byte[])obj, null);
			}
		}
	}

	public virtual Peer[] Peers()
	{
		string[] array = Session._Peers().Split(new char[] { '\n' });
		this.peers_ = new Peer[array.Length / 2];
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			Peer peer = new Peer(array[i], array[i + 1]);
			this.peers_[i / 2] = peer;
		}
		return this.peers_;
	}

	public virtual Peer[] AvailableServers()
	{
		string[] array = Session._AvailableServers().Split(new char[] { '\n' });
		Peer[] array2 = new Peer[array.Length / 2];
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			Peer peer = new Peer(array[i], array[i + 1]);
			array2[i / 2] = peer;
		}
		return array2;
	}

	public virtual bool Connected
	{
		get
		{
			return this.Peers().Length > 0;
		}
	}

	public virtual void Close()
	{
		Session._Close();
	}

	public virtual void ClearGameKartPacket()
	{
		Session._ClearGameKartPacket();
	}

	public virtual void AcceptConnections()
	{
		Session._AcceptConnections();
	}

	public virtual void RejectConnections()
	{
		Session._RejectConnections();
	}

	protected string name_;

	protected string id_;

	protected PacketHandler packetHandler_;

	protected SessionMode mode_;

	protected SessionState state_;

	protected Peer self_;

	public string connectedServer_;

	public static Session inst_;

	public Peer[] peers_;
}
