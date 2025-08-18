using System;
using UnityEngine;

public class BackgroundNotifier
{
	public static void WillResignActive()
	{
	}

	public static void DidEnterBackground()
	{
		if (StageController.IsInstantiated())
		{
			StageController.Instance.WillResignActive();
		}
		if (BackgroundNotifier.resign_ != null)
		{
			BackgroundNotifier.resign_();
		}
		KartManager.DidEnterBackground();
	}

	public static void DidBecomeActive()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			KartOptions.Instance.Bgm = !iOSUtil.IsIPodPlaying();
		}
		if (StageController.IsInstantiated())
		{
			StageController.Instance.DidBecomeActive();
		}
		if (BackgroundNotifier.active_ != null)
		{
			BackgroundNotifier.active_();
		}
	}

	public static void WillEnterForeground()
	{
		KartManager.WillEnterForeground();
	}

	public static BackgroundDelegate resign_;

	public static BackgroundDelegate active_;
}
