using System;
using UnityEngine;

public class SimpleCameraControl : MonoBehaviour
{
	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		GoPlayKart goPlayKart_ = KartManager.Instance.goPlayKart_;
		GameObject kart = goPlayKart_.m_kart;
		if (goPlayKart_ == null)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogError("kart is null");
			}
			return;
		}
		float num = Time.deltaTime * 1000f;
		Vector3 position = kart.transform.position;
		float magnitude = goPlayKart_.m_kart.rigidbody.velocity.magnitude;
		Vector3 up = kart.transform.up;
		Vector3 forward = kart.transform.forward;
		Vector3 vector = kart.transform.right;
		if (up.y < 0.2f)
		{
			up.y = 0.2f;
			up.Normalize();
			vector = Vector3.Cross(up, forward);
		}
		Quaternion quaternion = MathHelper.ToQuaternion(vector, forward, up);
		if (!this.isInitialize_)
		{
			this.m_orientQua = quaternion;
			this.m_speed = magnitude;
			this.m_pos = position;
			this.m_fov = 75f;
		}
		else
		{
			if (Quaternion.Dot(this.m_orientQua, quaternion) < 0f)
			{
				MathHelper.QuaUnaryNegative(ref this.m_orientQua);
			}
			this.m_orientQua = Quaternion.Slerp(this.m_orientQua, quaternion, Mathf.Min(num / 400f, 1f));
			float num2 = ((this.m_speed <= magnitude) ? Mathf.Min(num / 10000f, 1f) : Mathf.Min(num / 100f, 1f));
			this.m_speed = this.m_speed * (1f - num2) + magnitude * num2;
			if (goPlayKart_.isRealBoost())
			{
				num2 = Mathf.Min(num / 1000f, 1f);
				this.m_fov = this.m_fov * (1f - num2) + 110f * num2;
			}
			else if (goPlayKart_.isZoneBoost())
			{
				num2 = Mathf.Min(num / 300f, 1f);
				this.m_fov = this.m_fov * (1f - num2) + 130f * num2;
			}
			else
			{
				num2 = Mathf.Min(num / 1500f, 1f);
				this.m_fov = this.m_fov * (1f - num2) + 75f * num2;
			}
		}
		base.transform.localRotation = this.m_orientQua;
		Vector3 up2 = base.transform.up;
		Vector3 forward2 = base.transform.forward;
		base.transform.Rotate((0.25f + Mathf.Max(0f, this.m_speed) / 400f) * 180f / 3.14159274f, 0f, 0f);
		float num3 = 5.5f;
		float num4 = 3f;
		float num5 = this.m_speed * 0.015f + num3;
		num5 += this.m_speed * 0.03f;
		if (num5 < num3)
		{
			num5 = num3;
		}
		Vector3 vector2 = position - forward2 * num5 + up2 * (num4 + this.m_speed / 60f);
		if (!this.isInitialize_)
		{
			this.m_pos = vector2;
		}
		else
		{
			this.m_pos.x = vector2.x;
			float num6 = Mathf.Min(num / 100f, 1f);
			this.m_pos.y = this.m_pos.y * (1f - num6) + vector2.y * num6;
			this.m_pos.z = vector2.z;
		}
		base.transform.position = this.m_pos;
		if (this.m_fov != base.camera.fieldOfView)
		{
			float num7 = 1.5f;
			if (this.m_fov < 28f)
			{
				num7 = 1.5f + (28f - this.m_fov);
			}
			base.camera.nearClipPlane = num7;
			base.camera.fieldOfView = this.m_fov;
		}
		this.isInitialize_ = true;
	}

	protected Quaternion m_orientQua;

	protected Vector3 m_pos;

	protected float m_fov = 75f;

	protected float m_speed;

	private bool isInitialize_;
}
