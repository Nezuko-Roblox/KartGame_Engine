using System;
using UnityEngine;

public class CameraManager
{
	public static CameraManager Instance
	{
		get
		{
			if (CameraManager.instance_ == null)
			{
				CameraManager.instance_ = new CameraManager();
			}
			return CameraManager.instance_;
		}
	}

	public void Initialize()
	{
		global::UnityEngine.Object[] array = global::UnityEngine.Object.FindObjectsOfType(typeof(Camera));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == "gui_camera")
			{
				this.guiCam_ = (Camera)array[i];
			}
			else if (array[i].name == "main_camera")
			{
				this.mainCam_ = (Camera)array[i];
			}
		}
	}

	public static CameraManager instance_;

	public Camera mainCam_;

	public Camera guiCam_;
}
