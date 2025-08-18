using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KartRecord : LinkedList<KartRecordElem>
{
	public KartRecord()
	{
		this.header_ = new KartRecordHeader();
	}

	public void AddRecord(float t)
	{
		if (this.Count <= 0)
		{
			if (KartManager.Instance.goCourse_.GetLastCorrectDirectionPassingPlane() >= 1)
			{
				this.firstRecordTime_ = t;
				Transform transform = KartManager.Instance.goPlayKart_.m_kart.transform;
				base.AddLast(new KartRecordElem(t, transform.position, transform.localRotation, (int)KartManager.Instance.goPlayKart_.CharacterAnim));
			}
		}
		else if ((int)((t - this.firstRecordTime_) / 0.5f) >= this.Count)
		{
			Transform transform2 = KartManager.Instance.goPlayKart_.m_kart.transform;
			base.AddLast(new KartRecordElem(t, transform2.position, transform2.localRotation, (int)KartManager.Instance.goPlayKart_.CharacterAnim));
		}
	}

	public void Serialize(string fileName)
	{
		string recordPath = FiaUtil.recordPath;
		if (!Directory.Exists(recordPath))
		{
			Directory.CreateDirectory(recordPath);
		}
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX];
		if (kartParameter == null && Debug.isDebugBuild)
		{
			Debug.LogError("player kart index paramater is not exist!!!");
		}
		float finishTime = KartManager.Instance.goCourse_.GetFinishTime(KartManager.PLAYER_KART_IDX);
		if (finishTime <= 0f)
		{
			return;
		}
		this.header_.version_ = 1;
		this.header_.track_ = KartManager.Instance.parameter_.track_;
		this.header_.kart_ = kartParameter.body_;
		this.header_.playername_ = kartParameter.name_;
		this.header_.character_ = kartParameter.character_;
		this.header_.finishTime_ = finishTime;
		this.header_.serializeTime_ = DateTime.Now.ToString("yyyyMMddHHmmss");
		this.header_.facebookId_ = "ccao@nexon.co.kr";
		StreamWriter streamWriter = new StreamWriter(Path.Combine(FiaUtil.recordPath, fileName));
		if (streamWriter != null)
		{
			streamWriter.WriteLine(this.header_.ToString());
			for (LinkedListNode<KartRecordElem> linkedListNode = base.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				streamWriter.WriteLine(linkedListNode.Value.ToString());
			}
		}
		streamWriter.Close();
	}

	public void SerializeToBin(string filename)
	{
		string recordPath = FiaUtil.recordPath;
		if (!Directory.Exists(recordPath))
		{
			Directory.CreateDirectory(recordPath);
		}
		KartParameter kartParameter = KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX];
		if (kartParameter == null)
		{
			Debug.LogError("player kart index paramater is not exist!!!");
		}
		float finishTime = KartManager.Instance.goCourse_.GetFinishTime(KartManager.PLAYER_KART_IDX);
		if (finishTime <= 0f)
		{
			return;
		}
		this.header_.version_ = 1;
		this.header_.track_ = KartManager.Instance.parameter_.track_;
		this.header_.kart_ = kartParameter.body_;
		this.header_.playername_ = kartParameter.name_;
		this.header_.character_ = kartParameter.character_;
		this.header_.finishTime_ = finishTime;
		this.header_.serializeTime_ = DateTime.Now.ToString("yyyyMMddHHmmss");
		this.header_.facebookId_ = "ccao@nexon.co.kr";
		BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(Path.Combine(FiaUtil.recordPath, filename)));
		if (binaryWriter != null)
		{
			this.header_.WriteBinary(ref binaryWriter);
			for (LinkedListNode<KartRecordElem> linkedListNode = base.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				linkedListNode.Value.WriteBinary(ref binaryWriter);
			}
		}
		binaryWriter.Close();
	}

	public bool LoadByResource(string filename, bool isBinary)
	{
		string text = "record/" + filename;
		if (!isBinary)
		{
			StringReader stringReader = new StringReader(((TextAsset)Resources.Load(text)).text);
			if (stringReader == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("StreamReader is null ");
				}
				return false;
			}
			string text2 = string.Empty;
			if ((text2 = stringReader.ReadLine()) == null)
			{
				stringReader.Close();
				return false;
			}
			if (!this.header_.FromString(text2))
			{
				stringReader.Close();
				return false;
			}
			if (this.header_.version_ != 1)
			{
				stringReader.Close();
				return false;
			}
			while ((text2 = stringReader.ReadLine()) != null)
			{
				KartRecordElem kartRecordElem = default(KartRecordElem);
				kartRecordElem.ParseString(text2);
				base.AddLast(kartRecordElem);
			}
			stringReader.Close();
		}
		else
		{
			TextAsset textAsset = (TextAsset)Resources.Load(text, typeof(TextAsset));
			if (textAsset == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("text asset is null : " + filename);
				}
				return false;
			}
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(textAsset.bytes));
			if (binaryReader == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("BinaryReader is null ");
				}
				return false;
			}
			int num = (int)binaryReader.BaseStream.Length;
			int i = 0;
			i += this.header_.ReadBinary(ref binaryReader);
			if (this.header_.version_ != 1)
			{
				binaryReader.Close();
				return false;
			}
			while (i < num)
			{
				KartRecordElem kartRecordElem2 = default(KartRecordElem);
				i += kartRecordElem2.ReadBinary(ref binaryReader);
				base.AddLast(kartRecordElem2);
			}
		}
		this.recordIter_ = null;
		this.isLoaded_ = true;
		return true;
	}

	public bool Load(string fileName)
	{
		string text = Path.Combine(FiaUtil.recordPath, fileName);
		if (fileName.Contains(".txt"))
		{
			StreamReader streamReader = File.OpenText(text);
			if (streamReader == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("StreamReader is null ");
				}
				return false;
			}
			string text2 = string.Empty;
			if ((text2 = streamReader.ReadLine()) == null)
			{
				streamReader.Close();
				return false;
			}
			if (!this.header_.FromString(text2))
			{
				streamReader.Close();
				return false;
			}
			if (this.header_.version_ != 1)
			{
				streamReader.Close();
				return false;
			}
			while ((text2 = streamReader.ReadLine()) != null)
			{
				KartRecordElem kartRecordElem = default(KartRecordElem);
				kartRecordElem.ParseString(text2);
				base.AddLast(kartRecordElem);
			}
			streamReader.Close();
		}
		else
		{
			BinaryReader binaryReader = new BinaryReader(File.OpenRead(text));
			if (binaryReader == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogError("BinaryReader is null ");
				}
				return false;
			}
			int num = (int)binaryReader.BaseStream.Length;
			int i = 0;
			i += this.header_.ReadBinary(ref binaryReader);
			if (this.header_.version_ != 1)
			{
				binaryReader.Close();
				return false;
			}
			while (i < num)
			{
				KartRecordElem kartRecordElem2 = default(KartRecordElem);
				i += kartRecordElem2.ReadBinary(ref binaryReader);
				base.AddLast(kartRecordElem2);
			}
		}
		this.recordIter_ = null;
		this.isLoaded_ = true;
		return true;
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

	public void ResetForRestarting()
	{
		this.recordIter_ = null;
	}

	public int GetRecord(float t, out KartRecordElem e)
	{
		if (this.recordIter_ == null)
		{
			this.recordIter_ = base.First;
		}
		if (this.recordIter_.Value.time_ >= t || this.recordIter_.Next == null)
		{
			e = this.recordIter_.Value;
			return (this.recordIter_.Value.time_ < t) ? 1 : (-1);
		}
		LinkedListNode<KartRecordElem> linkedListNode = this.recordIter_.Next;
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
			e.time_ = t;
		}
		else
		{
			KartRecordElem value = this.recordIter_.Value;
			KartRecordElem value2 = linkedListNode.Value;
			float num = (t - value.time_) / (value2.time_ - value.time_);
			e = new KartRecordElem(t, Vector3.Lerp(value.position_, value2.position_, num), Quaternion.Slerp(value.rotation_, value2.rotation_, num), (int)value.state_);
		}
		return 0;
	}

	public const byte VERSION_ = 1;

	private const float RECORD_INTERVAL = 0.5f;

	private float firstRecordTime_;

	public KartRecordHeader header_;

	public bool isLoaded_;

	private LinkedListNode<KartRecordElem> recordIter_;
}
