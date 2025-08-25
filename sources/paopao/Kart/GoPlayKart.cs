using System;
using UnityEngine;

public class GoPlayKart : GoKart
{
	public GoPlayKart()
	{
		this.m_theGravity = new Vector3(0f, -49f, 0f);
		this.m_NetWForce = Vector3.zero;
		this.m_NetLTorque = Vector3.zero;
		this.m_boostLeft = 0;
		this.m_slipBoost = false;
		this.m_Contact = true;
		this.m_spec.Initialize();
		this.m_sus.Initialize();
		this.m_drift.Initialize();
		this.m_driftGauge.Initialize();
		this.m_adBoost.Initialize();
		this.m_ctrl.Initialize();
		this.m_cState.Initialize();
		this.m_extern.Initialize();
		this.m_DriveFactor = new DriveFactor[3, 2];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				this.m_DriveFactor[i, j].Initialize();
			}
		}
		this.m_stuckHelper.Initialize();
		this.m_ort = Matrix3.CreateMtxIdentity();
		this.loadParam();
	}

	public void loadParam()
	{
		this.m_slipReserveTime = 0f;
		this.m_steer = (this.m_grip = 0f);
		this.m_slip[0] = 2f;
		this.m_slip[1] = 0.5f;
		this.m_DriveFactor[1, 0].speedLimit = 340f;
		this.m_DriveFactor[1, 0].betaCut = 0.6f;
		this.m_DriveFactor[1, 0].frontGripFactor = -1f;
		this.m_DriveFactor[1, 0].rearGripFactor = -1f;
		this.m_DriveFactor[1, 1].betaCut = 0.85f;
		this.m_DriveFactor[2, 0].speedLimit = 180f;
		this.m_DriveFactor[2, 0].betaCut = 0.6f;
		this.m_DriveFactor[2, 0].driftSlipFactor = 0.5f;
		this.m_DriveFactor[2, 1].frontGripFactor = -2f;
		this.m_DriveFactor[2, 1].rearGripFactor = -2f;
	}

	public void UpdateKartSpec(string levelParam, string bodyParam)
	{
		this.m_spec.Load(levelParam, bodyParam);
	}

	public override void basicAction(float tick)
	{
		this.m_isDrift = false;
		float fixedDeltaTime = Time.fixedDeltaTime;
		if (!base.Forcing && !base.IsInResetState)
		{
			if (this.m_boostLeft > 0)
			{
				this.m_boostLeft -= Mathf.Min(this.m_boostLeft, (int)(fixedDeltaTime * 1000f));
				if (this.m_boostLeft == 0)
				{
					this.m_BoostKind = BoostKind.NoBoost;
				}
			}
			this.processAdBoostTime(fixedDeltaTime);
			this.beginNetForce(fixedDeltaTime);
			bool flag = this.decideKartContact(fixedDeltaTime, false);
			if (flag)
			{
				this.calcNonpenetrateForce(fixedDeltaTime);
				this.calcKartTractionForce(fixedDeltaTime);
				this.calcKartSteeringForce(fixedDeltaTime);
			}
			else
			{
				this.calcFlyingKartForce();
			}
			this.calcResistForce();
			this.endNetForce(fixedDeltaTime);
			this.ProcessDriftGauge(fixedDeltaTime);
			this.m_isDrift = this.m_isDrift || this.m_drift.slipMode || this.m_drift.slipTime > 0f || this.m_drift.forceSlip;
		}
		else
		{
			this.m_KartWLVel = Vector3.zero;
			this.m_KartLAVel = Vector3.zero;
			this.m_NetWForce = Vector3.zero;
			this.m_NetLTorque = Vector3.zero;
			if (this.m_boostLeft > 0)
			{
				this.m_boostLeft -= Mathf.Min(this.m_boostLeft, (int)(fixedDeltaTime * 1000f));
				if (this.m_boostLeft == 0)
				{
					this.m_BoostKind = BoostKind.NoBoost;
				}
			}
		}
		base.basicAction(tick);
	}

	private void calcResistForce()
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		vector -= this.m_KartWLVel * this.m_spec.airFriction;
		vector2 -= this.m_KartLAVel * this.m_spec.airFriction;
		if (this.m_Contact)
		{
			vector -= this.m_KartWLVel * this.m_KartWLVel.magnitude * this.m_spec.dragFactor * this.m_extern.dragFactor * this.m_extern.compensationDragFactor;
		}
		this.m_NetWForce += vector;
		this.m_NetLTorque += vector2;
	}

	private void calcNonpenetrateForce(float deltaT)
	{
		for (uint num = 0U; num < 4U; num += 1U)
		{
			float num2;
			if (this.m_sus.wheelContact[(int)((UIntPtr)num)])
			{
				num2 = ((this.m_sus.deltaTravel[(int)((UIntPtr)num)] <= 0f) ? (this.m_spec.springK * this.m_sus.travel[(int)((UIntPtr)num)] + this.m_spec.damperRebC * (this.m_sus.deltaTravel[(int)((UIntPtr)num)] / deltaT)) : (this.m_spec.springK * this.m_sus.travel[(int)((UIntPtr)num)] + this.m_spec.damperCopC * (this.m_sus.deltaTravel[(int)((UIntPtr)num)] / deltaT)));
			}
			else
			{
				num2 = 0f;
			}
			num2 = ((num2 <= 0f) ? 0f : (Vector3.Dot(this.m_sus.wheelContactN[(int)((UIntPtr)num)], this.m_first.up) * num2));
			Vector3 vector = new Vector3(this.m_spec.width * this.m_sus.wheelOff[(int)((UIntPtr)num)].x, 0f, -this.m_spec.length * this.m_sus.wheelOff[(int)((UIntPtr)num)].y);
			Vector3 vector2 = Vector3.Cross(vector, new Vector3(0f, num2, 0f)) * 0.1f;
			this.m_NetLTorque += vector2;
		}
		this.m_NetWForce += this.m_theGravity * this.m_spec.mass * this.m_extern.gravityFactor * 0.8f;
	}

	private void calcKartTractionForce(float deltaT)
	{
		if (this.m_extern.slip)
		{
			return;
		}
		Vector3 vector = Vector3.Cross(this.m_first.left, this.m_sus.contactN);
		if (this.m_ctrl.getRealAccel() != 0f && !base.Stuck)
		{
			if (this.isRealBoost())
			{
				float num = 1.5f;
				if (this.m_BoostKind == BoostKind.BoostDrift)
				{
					num = 2.5f;
				}
				this.m_NetWForce += vector * this.m_ctrl.getRealAccel() * num * ((!this.m_drift.forceSlip) ? this.m_spec.forwardAccel : this.m_spec.driftEscapeForce);
			}
			else
			{
				this.m_NetWForce += vector * this.m_ctrl.getRealAccel() * ((!this.m_drift.forceSlip) ? this.m_spec.forwardAccel : this.m_spec.driftEscapeForce);
			}
			if (this.m_first.frontVel < 0f)
			{
				if (this.m_drift.slipMode || this.m_drift.forceSlip)
				{
					this.m_NetWForce += this.m_first.front * this.m_first.speed * this.m_spec.mass * 9.8f;
				}
				else
				{
					this.m_NetWForce += this.m_first.front * Mathf.Min(5f, this.m_first.speed) * this.m_spec.mass * 9.8f;
				}
			}
			this.m_ctrl.stayTime = 0f;
		}
		else if (this.m_ctrl.getRealBrake() != 0f || base.Stuck)
		{
			bool flag = true;
			if (this.m_first.frontVel < 0.5f)
			{
				this.m_ctrl.stayTime = this.m_ctrl.stayTime + deltaT;
				if (this.m_first.frontVel < -0.5f)
				{
					this.m_ctrl.stayTime = 1f;
				}
				if (this.m_ctrl.stayTime > 0.2f)
				{
					if (base.Stuck)
					{
						if (this.m_first.frontVel >= -0.5f)
						{
							this.m_KartWLVel = Vector3.zero;
							flag = false;
						}
					}
					else
					{
						this.m_NetWForce += vector * this.m_ctrl.getRealBrake() * -this.m_spec.backwardAccel;
						flag = false;
					}
				}
				else if (Mathf.Abs(this.m_first.leftVel) < 0.2f)
				{
					this.m_KartWLVel = Vector3.zero;
					flag = false;
				}
			}
			if (flag || (this.m_extern.speedLimit > 0f && this.m_KartWLVel.sqrMagnitude > 0f))
			{
				Vector3 vector2 = this.m_KartWLVel.normalized;
				vector2 -= Vector3.Dot(vector2, this.m_first.up) * this.m_first.up;
				if (Vector3.Dot(vector2, vector) > 0.8f)
				{
					this.m_NetWForce -= vector2 * this.m_spec.gripBrake;
				}
				else
				{
					this.m_NetWForce -= vector2 * this.m_spec.slipBrake;
				}
			}
		}
		else if (this.m_first.frontVel <= 0.5f && this.m_first.frontVel >= -0.5f)
		{
			this.m_ctrl.stayTime = this.m_ctrl.stayTime + deltaT;
		}
	}

	public float getSteerAngle()
	{
		return this.m_ctrl.steerAngle * 180f / 3.14159274f;
	}

	public float getRealSteer()
	{
		return this.m_ctrl.getRealSteer();
	}

	public float getRealAccel()
	{
		return this.m_ctrl.getRealAccel();
	}

	public float getRealBrake()
	{
		return this.m_ctrl.getRealBrake();
	}

	private void calcKartSteeringForce(float deltaT)
	{
		if (this.m_extern.slip)
		{
			return;
		}
		float num = Mathf.Sqrt(this.m_first.frontVel * this.m_first.frontVel + this.m_first.leftVel * this.m_first.leftVel);
		float num2 = ((this.m_first.frontVel <= 0f) ? (-1f) : 1f);
		this.m_ctrl.steerAngle = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad();
		this.m_ctrl.steerAngle = this.m_ctrl.steerAngle * Mathf.Exp(-Mathf.Abs(this.m_first.frontVel / this.m_spec.steerConstraint * this.m_extern.wheelFactor));
		this.isBackupStreer[0] = this.m_ctrl.getRealAccel() != 0f;
		this.isBackupStreer[1] = this.m_ctrl.oldSteerAngle * this.m_ctrl.steerAngle > 0f;
		this.isBackupStreer[2] = Mathf.Abs(this.m_ctrl.oldSteerAngle) < Mathf.Abs(this.m_ctrl.steerAngle);
		if (this.isBackupStreer[0] && this.isBackupStreer[1] && this.isBackupStreer[2])
		{
			if (!iOSController.Instance.analogSteer_)
			{
				this.m_ctrl.steerAngle = this.m_ctrl.oldSteerAngle;
			}
		}
		else
		{
			this.m_ctrl.oldSteerAngle = this.m_ctrl.steerAngle;
		}
		float num3 = 0.5f;
		float num4 = 0.5f;
		bool flag = false;
		uint num5 = 0U;
		bool flag2 = this.m_drift.slipMode || this.m_drift.forceSlip;
		this.m_drift.forceSlip = false;
		if (num > 5f)
		{
			float num6 = this.m_KartLAVel.y * num3 / num;
			float num7 = this.m_KartLAVel.y * num4 / num;
			float num8 = this.m_first.leftVel / num;
			bool flag3 = false;
			if (this.m_DriveMode > 0U && this.m_slipBoost && Mathf.Abs(num8) > this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 0].betaCut && this.m_adBoost.useLeftTime <= 0f)
			{
				flag = true;
				num5 = this.m_DriveMode;
				this.m_DriveMode = 0U;
				this.m_adBoost.validTime = 0f;
				this.m_adBoost.useLeftTime = 0.5f;
				this.m_BoostKind = BoostKind.BoostDrift;
				InGameStatistics.Instance.Others.DriftBooster++;
			}
			if (!this.m_drift.slipMode && !this.m_drift.trigger && Mathf.Abs(this.m_first.leftVel) > Mathf.Abs(this.m_first.frontVel) * 1.2f && num > 15f)
			{
				this.m_drift.forceSlip = true;
			}
			float num9 = 0f;
			float num10;
			float num11;
			if (this.m_drift.trigger)
			{
				this.m_steer = 0f;
				this.m_grip = 0f;
				num10 = 0f;
				num11 = -(9.8f * this.m_spec.mass) * this.m_spec.frontGripFactor * (this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * this.m_spec.driftTrigFactor);
				if (this.m_drift.triggerTime <= 0f)
				{
					this.m_drift.triggerTime = this.m_spec.driftTrigTime;
					this.m_drift.slipTime = this.m_drift.triggerTime * 2f;
				}
				else
				{
					this.m_drift.triggerTime = this.m_drift.triggerTime - deltaT;
					if (this.m_drift.triggerTime <= 0f)
					{
						this.m_drift.triggerTime = 0f;
						this.m_drift.trigger = false;
						if (!this.m_driftGauge.progressOn)
						{
							this.m_driftGauge.progressOn = true;
							this.m_driftGauge.progressTime = 0f;
							this.m_driftGauge.progress = 0f;
						}
					}
				}
			}
			else if (this.m_drift.slipMode || this.m_drift.slipTime > 0f || this.m_drift.forceSlip || this.m_slipReserveTime > 0f)
			{
				if (this.m_DriveMode == 1U)
				{
					float num12 = num / this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].speedLimit;
					float num13 = num12 * num12;
					if (num13 > 1f)
					{
						num13 = 1f;
					}
					float num14;
					float num15;
					float num16;
					if (this.m_drift.slipMode)
					{
						this.m_steer += 1E-06f * (1f - this.m_steer) * num13;
						this.m_grip += 0.005f * (1f - this.m_grip);
						this.m_NetWForce *= 1f - this.m_grip;
						num14 = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].onDriftSteerFactor;
						num15 = this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].frontGripFactor;
						num16 = this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].rearGripFactor;
					}
					else
					{
						this.m_steer += 0.001f * (1f - this.m_steer);
						this.m_grip = 0f;
						num14 = this.m_ctrl.steerAngle * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].onRestTimeSteerFactor;
						num15 = this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].backFrontGripFactor;
						num16 = this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].backRearGripFactor;
					}
					float num17 = this.m_spec.frontGripFactor + num15;
					float num18 = this.m_spec.rearGripFactor + num16;
					if (this.m_steer > 1f)
					{
						this.m_steer = 1f;
					}
					num18 -= this.m_grip;
					num10 = 9.8f * this.m_spec.mass * num17 * (num14 * num2 - num8 - num6);
					num11 = 9.8f * this.m_spec.mass * num18 * (-num8 + num7);
					num10 *= this.m_spec.driftSlipFactor * this.m_steer;
					num11 *= this.m_spec.driftSlipFactor * this.m_steer;
					this.m_slipReserveTime = Mathf.Max(this.m_slipReserveTime - deltaT, 0f);
				}
				else if (this.m_drift.slipMode)
				{
					num10 = 9.8f * this.m_spec.mass * (this.m_spec.frontGripFactor + this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].frontGripFactor) * (this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * num2 - num8 - num6);
					num11 = 9.8f * this.m_spec.mass * (this.m_spec.rearGripFactor + this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].rearGripFactor) * (-num8 + num7);
					num10 *= this.m_spec.driftSlipFactor * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].driftSlipFactor;
					num11 *= this.m_spec.driftSlipFactor * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 1].driftSlipFactor;
				}
				else
				{
					num10 = 9.8f * this.m_spec.mass * this.m_spec.frontGripFactor * (this.m_ctrl.steerAngle * num2 - num8 - num6);
					num11 = 9.8f * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
					num10 *= this.m_spec.driftSlipFactor;
					num11 *= this.m_spec.driftSlipFactor;
				}
				num9 = ((this.m_first.speed <= 10f) ? (-(num10 + num11) * this.m_spec.driftLeanFactor * 0.5f) : (-(num10 + num11) * this.m_spec.driftLeanFactor));
				this.m_drift.slipTime = Mathf.Max(this.m_drift.slipTime - deltaT, 0f);
			}
			else
			{
				flag3 = true;
				if (this.m_DriveMode > 0U)
				{
					float num19 = num / this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 0].speedLimit;
					float num20 = this.m_slip[0] * num19 * num19 + this.m_slip[1];
					if (num20 > 1f)
					{
						num20 = 1f;
					}
					float num21;
					if (this.m_DriveMode == 2U)
					{
						num21 = this.m_ctrl.getRealSteer() * this.m_spec.getMaxSteerRad() * num20;
					}
					else
					{
						num21 = this.m_ctrl.steerAngle * (1f + num20);
					}
					num10 = 9.8f * this.m_spec.mass * this.m_spec.frontGripFactor * (num21 * num2 - num8 - num6);
					num11 = 9.8f * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
					num10 *= this.m_spec.driftSlipFactor * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 0].driftSlipFactor;
					num11 *= this.m_spec.driftSlipFactor * this.m_DriveFactor[(int)((UIntPtr)this.m_DriveMode), 0].driftSlipFactor;
				}
				else
				{
					num10 = 9.8f * this.m_spec.mass * this.m_spec.frontGripFactor * (this.m_ctrl.steerAngle * num2 - num8 - num6);
					num11 = 9.8f * this.m_spec.mass * this.m_spec.rearGripFactor * (-num8 + num7);
				}
				num9 = -(num10 + num11) * this.m_spec.steerLeanFactor;
				if (this.m_driftGauge.progressOn)
				{
					this.m_driftGauge.gauge = Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
					this.m_driftGauge.progressOn = false;
					this.m_driftGauge.progressTime = 0f;
					this.m_driftGauge.lastProgress = this.m_driftGauge.progress;
					this.m_driftGauge.progress = 0f;
				}
			}
			this.m_NetWForce += this.m_ort * new Vector3(num10 + num11, 0f, (!flag3) ? 0f : (-Mathf.Abs(num10 + num11) * this.m_spec.cornerDrawFactor));
			this.m_NetLTorque += new Vector3(0f, num3 * num10 - num4 * num11, num9);
		}
		else
		{
			this.m_drift.slipMode = false;
			this.m_drift.slipTime = 0f;
			this.m_drift.trigger = false;
			this.m_drift.triggerTime = 0f;
			this.m_adBoost.validTrigger = false;
			if (this.m_driftGauge.progressOn)
			{
				this.m_driftGauge.gauge = Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
			}
			this.m_driftGauge.progressOn = false;
			this.m_driftGauge.progressTime = 0f;
			this.m_driftGauge.lastProgress = this.m_driftGauge.progress;
			this.m_driftGauge.progress = 0f;
			float num22 = this.m_KartLAVel.y * num3 / 5f;
			float num23 = this.m_KartLAVel.y * num4 / 5f;
			float num24 = this.m_first.leftVel / 5f;
			float num25 = 9.8f * this.m_spec.mass * this.m_spec.frontGripFactor * (((num >= 0.5f) ? (this.m_ctrl.steerAngle * num2) : 0f) - num24 - num22);
			float num26 = 9.8f * this.m_spec.mass * this.m_spec.rearGripFactor * (-num24 + num23);
			this.m_NetWForce += this.m_ort * new Vector3(num25 + num26, 0f, 0f);
			this.m_NetLTorque += new Vector3(0f, num3 * num25 - num4 * num26, 0f);
		}
		if (this.m_adBoost.validTrigger && flag2 && !this.m_drift.slipMode && !this.m_drift.forceSlip && this.m_adBoost.validTime == 0f)
		{
			this.m_adBoost.validTrigger = false;
			this.m_adBoost.validTime = 0.5f;
		}
		if (flag)
		{
			this.m_DriveMode = num5;
		}
	}

	public void setReKartOld(GameObject obj, Transform[] wheels)
	{
		this.m_kart = obj;
		this.wheelLocalPos_ = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			this.wheelLocalPos_[i] = wheels[i].localPosition;
			Vector3[] array = this.wheelLocalPos_;
			int num = i;
			array[num].x = array[num].x + 0.2f * ((i % 2 != 0) ? 1f : (-1f));
		}
		this.setDefaultSpec();
	}

	public override void setReKart(KartBasicController controller, Transform[] wheels)
	{
		base.setReKart(controller, wheels);
		this.wheelLocalPos_ = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			this.wheelLocalPos_[i] = wheels[i].localPosition;
			Vector3[] array = this.wheelLocalPos_;
			int num = i;
			array[num].x = array[num].x + 0.2f * ((i % 2 != 0) ? 1f : (-1f));
		}
		BoxCollider boxCollider = this.m_kart.transform.GetComponentInChildren(typeof(BoxCollider)) as BoxCollider;
		float x = boxCollider.size.x;
		float z = boxCollider.size.z;
		this.m_spec.width = Mathf.Min(0.98f, this.m_kart.transform.lossyScale.x * x * 0.5f);
		this.m_spec.length = this.m_kart.transform.lossyScale.z * z * 0.5f;
		this.setDefaultSpec();
	}

	private void setDefaultSpec()
	{
		this.m_spec.springK = this.m_spec.mass * Mathf.Abs(this.m_theGravity.y) * 0.5f;
		this.m_spec.damperCopC = 0f;
		this.m_spec.damperRebC = this.m_spec.springK * 0.2f;
		this.m_reciprocalMass = Vector3.zero;
		Vector3Helper.SetVector3(ref this.m_reciprocalMass, 12f / this.m_spec.mass);
	}

	private void beginNetForce(float deltaT)
	{
		this.m_NetWForce = this.m_extern.force;
		this.m_NetLTorque = this.m_extern.torque;
		this.m_extern.force = Vector3.zero;
		this.m_extern.torque = Vector3.zero;
		this.m_extern.liftVel = Vector3.zero;
		this.m_first.left = this.m_kart.transform.right;
		this.m_first.front = this.m_kart.transform.forward;
		this.m_first.up = this.m_kart.transform.up;
		this.m_first.frontVel = Vector3.Dot(this.m_KartWLVel, this.m_first.front);
		this.m_first.leftVel = Vector3.Dot(this.m_KartWLVel, this.m_first.left);
		this.m_first.upVel = Vector3.Dot(this.m_KartWLVel, this.m_first.up);
		this.m_first.speed = this.m_KartWLVel.magnitude;
		this.m_ort.setCol(this.m_first.left, this.m_first.up, this.m_first.front);
	}

	private void endNetForce(float deltaT)
	{
		this.m_NetWForce += this.m_extern.annexForce;
		this.m_KartWLVel += this.m_NetWForce / this.m_spec.mass * deltaT;
		this.m_KartLAVel += Vector3.Scale(this.m_reciprocalMass, (this.m_NetLTorque - Vector3.Cross(this.m_KartLAVel, Vector3.Scale(this.m_reciprocalMass, this.m_KartLAVel))) * deltaT);
	}

	public Vector3 GetWheelPos(int i)
	{
		return this.m_kart.transform.TransformPoint(this.wheelLocalPos_[i]);
	}

	public bool decideKartContact(float deltaT, bool wallInc)
	{
		this.m_cState.shock = this.m_Contact;
		this.m_Contact = false;
		if (this.m_kart == null)
		{
		}
		for (uint num = 0U; num < 4U; num += 1U)
		{
			Vector3 vector = this.m_kart.transform.position;
			vector += this.m_first.left * this.m_spec.width * this.m_sus.wheelOff[(int)((UIntPtr)num)].x * 0.8f;
			vector += this.m_first.front * this.m_spec.length * this.m_sus.wheelOff[(int)((UIntPtr)num)].y * 0.8f;
			vector += this.m_first.up * this.m_sus.maxTravel;
			Vector3 vector2 = vector + this.m_first.up;
			float num2 = this.m_sus.maxTravel * 2f + 1f;
			RaycastHit raycastHit;
			this.m_sus.wheelContact[(int)((UIntPtr)num)] = Physics.Raycast(vector2, -this.m_first.up, out raycastHit, num2, 256);
			if (this.m_sus.wheelContact[(int)((UIntPtr)num)])
			{
				this.m_Contact = true;
				this.m_sus.wheelContactN[(int)((UIntPtr)num)] = raycastHit.normal;
				float num3 = Vector3.Dot(raycastHit.point, this.m_first.up) - (Vector3.Dot(this.m_kart.transform.position, this.m_first.up) - this.m_sus.maxTravel);
				float num4 = this.m_sus.travel[(int)((UIntPtr)num)];
				this.m_sus.travel[(int)((UIntPtr)num)] = Mathf.Max(0f, Mathf.Min(num3, this.m_sus.maxTravel * 2f));
				this.m_sus.deltaTravel[(int)((UIntPtr)num)] = this.m_sus.travel[(int)((UIntPtr)num)] - num4;
			}
			else
			{
				this.m_sus.travel[(int)((UIntPtr)num)] = 0f;
				this.m_sus.deltaTravel[(int)((UIntPtr)num)] = 0f;
			}
			if (this.m_sus.wheelContact[(int)((UIntPtr)num)])
			{
			}
		}
		this.m_sus.contactN = Vector3.zero;
		if (this.m_Contact)
		{
			uint num5 = 0U;
			for (uint num6 = 0U; num6 < 4U; num6 += 1U)
			{
				if (this.m_sus.wheelContact[(int)((UIntPtr)num6)])
				{
					num5 += 1U;
					this.m_sus.contactN = this.m_sus.contactN + this.m_sus.wheelContactN[(int)((UIntPtr)num6)];
				}
			}
			this.m_sus.contactN = this.m_sus.contactN / num5;
			this.m_cState.shock = !this.m_cState.shock;
			if (num5 > 2U && this.m_cState.hop)
			{
				this.m_cState.hop = false;
			}
		}
		else
		{
			this.m_cState.shock = false;
		}
		return this.m_Contact;
	}

	public void setAccel(bool accel)
	{
		if (accel)
		{
			this.m_ctrl.accel = 1f;
			this.m_slipBoost = true;
			if (this.m_adBoost.validTime > 0f && (float)this.m_boostLeft < 0.5f)
			{
				this.m_adBoost.validTime = 0f;
				this.m_adBoost.useLeftTime = 0.5f;
				this.m_BoostKind = BoostKind.BoostDrift;
				InGameStatistics.Instance.Others.DriftBooster++;
			}
		}
		else
		{
			this.m_ctrl.accel = 0f;
			if (!this.isZoneBoost())
			{
				this.m_boostLeft = 0;
				this.m_BoostKind = BoostKind.NoBoost;
			}
		}
	}

	public void setBrake(bool brake)
	{
		if (brake)
		{
			this.m_ctrl.brake = 1f;
			if (this.m_ctrl.accelBrakeSwap && this.m_adBoost.validTime > 0f)
			{
				this.m_adBoost.validTime = 0f;
				this.m_adBoost.useLeftTime = 0.5f;
				this.m_BoostKind = BoostKind.BoostDrift;
				InGameStatistics.Instance.Others.DriftBooster++;
			}
		}
		else
		{
			this.m_ctrl.brake = 0f;
		}
	}

	public float Wheel
	{
		get
		{
			return this.m_ctrl.steer;
		}
		set
		{
			this.m_ctrl.steer = value;
		}
	}

	public void setDrift(bool drift)
	{
		if (drift)
		{
			if (this.m_drift.slipTime <= 0f)
			{
				this.m_drift.slipMode = true;
				this.m_drift.trigger = true;
				this.m_adBoost.validTrigger = this.m_first.frontVel > 0f;
			}
		}
		else
		{
			if (this.m_DriveMode == 1U && this.m_drift.slipMode && this.m_slipReserveTime <= 0f)
			{
				float num = this.m_first.speed / 120f;
				this.m_slipReserveTime = 10f * num * num;
			}
			this.m_drift.slipMode = false;
		}
	}

	private void calcFlyingKartForce()
	{
		this.m_drift.slipMode = false;
		this.m_drift.forceSlip = false;
		this.m_drift.trigger = false;
		this.m_NetWForce += this.m_theGravity * this.m_spec.mass * this.m_extern.gravityFactor;
		this.m_NetLTorque -= this.m_KartLAVel * 30f;
		this.m_ctrl.oldSteerAngle = 0f;
		if (this.m_cState.hop)
		{
			if (this.m_first.up.y < 0.05f)
			{
				this.m_NetLTorque.z = this.m_NetLTorque.z + ((this.m_first.left.y <= 0f) ? ((1f - this.m_first.up.y) * -90f) : ((1f - this.m_first.up.y) * 90f));
			}
			if (this.m_first.front.y > 0.5f)
			{
				this.m_NetLTorque.x = this.m_NetLTorque.x + (this.m_first.front.y + 1f) * 90f;
			}
			else if (this.m_first.front.y < -0.5f)
			{
				this.m_NetLTorque.x = this.m_NetLTorque.x - (1f - this.m_first.front.y) * 90f;
			}
		}
	}

	public void setBoost(int time, BoostKind kind)
	{
		if (kind == BoostKind.BoostNormal)
		{
			InGameStatistics.Instance.TotalItemUsage.IncreaseStat(GameItem.BOOSTER);
			InGameStatistics.Instance.EffectiveItemUsage.IncreaseStat(GameItem.BOOSTER);
		}
		if (this.isZoneBoost(kind) || this.m_ctrl.getRealAccel() != 0f)
		{
			this.m_boostLeft = time;
			this.m_BoostKind = kind;
		}
	}

	public void setBoostForce(int time, BoostKind kind)
	{
		this.m_boostLeft = time;
		this.m_BoostKind = kind;
	}

	public override bool isRealBoost()
	{
		return this.isRealBoost(this.m_BoostKind);
	}

	public override bool isRealBoost(BoostKind kind)
	{
		return kind == BoostKind.BoostStart || kind == BoostKind.BoostNormal || kind == BoostKind.BoostTeam || kind == BoostKind.BoostDrift || kind == BoostKind.BoostAnimal;
	}

	public override bool isItemBoost()
	{
		return this.isItemBoost(this.m_BoostKind);
	}

	public override bool isItemBoost(BoostKind kind)
	{
		return kind == BoostKind.BoostNormal || kind == BoostKind.BoostTeam || kind == BoostKind.BoostAnimal;
	}

	public override bool isZoneBoost()
	{
		return this.isZoneBoost(this.m_BoostKind);
	}

	public override bool isZoneBoost(BoostKind kind)
	{
		return kind == BoostKind.BoostZone || kind == BoostKind.BoostJumpZone || kind == BoostKind.BoostDelivery;
	}

	public override bool isBoost(BoostKind kind)
	{
		return kind == this.m_BoostKind;
	}

	private void calcBoostForce()
	{
	}

	private void processAdBoostTime(float deltaT)
	{
		if (this.m_adBoost.validTime > 0f)
		{
			this.m_adBoost.validTime = this.m_adBoost.validTime - deltaT;
			this.m_adBoost.validTime = Mathf.Max(0f, this.m_adBoost.validTime);
		}
		if (this.m_adBoost.useLeftTime > 0f)
		{
			this.m_adBoost.useLeftTime = this.m_adBoost.useLeftTime - deltaT;
			if (this.m_adBoost.useLeftTime <= 0f)
			{
				this.m_adBoost.useLeftTime = 0f;
				if (this.m_BoostKind == BoostKind.BoostDrift)
				{
					this.m_BoostKind = BoostKind.NoBoost;
				}
			}
		}
	}

	public float GetKartSpeed()
	{
		return this.m_KartWLVel.magnitude * 3.6f;
	}

	public float GetKartRealSpeed()
	{
		return this.m_KartRealVelocity.magnitude * 3.6f;
	}

	public void ResetCrash()
	{
		this.isCrash_ = false;
		this.crashVelocity_ = 0f;
	}

	public void SetCrash(float vel)
	{
		this.isCrash_ = true;
		if (this.crashVelocity_ <= vel)
		{
			this.crashVelocity_ = vel;
		}
	}

	public bool IsShock()
	{
		return this.isShock_;
	}

	public void ResetShock()
	{
		this.isShock_ = false;
		this.shockVelocity_ = 0f;
	}

	public void SetShock(float vel)
	{
		this.isShock_ = true;
		if (this.shockVelocity_ <= vel)
		{
			this.shockVelocity_ = vel;
		}
	}

	public void ResetKart()
	{
		this.m_KartWLVel = Vector3.zero;
		this.m_KartLAVel = Vector3.zero;
		this.m_NetWForce = Vector3.zero;
		this.m_NetLTorque = Vector3.zero;
		if (this.m_boostLeft > 0)
		{
			this.m_boostLeft = 0;
			this.m_BoostKind = BoostKind.NoBoost;
		}
	}

	public override void Warp(Vector3 pos, Quaternion rot, bool flush, bool resetVel)
	{
		base.Warp(pos, rot, flush, resetVel);
		this.m_NetWForce = Vector3.zero;
		this.m_NetLTorque = Vector3.zero;
		if (this.m_boostLeft > 0)
		{
			this.m_boostLeft = 0;
			this.m_BoostKind = BoostKind.NoBoost;
		}
	}

	public float GetDriftMaxGauge()
	{
		return this.m_spec.driftMaxGauge;
	}

	public float GetDriftGauge()
	{
		return this.m_driftGauge.gauge;
	}

	public float GetDriftGaugeProgress()
	{
		return Mathf.Min(this.m_spec.driftMaxGauge, this.m_driftGauge.gauge + this.m_driftGauge.progress);
	}

	public bool UseDriftGauge(float gauge)
	{
		if (gauge > this.m_driftGauge.gauge)
		{
			return false;
		}
		this.m_driftGauge.gauge = this.m_driftGauge.gauge - gauge;
		return true;
	}

	public float GetDriftLastProgress()
	{
		float lastProgress = this.m_driftGauge.lastProgress;
		this.m_driftGauge.lastProgress = 0f;
		return lastProgress;
	}

	private void ProcessDriftGauge(float deltaT)
	{
		if (this.m_driftGauge.progressOn && this.m_Contact && this.m_first.frontVel >= 0f)
		{
			this.m_driftGauge.progressTime = this.m_driftGauge.progressTime + deltaT;
			if (this.m_driftGauge.progressTime < 0.2f)
			{
				this.m_driftGauge.progress = this.m_driftGauge.progress + 2f * deltaT * this.m_first.leftVel * this.m_first.leftVel * 3f;
			}
			else if (this.m_driftGauge.progressTime < 0.5f)
			{
				this.m_driftGauge.progress = this.m_driftGauge.progress + 2f * deltaT * this.m_first.leftVel * this.m_first.leftVel * 1.5f;
			}
			else
			{
				this.m_driftGauge.progress = this.m_driftGauge.progress + 2f * (deltaT * this.m_first.leftVel * this.m_first.leftVel) / (this.m_driftGauge.progressTime * 2f);
			}
		}
	}

	public void ResetDriftGauge()
	{
		this.m_driftGauge.progressOn = false;
		this.m_driftGauge.progressTime = 0f;
		this.m_driftGauge.progress = 0f;
	}

	public bool IsAccel()
	{
		return this.m_ctrl.getRealAccel() > 0f;
	}

	public bool IsBrake()
	{
		return this.m_ctrl.getRealBrake() > 0f;
	}

	public int GetNormalBoosterTime()
	{
		return (this.m_spec.normalBoosterTime >= 0f) ? ((int)this.m_spec.normalBoosterTime) : 0;
	}

	public bool CheckStuck()
	{
		if (this.m_stuckHelper.inStuck)
		{
			this.m_stuckHelper.inStuck = false;
			return true;
		}
		return false;
	}

	public void AddWallStuckTime(bool reset, float deltaT)
	{
		if (reset)
		{
			this.m_stuckHelper.wallStuckTime = 0f;
		}
		else
		{
			this.m_stuckHelper.wallStuckTime = this.m_stuckHelper.wallStuckTime + deltaT;
			if (this.m_stuckHelper.wallStuckTime > 1f)
			{
				this.m_stuckHelper.inStuck = true;
			}
		}
	}

	public void AddGndStuckTime(bool reset, float deltaT)
	{
		if (reset)
		{
			this.m_stuckHelper.gndStuckTime = 0f;
		}
		else
		{
			this.m_stuckHelper.gndStuckTime = this.m_stuckHelper.gndStuckTime + deltaT;
			if (this.m_stuckHelper.gndStuckTime > 1f)
			{
				this.m_stuckHelper.inStuck = true;
			}
		}
	}

	public void AddObstStuckTime(bool reset, float deltaT)
	{
		if (reset)
		{
			this.m_stuckHelper.obstStuckTime = 0f;
		}
		else
		{
			this.m_stuckHelper.obstStuckTime = this.m_stuckHelper.obstStuckTime + deltaT;
			if (this.m_stuckHelper.obstStuckTime > 0.4f)
			{
				this.m_stuckHelper.inStuck = true;
			}
		}
	}

	public bool NeedReset
	{
		get
		{
			return this.needReset_;
		}
		set
		{
			this.needReset_ = value;
		}
	}

	public Vector3 BackupVelocity
	{
		get
		{
			return this.backupVelocity_;
		}
		set
		{
			this.backupVelocity_ = value;
		}
	}

	public float DragFactor
	{
		get
		{
			return this.m_extern.dragFactor;
		}
		set
		{
			this.m_extern.dragFactor = value;
		}
	}

	public float CompensationDragFactor
	{
		get
		{
			return this.m_extern.compensationDragFactor;
		}
		set
		{
			this.m_extern.compensationDragFactor = value;
		}
	}

	public override void ResetForRestarting()
	{
		base.ResetForRestarting();
		this.m_theGravity = new Vector3(0f, -49f, 0f);
		this.m_NetWForce = Vector3.zero;
		this.m_NetLTorque = Vector3.zero;
		this.m_KartRealVelocity = Vector3.zero;
		this.m_boostLeft = 0;
		this.m_slipBoost = false;
		this.m_Contact = true;
		this.m_sus.Initialize();
		this.m_drift.Initialize();
		this.m_driftGauge.Initialize();
		this.m_adBoost.Initialize();
		this.m_ctrl.Initialize();
		this.m_cState.Initialize();
		this.m_extern.Initialize();
		this.m_stuckHelper.Initialize();
		this.m_ort = Matrix3.CreateMtxIdentity();
		this.m_slipReserveTime = 0f;
		this.m_steer = (this.m_grip = 0f);
		this.m_slip[0] = 2f;
		this.m_slip[1] = 0.5f;
		this.m_isDrift = false;
		this.m_BoostKind = BoostKind.NoBoost;
		this.isCrash_ = false;
		this.crashVelocity_ = 0f;
		this.isShock_ = false;
		this.shockVelocity_ = 0f;
		this.needReset_ = false;
		this.backupVelocity_ = Vector3.zero;
	}

	private Matrix3 m_ort;

	private Vector3 m_reciprocalMass;

	private Vector3 m_theGravity;

	public Vector3 m_NetWForce;

	private Vector3 m_NetLTorque;

	private int m_boostLeft;

	private float m_slipReserveTime;

	private float m_steer;

	private float m_grip;

	private float[] m_slip = new float[2];

	private bool m_slipBoost;

	public PhysicSpec m_spec;

	public FirstPipelineValue m_first;

	public Suspension m_sus;

	public DriftControl m_drift;

	public DriftGauge m_driftGauge;

	public AdBoost m_adBoost;

	public Control m_ctrl;

	public CollisionState m_cState;

	private External m_extern;

	private DriveFactor[,] m_DriveFactor;

	private StuckHelper m_stuckHelper;

	public bool m_Contact;

	public bool m_isDrift;

	private BoostKind m_BoostKind;

	private uint m_DriveMode;

	public Vector3[] wheelLocalPos_;

	public bool isCrash_;

	public float crashVelocity_;

	public bool isShock_;

	public float shockVelocity_;

	private bool needReset_;

	private Vector3 backupVelocity_ = Vector3.zero;

	public bool[] isBackupStreer = new bool[3];
}
