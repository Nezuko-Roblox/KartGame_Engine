using System;
using System.IO;

public class ServerStatePacket : Packet
{
	public ServerStatePacket()
	{
	}

	public ServerStatePacket(ServerState state)
	{
		this.state_ = state;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write((byte)this.state_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.state_ = (ServerState)reader.ReadByte();
	}

	public override string ToString()
	{
		return string.Format("<ServerStatePacket> state_={0}", this.state_);
	}

	private ServerState state_;
}
