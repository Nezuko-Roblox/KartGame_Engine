using System;
using System.IO;
using UnityEngine;

public class GoAIKart : GoKart
{
	public GoAIKart(int kartIndex, string[] recordname, bool isRecordInAssetBundle)
	{
		if (KartManager.Instance.goCourse_.MaxLap != recordname.Length)
		{
		}
		this.kartIndex_ = kartIndex;
		this.record_ = new KartAIRecord[recordname.Length];
		for (int i = 0; i < recordname.Length; i++)
		{
			this.LoadRecord(recordname[i], out this.record_[i], isRecordInAssetBundle);
			this.record_[i].StandardTime = this.totalTime_;
			this.totalTime_ += this.record_[i].Last.Value.time_;
		}
		this.recordIndex_ = 0;
		this.currentRecord_ = this.record_[this.recordIndex_];
		this.isDefaultPath_ = false;
	}

	private void LoadRecord(string recordname, out KartAIRecord record, bool isRecordInAssetBundle)
	{
		if (isRecordInAssetBundle)
		{
			this.LoadRecordInAssetBundle(recordname, out record);
		}
		else
		{
			this.LoadRecordInDirectory(recordname, out record);
		}
	}

	private void LoadRecordInDirectory(string recordname, out KartAIRecord record)
	{
		record = new KartAIRecord();
		if (recordname.Contains(".txt"))
		{
			record.Load(recordname);
		}
		else
		{
			try
			{
				record.LoadBinaryFile(recordname);
			}
			catch (FileNotFoundException ex)
			{
			}
		}
	}

	private void LoadRecordInAssetBundle(string recordname, out KartAIRecord record)
	{
		string mainAsset = TrackAssetDefinitionManager.Instance.GetMainAsset((int)KartManager.Instance.parameter_.track_);
		BinaryAsset binaryAsset = (BinaryAsset)ResourceLoader.Instance.GetAsset(mainAsset, recordname);
		record = new KartAIRecord();
		try
		{
			record.Load(binaryAsset);
		}
		catch (FileNotFoundException ex)
		{
		}
	}

	public void InitFirstRecord()
	{
		this.InitFirstRecord(5f);
	}

	public void InitFirstRecord(float timeInterval)
	{
		if (this.currentRecord_ == null)
		{
			return;
		}
		this.firstRecord_ = new KartAIRecordElem(Mathf.Max(this.currentRecord_.First.Value.time_ - timeInterval, 0f), this.m_kart.transform.position, this.m_kart.transform.localRotation, 0, 0, 0f);
	}

	public override void basicAction(float tick)
	{
		float num = tick - KartManager.Instance.DriveStartTime;
		if (KartManager.Instance.DriveStartTime <= 0f)
		{
			num = 0f;
		}
		KartAIRecordElem kartAIRecordElem;
		int num2 = this.currentRecord_.GetRecord(num, out kartAIRecordElem);
		if (num2 == 1)
		{
			if (this.recordIndex_ >= this.record_.Length - 1)
			{
				this.position_ += this.m_KartWLVel;
				base.CharacterAnim = CharacterAnimation.IDLE;
				base.basicAction(tick);
				return;
			}
			this.recordIndex_++;
			this.isDefaultPath_ = false;
			this.currentRecord_ = this.record_[this.recordIndex_];
			this.InitFirstRecord(this.currentRecord_.First.Value.time_ - kartAIRecordElem.time_);
			num2 = this.currentRecord_.GetRecord(num, out kartAIRecordElem);
			if (num2 == 1)
			{
			}
		}
		if (num2 == -1)
		{
			if (this.firstRecord_.time_ > num)
			{
				this.m_KartWLVel = Vector3.zero;
				this.position_ = this.firstRecord_.position_;
				this.rotation_ = this.firstRecord_.rotation_;
				base.CharacterAnim = CharacterAnimation.IDLE;
			}
			else
			{
				float num3 = Mathf.Clamp01((num - this.firstRecord_.time_) / (kartAIRecordElem.time_ - this.firstRecord_.time_));
				Vector3 vector = Vector3.zero;
				if (this.recordIndex_ == 0)
				{
					vector = Vector3.Slerp(this.firstRecord_.position_, kartAIRecordElem.position_, num3 * num3);
				}
				else
				{
					vector = Vector3.Lerp(this.firstRecord_.position_, kartAIRecordElem.position_, num3);
				}
				Quaternion quaternion = Quaternion.Slerp(this.firstRecord_.rotation_, kartAIRecordElem.rotation_, num3);
				Vector3 vector2 = (kartAIRecordElem.position_ - this.firstRecord_.position_).normalized;
				Vector3 vector3 = quaternion * Vector3.up;
				Vector3 vector4 = Vector3.Cross(vector3, vector2);
				vector2 = Vector3.Cross(vector4, vector3);
				Quaternion quaternion2 = Quaternion.LookRotation(vector2, vector3);
				this.m_KartWLVel = vector - this.position_;
				this.position_ = vector;
				this.rotation_ = quaternion2;
				base.CharacterAnim = CharacterAnimation.IDLE;
			}
		}
		else
		{
			this.m_KartWLVel = kartAIRecordElem.position_ - this.position_;
			this.position_ = kartAIRecordElem.position_;
			this.rotation_ = kartAIRecordElem.rotation_;
			base.CharacterAnim = (CharacterAnimation)kartAIRecordElem.state_;
		}
		base.basicAction(tick);
	}

