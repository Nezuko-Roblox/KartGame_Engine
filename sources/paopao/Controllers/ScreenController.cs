using System;
using UnityEngine;

public class ScreenController : MonoBehaviourEx
{
	public static ScreenController Instance
	{
		get
		{
			if (ScreenController.instance_ == null)
			{
			}
			return ScreenController.instance_;
		}
	}

	public ScreenOrientation OriginalOrientation
	{
		get
		{
			return this.originalOrientation_;
		}
		set
		{
			this.originalOrientation_ = value;
		}
	}

	private void RotateAllCameras()
	{
		if (!this.enabled)
		{
			return;
		}
		foreach (Camera camera in Camera.allCameras)
		{
			Matrix4x4 matrix4x = camera.projectionMatrix;
			matrix4x *= Matrix4x4.Scale(new Vector3(-1f, -1f, 1f));
			camera.projectionMatrix = matrix4x;
			Rect pixelRect = camera.pixelRect;
			camera.pixelRect = new Rect((float)Screen.width - (pixelRect.xMin + pixelRect.width), (float)Screen.height - (pixelRect.yMin + pixelRect.height), pixelRect.width, pixelRect.height);
		}
	}

	public ScreenOrientation DisplayingOrientation
	{
		get
		{
			return this.displayingOrientation_;
		}
		set
		{
			if (this.displayingOrientation_ == value)
			{
				return;
			}
			this.displayingOrientation_ = value;
			Screen.orientation = this.displayingOrientation_;
			if (ScreenController.Instance.NeedToBeForced())
			{
				this.RotateAllCameras();
			}
		}
	}

	public bool NeedToBeForced()
	{
		return this.enabled && ((Application.platform == RuntimePlatform.Android && this.api_level < this.min_api) || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor);
	}

	public void RestoreDisplayingOrientation()
	{
		if (!this.enabled)
		{
			return;
		}
		if (ScreenController.Instance.IsDisplayingLandscapeRight())
		{
			this.RotateAllCameras();
		}
	}

	private void Start()
	{
		ScreenController.instance_ = this;
		global::UnityEngine.Object.DontDestroyOnLoad(ScreenController.instance_);
		this.originalOrientation_ = ScreenOrientation.LandscapeLeft;
		this.displayingOrientation_ = ScreenOrientation.LandscapeLeft;
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.Plugins.SDKVersion"))
			{
				this.api_level = androidJavaClass.CallStatic<int>("SDK_INT", new object[0]);
				this.min_api = androidJavaClass.CallStatic<int>("GINGERBREAD", new object[0]);
			}
		}
		this.enabled = false;
	}

	private void Update()
	{
		if (!this.enabled)
		{
			return;
		}
		if (KartManager.Instance.parameter_.Stage != StageType.GAME && KartManager.Instance.parameter_.Stage != StageType.GAME_WIFI)
		{
			if (Application.platform == RuntimePlatform.Android && this.api_level < this.min_api)
			{
				return;
			}
			this.shouldProcess = true;
			if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
			{
				if (ScreenController.Instance.DisplayingOrientation != ScreenOrientation.LandscapeLeft)
				{
					ScreenController.Instance.OriginalOrientation = ScreenOrientation.LandscapeLeft;
					iPhoneKeyboard.autorotateToLandscapeLeft = true;
					iPhoneKeyboard.autorotateToLandscapeRight = true;
				}
			}
			else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight && ScreenController.Instance.DisplayingOrientation != ScreenOrientation.LandscapeRight)
			{
				ScreenController.Instance.OriginalOrientation = ScreenOrientation.LandscapeRight;
				iPhoneKeyboard.autorotateToLandscapeLeft = true;
				iPhoneKeyboard.autorotateToLandscapeRight = true;
			}
		}
		else if (this.shouldProcess)
		{
			iPhoneKeyboard.autorotateToLandscapeRight = false;
			iPhoneKeyboard.autorotateToLandscapeLeft = false;
			this.shouldProcess = false;
		}
	}

	public bool IsDisplayingLandscapeLeft()
	{
		return ScreenController.Instance.DisplayingOrientation == ScreenOrientation.LandscapeLeft;
	}

	public bool IsDisplayingLandscapeRight()
	{
		return ScreenController.Instance.DisplayingOrientation == ScreenOrientation.LandscapeRight;
	}

	private new bool enabled;

	private static ScreenController instance_;

	private ScreenOrientation originalOrientation_;

	private ScreenOrientation displayingOrientation_;

	private int api_level;

	private int min_api;

	private bool shouldProcess = true;
}
