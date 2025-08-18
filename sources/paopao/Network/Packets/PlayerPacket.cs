using System;
using System.IO;

public class PlayerPacket : Packet
{
	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write(this.id_);
		writer.Write(this.name_);
		writer.Write(this.slot_);
		writer.Write(this.characterName_);
		writer.Write(this.kartName_);
		writer.Write(this.ready_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.id_ = reader.ReadString();
		this.name_ = reader.ReadString();
		this.slot_ = reader.ReadInt32();
		this.characterName_ = reader.ReadByte();
		this.kartName_ = reader.ReadByte();
		this.ready_ = reader.ReadBoolean();
	}

	public override string ToString()
	{
		return string.Format("<PlayerPacket> id_={0}, name_={1}, slot_={2}, \ncharacterName_={3}, kartName_={4}, ready_={5}", new object[] { this.id_, this.name_, this.slot_, this.characterName_, this.kartName_, this.ready_ });
	}

	public string id_;

	public string name_;

	public int slot_;

	public byte characterName_;

	public byte kartName_;

	public bool ready_;
}
