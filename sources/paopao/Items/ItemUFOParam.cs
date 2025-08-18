using System;
using System.IO;

public class ItemUFOParam : ItemParam
{
	public ItemUFOParam()
	{
	}

	public ItemUFOParam(int src, int dst, bool isAttack)
	{
		this.srcKartIndex_ = src;
		this.dstKartIndex_ = dst;
		this.isAttack_ = isAttack;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			this.srcKartIndex_.ToString(),
			" ",
			this.dstKartIndex_.ToString(),
			" ",
			this.isAttack_.ToString()
		});
	}

	public override void WriteTo(BinaryWriter writer)
	{
		writer.Write((byte)this.srcKartIndex_);
		writer.Write((byte)this.dstKartIndex_);
		writer.Write(this.isAttack_);
	}

	public override void ReadFrom(BinaryReader reader)
	{
		this.srcKartIndex_ = (int)reader.ReadByte();
		this.dstKartIndex_ = (int)reader.ReadByte();
		this.isAttack_ = reader.ReadBoolean();
	}

	public int srcKartIndex_;

	public int dstKartIndex_;

	public bool isAttack_;
}
