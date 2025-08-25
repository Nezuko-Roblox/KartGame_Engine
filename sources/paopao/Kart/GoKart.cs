using System;
using UnityEngine;

public class GoKart
{
	public bool Stuck
	{
		get
		{
			return this.stuck_;
		}
		set
		{
			this.stuck_ = value;
		}
	}

	public bool Valid
	{
		get
		{
			return this.valid_;
		}
		set
		{
			if (this.valid_ != value)
			{
				this.valid_ = value;
			}
		}
	}

	public bool Forcing
	{
		get
		{
			return this.forcing_;
		}
		set
		{
			this.forcing_ = value;
		}
	}

	public bool IsInResetState
	{
		get
		{
			return this.isInResetState_;
		}
		set
		{
			this.isInResetState_ = value;
		}
	}

	public virtual void basicAction(float tick)
	{
	}

	public virtual void setReKart(KartBasicController controller, Transform[] wheels)
	{
		this.controller_ = controller;
		this.m_kart = controller.gameObject;
	}

	public virtual void Warp(Vector3 pos, Quaternion rot, bool flush, bool resetVel)
	{
		this.m_kart.transform.localPosition = pos;
		this.m_kart.transform.localRotation = rot;
		if (flush)
		{
		}
		if (resetVel)
		{
			this.m_KartWLVel = Vector3.zero;
			this.m_KartLAVel = Vector3.zero;
		}
	}

	public CharacterAnimation CharacterAnim
	{
		get
		{
			return this.characterAnim_;
		}
		set
		{
			this.characterAnim_ = value;
		}
	}

	public KartBodyAnimation KartBodyAnim
	{
		get
		{
			return this.kartbodyAnim_;
		}
		set
		{
			this.kartbodyAnim_ = value;
		}
	}

	public virtual bool isRealBoost()
	{
		return false;
	}

	public virtual bool isRealBoost(BoostKind kind)
	{
		return false;
	}

	public virtual bool isItemBoost()
	{
		return false;
	}

	public virtual bool isItemBoost(BoostKind kind)
	{
		return false;
	}

	public virtual bool isZoneBoost()
	{
		return false;
	}

	public virtual bool isZoneBoost(BoostKind kind)
	{
		return false;
	}

	public virtual bool isBoost(BoostKind kind)
	{
		return false;
	}

	public void SetStatus(GoKartStatus status, bool isOn)
	{
		if (isOn)
		{
			this.status_ |= (int)status;
		}
		else
		{
			this.status_ &= (int)(~(int)status);
		}
	}

	public bool IsStatusOn(GoKartStatus status)
	{
		return (this.status_ & (int)status) != 0;
	}

	public virtual void ResetForRestarting()
	{
		this.m_KartWLVel = Vector3.zero;
		this.m_KartLAVel = Vector3.zero;
		this.m_KartRealVelocity = Vector3.zero;
		this.stuck_ = false;
		this.valid_ = true;
		this.forcing_ = false;
		this.characterAnim_ = CharacterAnimation.IDLE;
		this.kartbodyAnim_ = KartBodyAnimation.IDLE;
		this.status_ = 0;
	}

	public GameObject m_kart;

	public KartBasicController controller_;

	public Vector3 m_KartWLVel;

	public Vector3 m_KartLAVel;

	public Vector3 m_KartRealVelocity;

	public bool stuck_;

	private bool valid_ = true;

	private bool forcing_;

	private bool isInResetState_;

	protected CharacterAnimation characterAnim_;

	protected KartBodyAnimation kartbodyAnim_;

	protected int status_;
}
