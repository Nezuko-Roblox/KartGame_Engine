using System;
using System.IO;
using UnityEngine;

public class KartRecordHeader
{
	public override string ToString()
	{
		string text = string.Empty;
		text = text + this.version_.ToString() + this.DELIMITER;
		text = text + this.track_.ToString() + this.DELIMITER;
		text = text + this.kart_.ToString() + this.DELIMITER;
		text = text + this.character_.ToString() + this.DELIMITER;
		text = text + this.playername_ + this.DELIMITER;
		text = text + this.finishTime_.ToString() + this.DELIMITER;
		text = text + this.facebookId_ + this.DELIMITER;
		return text + this.serializeTime_ + this.DELIMITER;
	}

	public bool FromString(string str)
	{
		string[] array = str.Split(this.DELIMITER.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 8)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogError("wrong token number[8] " + array.Length.ToString());
			}
			return false;
		}
		this.version_ = byte.Parse(array[0]);
		this.track_ = byte.Parse(array[1]);
		this.kart_ = byte.Parse(array[2]);
		this.character_ = byte.Parse(array[3]);
		this.playername_ = array[4];
		this.finishTime_ = float.Parse(array[5]);
		this.facebookId_ = array[6];
		this.serializeTime_ = array[7];
		return true;
	}

	public void WriteBinary(ref BinaryWriter streamWriter)
	{
		streamWriter.Write(this.version_);
		streamWriter.Write(this.track_);
		streamWriter.Write(this.kart_);
		streamWriter.Write(this.character_);
		streamWriter.Write(this.finishTime_);
		streamWriter.Write(this.facebookId_);
		streamWriter.Write(this.serializeTime_);
	}

	public int ReadBinary(ref BinaryReader streamReader)
	{
		this.version_ = streamReader.ReadByte();
		this.track_ = streamReader.ReadByte();
		this.kart_ = streamReader.ReadByte();
		this.character_ = streamReader.ReadByte();
		this.finishTime_ = streamReader.ReadSingle();
		this.facebookId_ = streamReader.ReadString();
		this.serializeTime_ = streamReader.ReadString();
		return (int)streamReader.BaseStream.Position;
	}

	public byte version_;

	public byte track_;

	public byte kart_;

	public byte character_;

	public string playername_;

	public float finishTime_;

	public string facebookId_;

	public string serializeTime_;

	private string DELIMITER = ";";
}
