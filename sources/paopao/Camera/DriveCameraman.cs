using System;
using UnityEngine;

public class DriveCameraman : GameCameraman
{
	public DriveCameraman()
	{
		this.reset(0);
	}

	~DriveCameraman()
	{
	}

	public void debugging()
	{
	}

	public override void reset(int style)
	{
		this.m_state = DriveCameraman.State.RESET;
		this.m_style = (DriveCameraman.Style)style;
	}

	public override int getStyle()
	{
		return (int)this.m_style;
	}

	public static float StaticConvertFov(float hFov)
	{
		return hFov * 0.75f;
	}

	public static float DynamicConvertFov(float hFov)
	{
		float num = (float)Screen.height / (float)Screen.width;
		return hFov * num;
	}

	public override void calc(int tick, ref Vector3 retPos, ref Transform retOrt, ref float retFov)
	{
		base.calc(tick, ref retPos, ref retOrt, ref retFov);
		GoKart kart = base.getKart();
		GameObject kart2 = kart.m_kart;
		if (kart == null)
		{
			return;
		}
		int elapse = base.getElapse();
		Vector3 position = kart.controller_.KartBodyTransform.position;
		float magnitude = kart.m_KartWLVel.magnitude;
		Vector3 up = kart2.transform.up;
		Vector3 forward = kart2.transform.forward;
		Vector3 vector = kart2.transform.right;
		if (up.y < 0.2f)
		{
			up.y = 0.2f;
			up.Normalize();
			vector = Vector3.Cross(up, forward);
		}
		Quaternion quaternion = MathHelper.ToQuaternion(vector, forward, up);
		DriveCameraman.State state = this.m_state;
		if (state != DriveCameraman.State.RESET)
		{
			if (state == DriveCameraman.State.NORMAL)
			{
				if (Quaternion.Dot(this.m_orientQua, quaternion) < 0f)
				{
					MathHelper.QuaUnaryNegative(ref this.m_orientQua);
				}
				this.m_orientQua = Quaternion.Slerp(this.m_orientQua, quaternion, Mathf.Min((float)elapse / 400f, 1f));
				float num = ((this.m_speed <= magnitude) ? Mathf.Min((float)elapse / 10000f, 1f) : Mathf.Min((float)elapse / 100f, 1f));
				this.m_speed = this.m_speed * (1f - num) + magnitude * num;
				if (kart.isRealBoost())
				{
					num = Mathf.Min((float)elapse / 1000f, 1f);
					this.m_fov = Mathf.Lerp(this.m_fov, 100f, num);
				}
				else if (kart.isZoneBoost())
				{
					num = Mathf.Min((float)elapse / 300f, 1f);
					this.m_fov = Mathf.Lerp(this.m_fov, DriveCameraman.StaticConvertFov(130f), num);
				}
				else
				{
					num = Mathf.Min((float)elapse / 1500f, 1f);
					this.m_fov = Mathf.Lerp(this.m_fov, 60f, num);
				}
			}
		}
		else
		{
			this.m_orientQua = quaternion;
			this.m_speed = magnitude;
			this.m_pos = position;
			this.m_fov = 60f;
		}
		retOrt.localRotation = this.m_orientQua;
		Vector3 up2 = retOrt.up;
		Vector3 forward2 = retOrt.forward;
		retOrt.Rotate((0.25f + Mathf.Max(0f, this.m_speed) / 400f) * 180f / 3.14159274f, 0f, 0f);
		float num2;
		float num3;
		if (this.m_style == DriveCameraman.Style.HIGH)
		{
			num2 = 5.5f;
			num3 = 3f;
		}
		else
		{
			num2 = 4f;
			num3 = 2.2f;
		}
		float num4 = this.m_speed * 0.015f + num2;
		num4 += this.m_speed * 0.03f;
		if (num4 < num2)
		{
			num4 = num2;
		}
		Vector3 vector2 = position - forward2 * num4 + up2 * (num3 + this.m_speed / 60f);
		if (this.m_state == DriveCameraman.State.RESET)
		{
			this.m_pos = vector2;
		}
		else
		{
			this.m_pos.x = vector2.x;
			float num5 = Mathf.Min((float)elapse / 100f, 1f);
			this.m_pos.y = this.m_pos.y * (1f - num5) + vector2.y * num5;
			this.m_pos.z = vector2.z;
		}
		retPos = this.m_pos;
		retFov = this.m_fov;
		this.m_state = DriveCameraman.State.NORMAL;
	}

	public override string getDescription()
	{
		return "Drive Cameraman";
	}

	protected DriveCameraman.State m_state;

	protected DriveCameraman.Style m_style;

	protected float m_speed;

	protected Quaternion m_orientQua;

	protected Vector3 m_pos;

	protected float m_fov;

	public enum Style
	{
		HIGH,
		LOW
	}

	public enum State
	{
		RESET,
		NORMAL
	}
}
