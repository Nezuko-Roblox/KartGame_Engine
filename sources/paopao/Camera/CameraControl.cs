using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(2);
		this.isInitialized_ = false;
		this.cameraManList_.Add("drive", new DriveCameraman());
		this.cameraManList_.Add("topview", new TopViewCameraman());
		if (base.animation != null)
		{
			base.animation.playAutomatically = false;
		}
	}

	private void SetCameraMan(ChangeCameraMessage msg)
	{
		this.isInitialized_ = true;
		if (base.animation != null)
		{
			base.animation.Stop();
		}
		base.transform.localPosition = Vector3.one;
		base.transform.localScale = Vector3.one;
		base.transform.localRotation = Quaternion.identity;
		if (this.cameraManList_.ContainsKey(msg.cameraName_))
		{
			base.transform.parent = null;
			this.cameraMan_ = this.cameraManList_[msg.cameraName_];
			this.cameraMan_.setKart(msg.kart_);
		}
		else
		{
			this.cameraMan_ = null;
			if (base.animation != null)
			{
				base.animation.wrapMode = msg.wrapMode_;
				base.animation.Play(msg.cameraName_);
			}
			base.transform.parent = msg.kart_.m_kart.transform;
		}
		base.camera.nearClipPlane = msg.nearPlane_;
		base.camera.farClipPlane = msg.farPlane_;
		base.camera.fieldOfView = msg.fov_;
	}

	private void Start()
	{
		if (!this.isInitialized_)
		{
			ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL);
			changeCameraMessage.Initialize("drive", KartManager.Instance.goPlayKart_);
			this.SetCameraMan(changeCameraMessage);
		}
	}

	private void FixedUpdate()
	{
		this.UpdateCameraPosition();
	}

	private void UpdateCameraPosition()
	{
		if (this.cameraMan_ != null)
		{
			Vector3 position = base.camera.transform.position;
			Transform transform = base.camera.transform;
			float fieldOfView = base.camera.fieldOfView;
			this.cameraMan_.calc((int)(Time.time * 1000f), ref position, ref transform, ref fieldOfView);
			base.camera.transform.position = position;
			if (fieldOfView != base.camera.fieldOfView)
			{
				base.camera.fieldOfView = fieldOfView;
			}
		}
	}

	private void Test()
	{
		float num = 744f;
		float num2 = 721f;
		float num3 = 80f;
		float num4 = 99f;
		Vector3[] array = new Vector3[]
		{
			new Vector3(num3, num, base.camera.nearClipPlane + 0.001f),
			new Vector3(num4, num, base.camera.nearClipPlane + 0.001f),
			new Vector3(num4, num2, base.camera.nearClipPlane + 0.001f),
			new Vector3(num3, num2, base.camera.nearClipPlane + 0.001f)
		};
		Vector3[] array2 = new Vector3[4];
		for (int i = 0; i < 4; i++)
		{
			array2[i] = base.camera.ScreenToWorldPoint(array[i]);
		}
		Debug.DrawLine(array2[0], array2[1]);
		Debug.DrawLine(array2[1], array2[2]);
		Debug.DrawLine(array2[2], array2[3]);
		Debug.DrawLine(array2[3], array2[0]);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL)
		{
			ChangeCameraMessage changeCameraMessage = (ChangeCameraMessage)msg;
			this.SetCameraMan(changeCameraMessage);
		}
		else if (msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			this.UpdateCameraPosition();
		}
	}

	private GameCameraman cameraMan_;

	private Dictionary<string, GameCameraman> cameraManList_ = new Dictionary<string, GameCameraman>();

	private bool isInitialized_;
}
