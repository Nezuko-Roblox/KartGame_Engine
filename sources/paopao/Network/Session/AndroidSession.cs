using System;

internal class AndroidSession : Session
{
	public AndroidSession(string name, SessionMode mode)
	{
		AndroidNetwork.instance.Close();
		if (mode == SessionMode.SERVER)
		{
			AndroidNetwork.instance.StartServer(name, 3);
			this.connectedServer_ = AndroidNetwork.instance.GetID();
		}
		else
		{
			AndroidNetwork.instance.StartClient(name, 3);
		}
		this.self_ = new Peer(AndroidNetwork.instance.GetID(), name);
		this.mode_ = mode;
		Session.inst_ = this;
		this.state_ = SessionState.AVAILABLE;
	}

	public override void SendPacket(byte[] packet, string receiverID, SendDataMode mode)
	{
		AndroidNetwork.instance.SendPacket(packet, packet.Length, receiverID);
	}

	public override void SendPacketToAll(byte[] packet, SendDataMode mode)
	{
		AndroidNetwork.instance.SendPacketToAll(packet, packet.Length);
	}

	public override void Kick(string peerID)
	{
		AndroidNetwork.instance.Kick(peerID);
	}

	public override void Kick(Peer peer)
	{
		AndroidNetwork.instance.Kick(peer.id_);
	}

	public override void Connect(Peer server)
	{
		this.Connect(server.id_);
	}

	public override void Connect(string peerID)
	{
		AndroidNetwork.instance.Connect(peerID);
		this.connectedServer_ = peerID;
		this.state_ = SessionState.CONNECTING;
	}

	public override Peer[] Peers()
	{
		string[] array = AndroidNetwork.instance.Peers().Split(new char[] { '\n' });
		this.peers_ = new Peer[array.Length / 2];
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			Peer peer = new Peer(array[i], array[i + 1]);
			this.peers_[i / 2] = peer;
		}
		return this.peers_;
	}

	public override Peer[] AvailableServers()
	{
		string[] array = AndroidNetwork.instance.AvailableServers().Split(new char[] { '\n' });
		Peer[] array2 = new Peer[array.Length / 2];
		for (int i = 0; i < array.Length - 1; i += 2)
		{
			Peer peer = new Peer(array[i], array[i + 1]);
			array2[i / 2] = peer;
		}
		return array2;
	}

	public override void Close()
	{
		AndroidNetwork.instance.Close();
	}

	public override void AcceptConnections()
	{
		AndroidNetwork.instance.AcceptConnections();
	}

	public override void RejectConnections()
	{
		AndroidNetwork.instance.RejectConnection();
	}

	public override void PollGameKartPacket()
	{
	}

	public override void ClearGameKartPacket()
	{
	}
}
