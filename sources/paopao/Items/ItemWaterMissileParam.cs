using System;
using System.IO;

public class ItemWaterMissileParam : ItemParam
{
	public ItemWaterMissileParam()
	{
	}

	public ItemWaterMissileParam(int src, int dst)
	{
		this.srcKartIndex_ = src;
		this.dstKartIndex_ = dst;
	}

	public override string ToString()
	{
		return this.srcKartIndex_.ToString() + " " + this.dstKartIndex_.ToString();
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
		writer.Write((byte)this.dstKartIndex_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
		this.dstKartIndex_ = (int)reader.ReadByte();
	}

	public int srcKartIndex_;

	public int dstKartIndex_;
}
