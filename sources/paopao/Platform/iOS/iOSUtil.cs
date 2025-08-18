using System;

public static class iOSUtil
{
	public static string Locale
	{
		get
		{
			if (iOSUtil.locale_ == null)
			{
				iOSUtil.locale_ = "ko";
			}
			return iOSUtil.locale_;
		}
	}

	public static bool IsIPodPlaying()
	{
		return false;
	}

	private const string DEFAULT_LOCALE = "ko";

	private static string[] SUPPORTED_LOCALE = new string[] { "ko", "en", "ca", "jp" };

	private static string locale_ = null;
}
