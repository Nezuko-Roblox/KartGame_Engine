using System;

public class KartDefine
{
	public const int MAP_NAME_LENGTH = 18;

	public const int USER_NAME_LENGTH = 10;

	public const bool IS_DEBUG = true;

	public const int MAX_DIFFICULTY = 3;

	public const float RACE_CUT_OFF_TIME = 600f;

	public static string[] CharacterAnimationString = new string[]
	{
		"idle", "turn_left", "turn_right", "turn_back_left", "turn_back_right", "look_back_left", "look_back_right", "boost", "small_accident", "big_accident",
		"attack", "item_success", "captured_bubble", "win_game", "lose_game", "win_game"
	};

	public static string[] KartAnimationString = new string[] { "idle", "slide_ani", "boom_ani", "bubble_ani" };

	public static string[] STAGE_SCNE_NAME = new string[]
	{
		string.Empty,
		"track_main",
		"track_single",
		"track_single",
		"track_wifi",
		"track_waitroom_host",
		"track_waitroom_client",
		"track_garage",
		"track_ccao",
		"track_multiplay",
		"track_info",
		"track_game",
		"track_between_stage",
		"track_loading",
		"track_store"
	};

	public static KartDefine.BetweenStageOption[] NEED_LOAING_SCENE = new KartDefine.BetweenStageOption[]
	{
		KartDefine.BetweenStageOption.NONE,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.GAME_LOADING,
		KartDefine.BetweenStageOption.GAME_LOADING,
		KartDefine.BetweenStageOption.DEFAULT_LOADING,
		KartDefine.BetweenStageOption.NONE,
		KartDefine.BetweenStageOption.NONE,
		KartDefine.BetweenStageOption.NONE,
		KartDefine.BetweenStageOption.DEFAULT_LOADING
	};

	public enum BetweenStageOption
	{
		NONE,
		DEFAULT_LOADING,
		GAME_LOADING
	}
}
