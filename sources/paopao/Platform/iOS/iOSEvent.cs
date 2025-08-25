using System;
using UnityEngine;

public static class iOSEvent
{
	private static void _Alert(string msg)
	{
	}

	public static void Alert(string msg)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			iOSEvent._Alert(msg);
		}
		else if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives");
			androidJavaClass.CallStatic<int>("showAlertDialog", new object[] { msg });
		}
	}

	public static void ShowExitPopup()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives");
			androidJavaClass.CallStatic<int>("showExitDialog", new object[0]);
		}
	}

	private static void _ShowIndicator()
	{
	}

	public static void ShowIndicator()
	{
	}

	private static void _HideIndicator()
	{
	}

	public static void HideIndicator()
	{
	}

	private static void _LockUserInteraction()
	{
	}

	public static void LockUserInteraction()
	{
	}

	private static void _UnlockUserInteraction()
	{
	}

	public static void UnlockUserInteraction()
	{
	}
}
