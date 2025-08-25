using System;

public class StaticOption
{
	public static string GetTrackName()
	{
		return StaticOption.TRACKNAME_DESKTOP[StaticOption.trackIndex_];
	}

	public static void IncreaseTrackIndex()
	{
		StaticOption.trackIndex_ = (StaticOption.trackIndex_ + 1) % StaticOption.TRACKNAME_DESKTOP.Length;
	}

	public static string GetLoadStageName()
	{
		return "track_loader";
	}

	private static string[] TRACKNAME_DESKTOP = new string[] { "track_ccao", "track_screenshot" };

	private static int trackIndex_ = 0;

	public static int startPosition_ = 0;
}
