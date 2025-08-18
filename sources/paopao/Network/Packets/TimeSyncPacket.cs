using System;
using System.IO;

public class TimeSyncPacket : Packet
{
	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write(this.throwTick_);
		writer.Write(this.catchTick_);
		writer.Write(this.recvTick_);
		writer.Flush();
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.throwTick_ = reader.ReadSingle();
		this.catchTick_ = reader.ReadSingle();
		this.recvTick_ = reader.ReadSingle();
	}

	public override string ToString()
	{
		return string.Format("<TimeSyncPacket> throwTick={0}, catchTick={1}, recvTick={2}", this.throwTick_, this.catchTick_, this.recvTick_);
	}

	public float throwTick_;

	public float catchTick_;
}
