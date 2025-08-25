using System;
using UnityEngine;

public class GoNetKart : GoKart
{
	public void SetPacket(GameKartPacket packet)
	{
		if (this.currPacket_ == null || packet.tick_ > this.currPacket_.tick_ || this.lastReceiveTime_ < Time.time - 0.5f)
		{
			this.pastPacket_ = this.currPacket_;
			this.currPacket_ = packet;
			base.CharacterAnim = packet.characterAnimation_;
			base.KartBodyAnim = packet.kartBodyAnimation_;
			this.lastReceiveTime_ = Time.time;
			this.rankValue_ = packet.rankValue_;
			this.apply_ = false;
			this.newPacket_ = true;
		}
	}

	public override void basicAction(float tick)
	{
		if (this.currPacket_ == null)
		{
			return;
		}
		if (this.newPacket_)
		{
			this.position_ = Vector3.zero;
			this.velocity_ = this.currPacket_.velocity_;
			if ((this.m_kart.rigidbody.position - this.currPacket_.position_).magnitude > 3f || this.pastPacket_ == null)
			{
				this.position_ = this.currPacket_.position_;
			}
			if (this.pastPacket_ != null)
			{
				Vector3 vector = (this.currPacket_.velocity_ - this.pastPacket_.velocity_) / 2f;
				float num = vector.magnitude / this.velocity_.magnitude;
				if ((double)num > 0.5)
				{
					vector = vector / num * 0.5f;
				}
				this.velocity_ += vector;
			}
			this.rotation_ = this.currPacket_.rotation_;
			this.apply_ = true;
			this.newPacket_ = false;
		}
		base.basicAction(tick);
	}

	public Vector3 position_;

	public Quaternion rotation_;

	public Vector3 force_;

	public Vector3 velocity_;

	public Vector3 acceleration_;

	public double rankValue_;

	protected float lastReceiveTime_;

	protected float lastBasicAction_;

	public GameKartPacket currPacket_;

	protected GameKartPacket pastPacket_;

	protected Quaternion rotateFrom_;

	protected Quaternion rotateTo_;

	protected float delta_;

	public bool apply_;

	public bool shielded_;

	protected bool newPacket_;
}
