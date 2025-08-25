using System;
using System.IO;

public abstract class Packet : Serializable
{
	public virtual void WriteTo(BinaryWriter writer)
	{
	}

	public virtual void ReadFrom(BinaryReader reader)
	{
	}

	public virtual byte[] Serialize(SendDataMode mode)
	{
		return PacketFactory.Inst.Serialize(this, mode);
	}

	public float recvTick_;
}
