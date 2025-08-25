using System;
using System.IO;
using UnityEngine;

public class ItemBananaParam : ItemParam
{
	public ItemBananaParam()
	{
	}

	public ItemBananaParam(int src, Vector3 pos, float id)
	{
		this.srcKartIndex_ = src;
		this.pos_ = pos;
		this.id_ = id;
	}

	public override string ToString()
	{
		return string.Format("<ItemBananaParam src_={0} pos_={0} id_={1} >", this.srcKartIndex_, this.pos_, this.id_);
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
		Serializer.Write(writer, this.pos_);
		writer.Write(this.id_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
		this.pos_ = Serializer.ReadVector3(reader);
		this.id_ = reader.ReadSingle();
	}

	public int srcKartIndex_;

	public Vector3 pos_;

	public float id_;
}
