using System;
using System.IO;

public class ItemWaterFlyParam : ItemParam
{
	public ItemWaterFlyParam()
	{
	}

	public ItemWaterFlyParam(int src, int target)
	{
		this.srcKartIndex_ = src;
		this.targetKartIndex_ = target;
	}

	public override string ToString()
	{
		return this.srcKartIndex_.ToString() + " " + this.targetKartIndex_.ToString();
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
		writer.Write((byte)this.targetKartIndex_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
		this.targetKartIndex_ = (int)reader.ReadByte();
	}

	public int srcKartIndex_;

	public int targetKartIndex_;
}
