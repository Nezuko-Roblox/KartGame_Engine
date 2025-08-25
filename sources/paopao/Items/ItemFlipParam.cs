using System;
using System.IO;

public class ItemFlipParam : ItemParam
{
	public ItemFlipParam()
	{
	}

	public ItemFlipParam(int src, bool[] dst, bool isAttack)
	{
		this.srcKartIndex_ = src;
		Array.Copy(dst, this.dstKartIndex_, dst.Length);
	}

	public override string ToString()
	{
		return this.srcKartIndex_.ToString() + " " + this.dstKartIndex_.Length.ToString();
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
		writer.Write((byte)this.dstKartIndex_.Length);
		foreach (bool flag in this.dstKartIndex_)
		{
			writer.Write(flag);
		}
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
		this.dstKartIndex_ = new bool[(int)reader.ReadByte()];
		for (int i = 0; i < this.dstKartIndex_.Length; i++)
		{
			this.dstKartIndex_[i] = reader.ReadBoolean();
		}
	}

	public int srcKartIndex_;

	public bool[] dstKartIndex_ = new bool[6];
}
