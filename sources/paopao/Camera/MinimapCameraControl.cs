using System;
using UnityEngine;

public class MinimapCameraControl : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(13);
	}

	private void Start()
	{
		Rect[] array = new Rect[]
		{
			new Rect(630f + (float)Screen.width - 800f, (float)(Screen.height - 70 - 152), 162f, 152f),
			new Rect(818f, (float)(Screen.height - 356), 194f, 198f)
		};
		base.camera.pixelRect = array[(int)GUIBase.GetGUIType()];
		this.rotation_ = Quaternion.identity;
		this.prevTick_ = 0f;
	}

	private void Update()
	{
		if (this.prevTick_ <= 0f)
		{
			this.rotation_ = KartManager.Instance.goPlayKart_.m_kart.transform.localRotation;
			this.prevTick_ = Time.time;
		}
		Quaternion localRotation = KartManager.Instance.goPlayKart_.m_kart.transform.localRotation;
		Vector3 position = KartManager.Instance.goPlayKart_.m_kart.transform.position;
		Quaternion quaternion = localRotation;
		float num = Mathf.Clamp01((Time.time - this.prevTick_) / 1.5f);
		if (Quaternion.Dot(this.rotation_, quaternion) < 0f)
		{
			MathHelper.QuaUnaryNegative(ref this.rotation_);
		}
		this.rotation_ = Quaternion.Slerp(this.rotation_, quaternion, num);
		this.prevTick_ = Time.time;
		Vector3 vector = this.rotation_ * Vector3.forward;
		vector.y = 0f;
		vector.Normalize();
		Vector3 normalized = (vector * this.leanFactor_ - Vector3.up).normalized;
		base.transform.localRotation = Quaternion.LookRotation(normalized, Vector3.up);
		base.transform.position = position - normalized * this.minimapLevel_;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	private Quaternion rotation_ = Quaternion.identity;

	private float prevTick_;

	private float leanFactor_ = 1f;

	private float minimapLevel_ = 90f;
}
