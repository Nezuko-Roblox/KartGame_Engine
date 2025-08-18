using System;
using System.IO;
using UnityEngine;

public struct KartAIRecordElem
{
	public KartAIRecordElem(float t, Vector3 pos, Quaternion rot, int state, int distancePlane, float distance)
	{
		this.time_ = t;
		this.position_ = pos;
		this.rotation_ = rot;
		this.state_ = (byte)state;
		this.distancePlane_ = (byte)distancePlane;
		this.distance_ = distance;
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
			this.state_.ToString(),
			text,
			this.distancePlane_.ToString(),
			text,
			this.distance_.ToString()
		});
	}

	public void ParseString(string str)
	{
		char[] array = new char[] { ' ', '\t' };
		string[] array2 = str.Split(array);
		int num = 11;
		if (array2.Length != num)
		{
		}
		this.time_ = float.Parse(array2[0]);
		this.position_ = Vector3Helper.CreateVector3(array2[1], array2[2], array2[3]);
		this.rotation_ = MathHelper.ToQuaternion(array2[4], array2[5], array2[6], array2[7]);
		this.state_ = byte.Parse(array2[8]);
		this.distancePlane_ = byte.Parse(array2[9]);
		this.distance_ = float.Parse(array2[10]);
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
		streamWriter.Write(this.distancePlane_);
		streamWriter.Write(this.distance_);
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
		this.distancePlane_ = streamReader.ReadByte();
		this.distance_ = streamReader.ReadSingle();
		return 38;
	}

	public float time_;

	public Vector3 position_;

	public Quaternion rotation_;

	public byte state_;

	public byte distancePlane_;

	public float distance_;
}
