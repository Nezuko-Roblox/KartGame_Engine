using System;

public class MockSession : Session
{
	public MockSession(string name, SessionMode mode)
	{
		this.self_ = new Peer("mock id", name);
		this.mode_ = mode;
		Session.inst_ = this;
	}

	public override void SendPacket(byte[] packet, string receiverID, SendDataMode mode)
	{
	}

	public override void SendPacketToAll(byte[] packet, SendDataMode mode)
	{
	}

	public override void Kick(string peerID)
	{
	}

	public override void Kick(Peer peer)
	{
	}

	public override void Connect(Peer server)
	{
		this.connectedServer_ = server.id_;
	}

	public override void Connect(string peerID)
	{
		this.connectedServer_ = peerID;
	}

	public override Peer[] Peers()
	{
		return new Peer[0];
	}

	public override Peer[] AvailableServers()
	{
		Peer[] array = new Peer[this.tries / 10];
		this.tries++;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Peer(string.Format("id {0} ", i), string.Format("name {0} ", i));
		}
		return array;
	}

	public override void Close()
	{
	}

	public override void AcceptConnections()
	{
	}

	public override void RejectConnections()
	{
	}

	public override void PollGameKartPacket()
	{
	}

	public override void ClearGameKartPacket()
	{
	}

	private int tries;
}
