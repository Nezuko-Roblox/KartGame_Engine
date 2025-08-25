using System;
using UnityEngine;

public class GoSmoothNetKart : GoNetKart
{
	public override void ResetForRestarting()
	{
		base.ResetForRestarting();
		this.firstUpdate = true;
	}

	public override void basicAction(float tick)
	{
		if (Time.time > KartManager.Instance.DriveStartTime)
		{
		}
		float num = this.m_kart.rigidbody.velocity.y;
		if (num > 0f)
		{
			num = 0f;
		}
		this.m_kart.rigidbody.velocity = new Vector3(0f, num, 0f);
		if (this.currPacket_ != null)
		{
			if (this.firstUpdate)
			{
				this.firstUpdate = false;
				this.lastUpdateTime_ = tick;
			}
			bool flag = false;
			if (this.newPacket_)
			{
				this.newPacket_ = false;
				bool flag2 = this.currPacket_.velocity_.magnitude < 0.01f;
				this.currentVelocity_ = this.currPacket_.velocity_;
				this.rotation_ = this.currPacket_.rotation_;
				if (Mathf.Abs(this.currPacket_.position_.y - this.m_kart.transform.position.y) > 3f)
				{
					this.m_kart.transform.position = new Vector3(this.m_kart.transform.position.x, this.currPacket_.position_.y, this.m_kart.transform.position.z);
				}
				Vector3 vector = this.currPacket_.position_ - this.m_kart.transform.position;
				float num2 = Mathf.Max(4f, this.currentVelocity_.magnitude * 0.2f);
				if (flag2)
				{
					this.m_kart.transform.position = this.currPacket_.position_;
					this.refinedVelocity_ = this.currentVelocity_;
					flag = true;
				}
				else if (vector.magnitude > num2)
				{
					this.m_kart.transform.position = this.currPacket_.position_;
					this.refinedVelocity_ = this.currentVelocity_;
					flag = true;
				}
				else
				{
					Vector3 vector2 = vector / this.refineErrorTime_;
					this.refinedVelocity_ = this.currentVelocity_ + vector2;
				}
			}
			float num3 = tick - this.lastReceiveTime_;
			float num4 = tick - this.lastUpdateTime_;
			Vector3 vector3 = ((num3 <= this.refineErrorTime_) ? this.refinedVelocity_ : this.currentVelocity_);
			Vector3 vector4 = this.m_kart.transform.position;
			if (!flag)
			{
				vector4 += vector3 * num4;
			}
			this.position_ = vector4;
			this.velocity_ = vector3;
			this.apply_ = true;
			this.lastUpdateTime_ = tick;
			this.m_kart.transform.position = this.position_;
		}
	}

	private Vector3 currentVelocity_ = Vector3.zero;

	private Vector3 refinedVelocity_ = Vector3.zero;

	private float refineErrorTime_ = 1f;

	private float lastUpdateTime_;

	private bool firstUpdate = true;
}
