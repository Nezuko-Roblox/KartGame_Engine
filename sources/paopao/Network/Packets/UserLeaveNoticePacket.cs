using System;
using System.IO;

public class UserLeaveNoticePacket : Packet
{
	public UserLeaveNoticePacket()
	{
	}

	public UserLeaveNoticePacket(string id, SessionMode mode)
	{
		this.id_ = id;
		this.mode_ = mode;
	}

	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write(this.id_);
		writer.Write((byte)this.mode_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.id_ = reader.ReadString();
		this.mode_ = (SessionMode)reader.ReadByte();
	}

	public override string ToString()
	{
		return string.Format("<UserLeaveNoticePacket> id_={0}, mode_={1}", this.id_, this.mode_);
	}

	public string id_;

	public SessionMode mode_;
}