	public float ResetKartWithDefaultRecord(int distancePlane, float distance)
	{
		this.totalTime_ = 0f;
		for (int i = 0; i < this.record_.Length; i++)
		{
			if (i == this.recordIndex_)
			{
				this.defaultRecord_.StandardTime = this.totalTime_;
				this.totalTime_ += this.defaultRecord_.Last.Value.time_;
			}
			else
			{
				this.record_[i].StandardTime = this.totalTime_;
				this.totalTime_ += this.record_[i].Last.Value.time_;
			}
		}
		this.currentRecord_ = this.defaultRecord_;
		this.isDefaultPath_ = true;
		float tick = this.currentRecord_.GetTick(distancePlane, distance);
		this.ResetKart(tick + KartManager.Instance.DriveStartTime);
		return tick;
	}

	public void ResetKart(float resetTick)
	{
		this.m_KartWLVel = Vector3.zero;
		this.m_KartLAVel = Vector3.zero;
		this.characterAnim_ = CharacterAnimation.IDLE;
		this.kartbodyAnim_ = KartBodyAnimation.IDLE;
		this.currentRecord_.ResetRecordIter();
		this.basicAction(resetTick);
	}

	public override bool isRealBoost()
	{
		return false;
	}

	public override bool isZoneBoost()
	{
		return false;
	}

	public bool IsDefaultPath
	{
		get
		{
			return this.isDefaultPath_;
		}
	}

	public bool IsPassThoughUserSection(int userSection)
	{
		return (this.currentRecord_.GetUserSection() & (1 << userSection)) != 0;
	}

	public override string ToString()
	{
		string empty = string.Empty;
		return empty + "record : " + ((!this.IsDefaultPath) ? this.recordIndex_.ToString() : "default") + "\n";
	}

	public override void ResetForRestarting()
	{
		base.ResetForRestarting();
		for (int i = 0; i < this.record_.Length; i++)
		{
			this.record_[i].ResetForRestarting();
		}
		if (this.defaultRecord_ != null)
		{
			this.defaultRecord_.ResetForRestarting();
		}
		this.totalTime_ = 0f;
		for (int j = 0; j < this.record_.Length; j++)
		{
			this.record_[j].StandardTime = this.totalTime_;
			this.totalTime_ += this.record_[j].Last.Value.time_;
		}
		this.position_ = Vector3.zero;
		this.rotation_ = Quaternion.identity;
		this.recordIndex_ = 0;
		this.currentRecord_ = this.record_[this.recordIndex_];
		this.isDefaultPath_ = false;
	}

	public float TotalTime
	{
		get
		{
			return this.totalTime_;
		}
	}

	private const float FIRST_PASS_PLANE_LIMIT_TIME = 5f;

	private KartAIRecord[] record_;

	private KartAIRecord defaultRecord_;

	private KartAIRecord currentRecord_;

	public Vector3 position_ = Vector3.zero;

	public Quaternion rotation_ = Quaternion.identity;

	public int kartIndex_;

	private int recordIndex_;

	private float totalTime_;

	private bool isDefaultPath_;

	private KartAIRecordElem firstRecord_;
}
