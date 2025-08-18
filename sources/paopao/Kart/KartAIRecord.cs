using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KartAIRecord : LinkedList<KartAIRecordElem>
{
	public float StandardTime
	{
		get
		{
			return this.standardTime_;
		}
		set
		{
			this.standardTime_ = value;
		}
	}

	public void AddRecord(float t)
	{
		bool flag = false;
		if (this.Count <= 0)
		{
			if (KartManager.Instance.goCourse_.GetLastCorrectDirectionPassingPlane() >= 1)
			{
				this.firstRecordTime_ = t;
				flag = true;
			}
		}
		else if ((int)((t - this.firstRecordTime_) / 0.1f) >= this.Count)
		{
			flag = true;
		}
		if (flag)
		{
			Transform transform = KartManager.Instance.goPlayKart_.m_kart.transform;
			base.AddLast(new KartAIRecordElem(t, transform.position, transform.localRotation, (int)KartManager.Instance.goPlayKart_.CharacterAnim, KartManager.Instance.goCourse_.GetDistancePlane(KartManager.PLAYER_KART_IDX), KartManager.Instance.goCourse_.GetDistance(KartManager.PLAYER_KART_IDX)));
		}
	}

	public void AddUserSection(int sectionindex)
	{
		if (sectionindex != -1)
		{
			this.userSection_ |= 1 << sectionindex;
		}
	}

	public void Serialize(string fileName)
	{
		string recordPath = FiaUtil.recordPath;
		if (!Directory.Exists(recordPath))
		{
			Directory.CreateDirectory(recordPath);
		}
		StreamWriter streamWriter = new StreamWriter(Path.Combine(FiaUtil.recordPath, fileName));
		if (streamWriter != null)
		{
			streamWriter.WriteLine(this.userSection_.ToString());
			for (LinkedListNode<KartAIRecordElem> linkedListNode = base.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				streamWriter.WriteLine(linkedListNode.Value.ToString());
			}
		}
		streamWriter.Close();
	}

	public void SerializeToBin(string fileName)
	{
		string recordPath = FiaUtil.recordPath;
		if (!Directory.Exists(recordPath))
		{
			Directory.CreateDirectory(recordPath);
		}
		BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(Path.Combine(FiaUtil.recordPath, fileName)));
		if (binaryWriter != null)
		{
			binaryWriter.Write(this.userSection_);
			for (LinkedListNode<KartAIRecordElem> linkedListNode = base.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				linkedListNode.Value.WriteBinary(ref binaryWriter);
			}
		}
		binaryWriter.Close();
	}

	public void LoadBinaryFile(string filename)
	{
		BinaryReader binaryReader = new BinaryReader(File.OpenRead(filename));
		if (binaryReader != null)
		{
			int i = 0;
			int num = (int)binaryReader.BaseStream.Length;
			this.userSection_ = binaryReader.ReadInt32();
			i += 4;
			while (i < num)
			{
				KartAIRecordElem kartAIRecordElem = default(KartAIRecordElem);
				i += kartAIRecordElem.ReadBinary(ref binaryReader);
				base.AddLast(kartAIRecordElem);
			}
		}
	}

	public void Load(string fileName)
	{
		StreamReader streamReader = File.OpenText(Path.Combine(FiaUtil.recordPath, fileName));
		if (streamReader != null)
		{
			string text = string.Empty;
			text = streamReader.ReadLine();
			this.userSection_ = int.Parse(text);
			while ((text = streamReader.ReadLine()) != null)
			{
				KartAIRecordElem kartAIRecordElem = default(KartAIRecordElem);
				kartAIRecordElem.ParseString(text);
				base.AddLast(kartAIRecordElem);
			}
		}
		if (this.recordIter_ != null)
		{
		}
	}

	public void Load(BinaryAsset asset)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(asset.content_));
		if (binaryReader != null)
		{
			int i = 0;
			int num = (int)binaryReader.BaseStream.Length;
			this.userSection_ = binaryReader.ReadInt32();
			i += 4;
			while (i < num)
			{
				KartAIRecordElem kartAIRecordElem = default(KartAIRecordElem);
				i += kartAIRecordElem.ReadBinary(ref binaryReader);
				base.AddLast(kartAIRecordElem);
			}
		}
	}

	public float GetRecordIterTime()
	{
		if (this.recordIter_ == null)
		{
			return -1f;
		}
		return this.recordIter_.Value.time_;
	}

	public void ResetRecordIter()
	{
		this.recordIter_ = null;
	}

	public int GetUserSection()
	{
		return this.userSection_;
	}

	public float GetTick(int distancePlane, float distance)
	{
		float num = 0f;
		LinkedListNode<KartAIRecordElem> linkedListNode = base.First;
		if (linkedListNode != null)
		{
			double num2 = (double)((float)((long)distancePlane << 32) + distance);
			double num3 = (double)((float)((long)linkedListNode.Value.distancePlane_ << 32) + linkedListNode.Value.distance_);
			while (linkedListNode != null && num3 <= num2)
			{
				num = linkedListNode.Value.time_;
				linkedListNode = linkedListNode.Next;
				num3 = (double)((float)((long)linkedListNode.Value.distancePlane_ << 32) + linkedListNode.Value.distance_);
			}
		}
		return num + this.standardTime_;
	}

	public int GetRecord(float t, out KartAIRecordElem e)
	{
		if (this.recordIter_ == null)
		{
			this.recordIter_ = base.First;
		}
		t -= this.standardTime_;
		if (this.recordIter_.Value.time_ >= t || this.recordIter_.Next == null)
		{
			e = this.recordIter_.Value;
			int num = ((this.recordIter_.Value.time_ < t) ? 1 : (-1));
			e.time_ += this.standardTime_;
			return num;
		}
		LinkedListNode<KartAIRecordElem> linkedListNode = this.recordIter_.Next;
		while (this.recordIter_.Value.time_ > t || linkedListNode.Value.time_ < t)
		{
			this.recordIter_ = this.recordIter_.Next;
			linkedListNode = linkedListNode.Next;
			if (linkedListNode == null)
			{
				break;
			}
		}
		if (linkedListNode == null)
		{
			e = this.recordIter_.Value;
			e.time_ = t + this.standardTime_;
			return 1;
		}
		KartAIRecordElem value = this.recordIter_.Value;
		KartAIRecordElem value2 = linkedListNode.Value;
		float num2 = (t - value.time_) / (value2.time_ - value.time_);
		e = new KartAIRecordElem(t + this.standardTime_, Vector3.Lerp(value.position_, value2.position_, num2), Quaternion.Slerp(value.rotation_, value2.rotation_, num2), (int)value.state_, (int)value.distancePlane_, value.distance_);
		return 0;
	}

	public void ResetForRestarting()
	{
		this.recordIter_ = null;
	}

	private const float RECORD_INTERVAL = 0.1f;

	private float firstRecordTime_;

	private int userSection_;

	private float standardTime_;

	private LinkedListNode<KartAIRecordElem> recordIter_;
}
