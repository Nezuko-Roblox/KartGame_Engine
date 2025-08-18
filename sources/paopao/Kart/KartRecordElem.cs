using System;
using System.IO;
using UnityEngine;

public struct KartRecordElem
{
	public KartRecordElem(float t, Vector3 pos, Quaternion rot, int state)
	{
		this.time_ = t;
		this.position_ = pos;
		this.rotation_ = rot;
		this.state_ = (byte)state;
	}

	public override string ToString()
	{
		string empty = string.Empty;
		string text = " ";
		return string.Concat(new string[]
		{
			this.time_.ToString(),
			text,
			this.position_.x.ToString(),
			text,
			this.position_.y.ToString(),
			text,
			this.position_.z.ToString(),
			text,
			this.rotation_.x.ToString(),
			text,
			this.rotation_.y.ToString(),
			text,
			this.rotation_.z.ToString(),
			text,
			this.rotation_.w.ToString(),
			text,
			this.state_.ToString()
		});
	}

	public void ParseString(string str)
	{
		char[] array = new char[] { ' ', '\t' };
		string[] array2 = str.Split(array);
		if (array2.Length != 9)
		{
		}
		this.time_ = float.Parse(array2[0]);
		this.position_ = Vector3Helper.CreateVector3(array2[1], array2[2], array2[3]);
		this.rotation_ = MathHelper.ToQuaternion(array2[4], array2[5], array2[6], array2[7]);
		this.state_ = byte.Parse(array2[8]);
	}

	public void WriteBinary(ref BinaryWriter streamWriter)
	{
		streamWriter.Write(this.time_);
		streamWriter.Write(this.position_.x);
		streamWriter.Write(this.position_.y);
		streamWriter.Write(this.position_.z);
		streamWriter.Write(this.rotation_.x);
		streamWriter.Write(this.rotation_.y);
		streamWriter.Write(this.rotation_.z);
		streamWriter.Write(this.rotation_.w);
		streamWriter.Write(this.state_);
	}

	public int ReadBinary(ref BinaryReader streamReader)
	{
		this.time_ = streamReader.ReadSingle();
		this.position_ = Vector3.zero;
		this.position_.x = streamReader.ReadSingle();
		this.position_.y = streamReader.ReadSingle();
		this.position_.z = streamReader.ReadSingle();
		this.rotation_ = Quaternion.identity;
		this.rotation_.x = streamReader.ReadSingle();
		this.rotation_.y = streamReader.ReadSingle();
		this.rotation_.z = streamReader.ReadSingle();
		this.rotation_.w = streamReader.ReadSingle();
		this.state_ = streamReader.ReadByte();
		return 33;
	}

	public float time_;

	public Vector3 position_;

	public Quaternion rotation_;

	public byte state_;
}
