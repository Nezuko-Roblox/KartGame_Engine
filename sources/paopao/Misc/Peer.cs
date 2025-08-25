using System;

public class Peer
{
	public Peer(string id, string name)
	{
		this.id_ = id;
		this.name_ = name;
	}

	public string id_;

	public string name_;

	public bool ready_;

	public PlayerPacket packet_;
}
