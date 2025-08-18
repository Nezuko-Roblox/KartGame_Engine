using System;
using System.IO;

public class PlayerResultPacket : Packet
{
	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write(this.id_);
		writer.Write((byte)this.index_);
		writer.Write(this.raceEndTime_);
		writer.Write((byte)this.rank_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.id_ = reader.ReadString();
		this.index_ = (int)reader.ReadByte();
		this.raceEndTime_ = reader.ReadSingle();
		this.rank_ = (int)reader.ReadByte();
	}

	public override string ToString()
	{
		return string.Format("<PlayerResultPacket> id_={0}, index_={1}, raceEndTime_={2}, rank_={3}", new object[] { this.id_, this.index_, this.raceEndTime_, this.rank_ });
	}

	public string id_;

	public int index_;

	public float raceEndTime_;

	public int rank_;
}
