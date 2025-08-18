using System;
using System.IO;

public class ItemWaterBombParam : ItemParam
{
	public ItemWaterBombParam()
	{
	}

	public ItemWaterBombParam(int src)
	{
		this.srcKartIndex_ = src;
	}

	public override string ToString()
	{
		return this.srcKartIndex_.ToString();
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
	}

	public int srcKartIndex_;
}
