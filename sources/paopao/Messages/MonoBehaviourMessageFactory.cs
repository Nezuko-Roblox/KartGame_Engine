using System;
using UnityEngine;

public class MonoBehaviourMessageFactory
{
	public static MonoBehaviourMessageFactory Instance
	{
		get
		{
			if (MonoBehaviourMessageFactory.instance_ == null)
			{
				MonoBehaviourMessageFactory.instance_ = new MonoBehaviourMessageFactory();
			}
			return MonoBehaviourMessageFactory.instance_;
		}
	}

	public void Initialize()
	{
		this.message_ = new MonoBehaviourMessage[]
		{
			new BlackBarMessage(),
			new MonoBehaviourMessage1Param<bool>(MonoBehaviourMessageType.SHOW_UI),
			new ChangeCameraMessage(),
			new MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>(MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.GOAL_IN),
			new UpdateGUIItemSlotMessge(),
			new MonoBehaviourMessage(MonoBehaviourMessageType.RESET),
			new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.ENTER_USER_SECTION),
			new MonoBehaviourMessage2Param<KartBodyAnimation, KartAnimationOption>(MonoBehaviourMessageType.CHANGE_KART_ANIMATION),
			new MonoBehaviourMessage2Param<GameItem, ItemParam>(MonoBehaviourMessageType.ITEM),
			new WarpMessage(),
			new MonoBehaviourMessage2Param<GameItem, ApplyItemParam>(MonoBehaviourMessageType.APPLY_ITEM),
			new MonoBehaviourMessage2Param<bool, bool>(MonoBehaviourMessageType.SHOW_RESULT),
			new MonoBehaviourMessage1Param<GameItem>(MonoBehaviourMessageType.GET_ITEM),
			new MonoBehaviourMessage1Param<GameStageCommand>(MonoBehaviourMessageType.GAMESTAGE_COMMAND),
			new MonoBehaviourMessage(MonoBehaviourMessageType.PAUSE),
			new MonoBehaviourMessage(MonoBehaviourMessageType.RESUME),
			new MonoBehaviourMessage2Param<float, bool>(MonoBehaviourMessageType.NEW_RECORD),
			new MonoBehaviourMessage2Param<int, RankingType>(MonoBehaviourMessageType.SHOW_RANKING),
			new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.ITEM_TO_CTRL),
			new MonoBehaviourMessage1Param<GameParamPacket>(MonoBehaviourMessageType.UPDATE_WAITROOM),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.UPDATE_SHOPLIST),
			new MonoBehaviourMessage1Param<string>(MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO),
			new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.SIMPLE_MESSAGE),
			new MonoBehaviourMessage1Param<StageType>(MonoBehaviourMessageType.CHANGE_SCENE),
			new MonoBehaviourMessage2Param<int, Peer[]>(MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST),
			new MonoBehaviourMessage2Param<AssetType, int>(MonoBehaviourMessageType.SHOW_INFO),
			new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL),
			new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL2),
			new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL_MULTI),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.NETWORK_JOIN_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.FACEBOOK_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.WAITING_PLAYERS_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.PATCH_SUMMARY_POPUP_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.RESTORE_PURCHASES_POPUP_MESSAGE),
			new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.PURCHASE_CONFIRM_POPUP_MESSAGE),
			new MonoBehaviourMessage(MonoBehaviourMessageType.RACE_OVER),
			new MonoBehaviourMessage1Param<SoundController.FxType>(MonoBehaviourMessageType.PLAY_SOUND),
			new MonoBehaviourMessage(MonoBehaviourMessageType.UPDATE_RANKING)
		};
	}

	public bool IsInitialized()
	{
		return this.message_ != null;
	}

	public MonoBehaviourMessage GetMessage(MonoBehaviourMessageType type)
	{
		return this.message_[(int)type];
	}

	public static MonoBehaviourMessageFactory instance_;

	private MonoBehaviourMessage[] message_;
}
