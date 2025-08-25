using System;
using UnityEngine;

public class ChangeCameraMessage : MonoBehaviourMessage
{
	public ChangeCameraMessage()
		: base(MonoBehaviourMessageType.CHANGE_CAMERA_CONTROL)
	{
	}

	public ChangeCameraMessage Initialize(string cameraName, GoKart kart)
	{
		this.cameraName_ = cameraName;
		this.kart_ = kart;
		return this;
	}

	public ChangeCameraMessage Initialize(string cameraName, GoKart kart, float nearPlane, float farPlane, float fov, WrapMode wrapMode)
	{
		this.cameraName_ = cameraName;
		this.kart_ = kart;
		this.nearPlane_ = nearPlane;
		this.farPlane_ = farPlane;
		this.fov_ = fov;
		this.wrapMode_ = wrapMode;
		return this;
	}

	public string cameraName_ = string.Empty;

	public float nearPlane_ = 0.01f;

	public float farPlane_ = 1800f;

	public float fov_ = 60f;

	public WrapMode wrapMode_;

	public GoKart kart_;
}
