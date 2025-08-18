using System;
using System.IO;

public class GameControlPacket : Packet
{
	public GameControlPacket()
	{
	}

	public GameControlPacket(GameControlPacket.Control control, float time)
	{
		this.control_ = control;
		this.time_ = time;
	}

	public GameControlPacket(GameControlPacket.Control control)
		: this(control, 0f)
	{
	}

	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write((byte)this.control_);
		if (this.control_ != GameControlPacket.Control.LOADING_DONE)
		{
			writer.Write(this.time_);
		}
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.control_ = (GameControlPacket.Control)reader.ReadByte();
		if (this.control_ != GameControlPacket.Control.LOADING_DONE)
		{
			this.time_ = reader.ReadSingle();
		}
	}

	public override string ToString()
	{
		return string.Format("<GameControlPacket> control={0}, time={1}", this.control_, this.time_);
	}

	public GameControlPacket.Control control_;

	public float time_;

	public enum Control
	{
		LOADING_DONE,
		RACING_START,
		FINISH_REPORT,
		FINISH_NOTICE,
		RACE_OVER
	}
}
