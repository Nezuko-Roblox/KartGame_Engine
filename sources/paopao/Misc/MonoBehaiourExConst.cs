using System;

public class MonoBehaiourExConst
{
	public static void Initialize()
	{
		MonoBehaiourExConst.PLAYER_KART = 24 + KartManager.PLAYER_KART_IDX;
	}

	public const int NULL = 0;

	public const int GAME_STAGE = 1;

	public const int MAIN_CAMERA = 2;

	public const int ITEM_SLOT = 3;

	public const int SOUND_CONTROLLER = 5;

	public const int TACHOMETER = 6;

	public const int RESULT = 7;

	public const int WRONG_WAY = 12;

	public const int MINIMAP_CAMERA = 13;

	public const int BLACK_BAR = 14;

	public const int BOOSTER_GAUGE = 15;

	public const int GUI_MINIMAP = 16;

	public const int GUI_MINIMAP_PANEL = 17;

	public const int GUI_NEW_RECORD = 18;

	public const int GUI_PAUSE = 19;

	public const int GUI_CONTROLS = 20;

	public const int GUI_QUIT_POPUP = 21;

	public const int GUI_LOADING_POPUP = 22;

	public const int GUI_FB_LOGIN_POPUP = 23;

	public const int KART = 24;

	public const int MINIMAP_MARK_0 = 32;

	public const int ITEM = 40;

	public const int GUI_ATLAS_CREATOR = 256;

	public const int STAGE_CONTROLLER = 264;

	public const int GUI_ACCOMPLISH_POPUP = 265;

	public const int GUI_TROPHY_POPUP = 266;

	public const int GUI_RANKING_RESET_POPUP = 267;

	public const int GUI_WAITING_PLAYERS_POPUP = 268;

	public const int GUI_MODE = 269;

	public const int GUI_PATCH_SUMMARY_POPUP = 270;

	public const int GUI_RESTORE_PURCHASES_POPUP = 271;

	public const int GUI_PURCHASE_CONFIRM_POPUP = 272;

	public const int STAGE_LOADING = 512;

	public const int STAGE_SINGLEMODE = 513;

	public const int STAGE_WIFI = 514;

	public const int STAGE_WAITROOM = 515;

	public const int STAGE_MAIN_MENU = 516;

	public const int STAGE_GARAGE = 517;

	public const int STAGE_INFO = 518;

	public const int STAGE_INTER_STAGE = 519;

	public const int STAGE_STORE = 520;

	public const int GUI_LOADING = 1024;

	public const int GUI_TRACKLIST = 1025;

	public const int GUI_RANKING = 1026;

	public const int GUI_RANKINGLIST = 1027;

	public const int GUI_RANKINGBG = 1028;

	public const int GUI_WAITROOM_HOST = 1032;

	public const int GUI_WAITROOM_TRACKLIST = 1033;

	public const int GUI_WAITROOM_CLIENT = 1034;

	public const int GUI_STORE_PURCHASE_BUTTON = 1045;

	public const int GUI_STORE_ITEM_INFO = 1046;

	public const int GUI_STORELIST = 1047;

	public const int GUI_GARAGE_SHOPLIST = 1048;

	public const int GUI_GARAGE_MODE = 1049;

	public const int GUI_QUEST_POPUP = 1050;

	public const int GUI_PACKAGE_INFO = 1051;

	public const int GUI_WIFI_ROOMLIST = 1064;

	public const int GUI_WIFI = 1065;

	public const int GUI_TUTORIAL = 1072;

	public const int GUI_TUTORIAL2 = 1073;

	public const int GUI_TUTORIAL_MULTI = 1074;

	public static int PLAYER_KART;
}
