using System;
using System.IO;
using UnityEngine;

public class GoGhostKart : GoKart
{
	public GoGhostKart(string recordname, bool isResourceDirectory)
	{
		this.record_ = new KartRecord();
		try
		{
			if (isResourceDirectory)
			{
				if (!this.record_.LoadByResource(recordname, true))
				{
					this.record_ = null;
				}
			}
			else if (!this.record_.Load(recordname))
			{
				this.record_ = null;
			}
		}
		catch (FileNotFoundException ex)
		{
		}
	}

	public override void basicAction(float tick)
	{
		if (this.record_ == null)
		{
			return;
		}
		float num = tick - KartManager.Instance.DriveStartTime;
		if (KartManager.Instance.DriveStartTime <= 0f)
		{
			num = 0f;
		}
		KartRecordElem kartRecordElem;
		int record = this.record_.GetRecord(num, out kartRecordElem);
		if (record == -1)
		{
			if (!this.isFirstKartSet_)
			{
				this.firstRecord_ = new KartRecordElem(Mathf.Max(kartRecordElem.time_ - 5f, 0f), this.m_kart.transform.position, this.m_kart.transform.localRotation, 0);
				this.isFirstKartSet_ = true;
			}
			if (this.firstRecord_.time_ > num)
			{
				this.m_KartWLVel = Vector3.zero;
				this.position_ = this.firstRecord_.position_;
				this.rotation_ = this.firstRecord_.rotation_;
				base.CharacterAnim = CharacterAnimation.IDLE;
			}
			else
			{
				float num2 = (num - this.firstRecord_.time_) / (kartRecordElem.time_ - this.firstRecord_.time_);
				Vector3 vector = Vector3.Lerp(this.firstRecord_.position_, kartRecordElem.position_, num2);
				Quaternion quaternion = Quaternion.Slerp(this.firstRecord_.rotation_, kartRecordElem.rotation_, num2);
				this.m_KartWLVel = vector - this.position_;
				this.position_ = vector;
				this.rotation_ = quaternion;
				base.CharacterAnim = CharacterAnimation.IDLE;
			}
		}
		else
		{
			this.m_KartWLVel = kartRecordElem.position_ - this.position_;
			this.position_ = kartRecordElem.position_;
			this.rotation_ = kartRecordElem.rotation_;
			base.CharacterAnim = (CharacterAnimation)kartRecordElem.state_;
		}
		base.basicAction(tick);
	}

	public override void ResetForRestarting()
	{
		if (this.record_ == null)
		{
			return;
		}
		base.ResetForRestarting();
		this.record_.ResetForRestarting();
		this.position_ = Vector3.zero;
		this.rotation_ = Quaternion.identity;
	}

	public KartRecordHeader GetRecordHeader()
	{
		if (this.record_ == null)
		{
			return null;
		}
		return this.record_.header_;
	}

	private const float FIRST_PASS_PLANE_LIMIT_TIME = 5f;

	private KartRecord record_;

	public Vector3 position_ = Vector3.zero;

	public Quaternion rotation_ = Quaternion.identity;

	private bool isFirstKartSet_;

	private KartRecordElem firstRecord_;
}
