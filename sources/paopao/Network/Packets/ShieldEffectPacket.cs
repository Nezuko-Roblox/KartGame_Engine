using System;
using System.IO;

public class ShieldEffectPacket : Packet
{
	public ShieldEffectPacket()
	{
	}

	public ShieldEffectPacket(int slot)
	{
		this.slot_ = slot;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.slot_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.slot_ = (int)reader.ReadByte();
	}

	public override string ToString()
	{
		return string.Format("<ShieldEffectPacket> slot_={0}", this.slot_);
	}

	public int slot_;
}
