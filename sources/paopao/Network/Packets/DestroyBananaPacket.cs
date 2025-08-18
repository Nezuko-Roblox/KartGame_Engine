using System;
using System.IO;

public class DestroyBananaPacket : Packet
{
	public DestroyBananaPacket()
	{
	}

	public DestroyBananaPacket(float id)
	{
		this.id_ = id;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write(this.id_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.id_ = reader.ReadSingle();
	}

	public override string ToString()
	{
		return string.Format("<DestroyBananaPacket> id_={0}", this.id_);
	}

	public float id_;
}
