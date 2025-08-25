using System;
using System.IO;
using UnityEngine;

public class GameKartPacket : Packet
{
	public override void WriteTo(BinaryWriter writer)
	{
		base.WriteTo(writer);
		writer.Write((byte)this.slot_);
		Serializer.Write(writer, this.position_);
		Serializer.Write(writer, this.velocity_);
		Serializer.Write(writer, this.rotation_);
		writer.Write((byte)this.characterAnimation_);
		writer.Write((byte)this.kartBodyAnimation_);
		writer.Write(this.tick_);
		writer.Write(this.rankValue_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		base.ReadFrom(reader);
		this.slot_ = (int)reader.ReadByte();
		this.position_ = Serializer.ReadVector3(reader);
		this.velocity_ = Serializer.ReadVector3(reader);
		this.rotation_ = Serializer.ReadQuaternion(reader);
		this.characterAnimation_ = (CharacterAnimation)reader.ReadByte();
		this.kartBodyAnimation_ = (KartBodyAnimation)reader.ReadByte();
		this.tick_ = reader.ReadSingle();
		this.rankValue_ = reader.ReadDouble();
	}

	public override string ToString()
	{
		return string.Format("<GameKartPacket> slot_={0}, position_={1}, velocity_={2}, rotation_={3}, characterAnimation_={4}, kartBodyAnimation_={5} tick_={6}", new object[] { this.slot_, this.position_, this.velocity_, this.rotation_, this.characterAnimation_, this.kartBodyAnimation_, this.tick_ });
	}

	public int slot_;

	public Vector3 position_;

	public Vector3 velocity_;

	public Quaternion rotation_;

	public CharacterAnimation characterAnimation_;

	public KartBodyAnimation kartBodyAnimation_;

	public float tick_;

	public double rankValue_;
}
