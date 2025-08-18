using System;
using UnityEngine;

public class KartOptions
{
	public static KartOptions Instance
	{
		get
		{
			if (KartOptions.instance_ == null)
			{
				KartOptions.instance_ = new KartOptions();
			}
			return KartOptions.instance_;
		}
	}

	private string GetKey(KartOptions.RegistryKey key)
	{
		return key.ToString();
	}

	private string GetKey(KartOptions.RegistryKey key, int idx)
	{
		if (key != KartOptions.RegistryKey.TRACK_INFO)
		{
			return string.Empty;
		}
		return string.Format("{0}_{1}", key.ToString(), idx);
	}

	private int GetRegistryInt(KartOptions.RegistryKey key)
	{
		if (!this.registryOthers_[(int)key].isReadRegistry_)
		{
			this.registryOthers_[(int)key].ReadRegistry(this.GetKey(key));
		}
		return ((RegistryInt)this.registryOthers_[(int)key]).Value;
	}

	private void SetRegistryInt(KartOptions.RegistryKey key, int value)
	{
		((RegistryInt)this.registryOthers_[(int)key]).Value = value;
	}

	private string GetRegistryString(KartOptions.RegistryKey key)
	{
		if (!this.registryOthers_[(int)key].isReadRegistry_)
		{
			this.registryOthers_[(int)key].ReadRegistry(this.GetKey(key));
		}
		return ((RegistryString)this.registryOthers_[(int)key]).Value;
	}

	private void SetRegistryString(KartOptions.RegistryKey key, string value)
	{
		((RegistryString)this.registryOthers_[(int)key]).Value = value;
	}

	private RegistryTrack GetRegistryTrack(int trackIdx)
	{
		if (this.registryTrack_ == null || this.registryTrack_[trackIdx] == null)
		{
			return null;
		}
		if (!this.registryTrack_[trackIdx].isReadRegistry_)
		{
			this.registryTrack_[trackIdx].ReadRegistry(this.GetKey(KartOptions.RegistryKey.TRACK_INFO, trackIdx));
		}
		return this.registryTrack_[trackIdx];
	}

	public void LoadRegistry()
	{
		if (this.registryTrack_ == null)
		{
			this.registryTrack_ = new RegistryTrack[TrackAssetDefinitionManager.Instance.GetAssetCount()];
			for (int i = 0; i < this.registryTrack_.Length; i++)
			{
				this.registryTrack_[i] = new RegistryTrack();
			}
		}
		int registryInt = this.GetRegistryInt(KartOptions.RegistryKey.VERSION);
		if (registryInt != this.VERSION_)
		{
			PlayerPrefs.DeleteAll();
			this.SetRegistryInt(KartOptions.RegistryKey.VERSION, this.VERSION_);
		}
		else
		{
			this.GetRegistryString(KartOptions.RegistryKey.DEFAULT_USER_NAME);
			this.GetRegistryInt(KartOptions.RegistryKey.QUEST);
			this.GetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_0);
			this.GetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_1);
			this.GetRegistryString(KartOptions.RegistryKey.RACE);
			for (int j = 0; j < this.registryTrack_.Length; j++)
			{
				this.GetRegistryTrack(j);
			}
			for (int k = 6; k < 11; k++)
			{
				this.registryOthers_[k].ReadRegistry(this.GetKey((KartOptions.RegistryKey)k));
			}
		}
		this.fbUserName_ = string.Empty;
	}

	public void ResetRegistry()
	{
		PlayerPrefs.DeleteAll();
		this.SetRegistryInt(KartOptions.RegistryKey.VERSION, this.VERSION_);
		if (this.registryOthers_ != null)
		{
			for (int i = 0; i < this.registryOthers_.Length; i++)
			{
				this.registryOthers_[i].Reset();
			}
		}
		if (this.registryTrack_ != null)
		{
			for (int j = 0; j < this.registryTrack_.Length; j++)
			{
				this.registryTrack_[j].Reset();
			}
		}
	}

	public void SaveRegistry()
	{
		for (int i = 0; i < this.registryTrack_.Length; i++)
		{
			if (this.registryTrack_[i] != null)
			{
				this.registryTrack_[i].SaveRegistry(this.GetKey(KartOptions.RegistryKey.TRACK_INFO, i));
			}
		}
		for (int j = 0; j < 11; j++)
		{
			if (this.registryOthers_[j] != null)
			{
				this.registryOthers_[j].SaveRegistry(this.GetKey((KartOptions.RegistryKey)j));
			}
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < 11; i++)
		{
			if (this.registryOthers_[i] != null)
			{
				text += string.Format("{0} : {1}\n", (KartOptions.RegistryKey)i, this.registryOthers_[i].ToString());
			}
		}
		if (this.registryTrack_ == null)
		{
			text += "track info is null";
		}
		else
		{
			text += string.Format("track info [{0}] \n", this.registryTrack_.Length);
			for (int j = 0; j < this.registryTrack_.Length; j++)
			{
				if (this.registryTrack_[j] != null)
				{
					text = text + this.registryTrack_.ToString() + "\n";
				}
				else
				{
					text += "<null>\n";
				}
			}
		}
		return text;
	}

	public bool Bgm
	{
		get
		{
			return this.bgm_;
		}
		set
		{
			this.bgm_ = value;
		}
	}

	public bool Fx
	{
		get
		{
			return this.fx_;
		}
		set
		{
			this.fx_ = value;
		}
	}

	public int Controller
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.CONTROLLER);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.CONTROLLER, value);
		}
	}

	public int Character
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.CHARACTER);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.CHARACTER, value);
		}
	}

	public int Kart
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.KART);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.KART, value);
		}
	}

	public int SelectItemTrack
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.LAST_SELECTED_ITEM_TRACK);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.LAST_SELECTED_ITEM_TRACK, value);
		}
	}

	public int SelectSpeedTrack
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.LAST_SELECTED_SPEED_TRACK);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.LAST_SELECTED_SPEED_TRACK, value);
		}
	}

	public GhostFilenameInfo GetBestInfo(byte trackIdx, int mode)
	{
		return this.GetRegistryTrack((int)trackIdx).GetBestInfo(mode);
	}

	public void SetBestInfo(GhostFilenameInfo bestInfo, int mode)
	{
		this.GetRegistryTrack((int)bestInfo.trackIdx_).SetBestInfo(mode, bestInfo);
	}

	public int[] GetRaceCounter(byte trackIdx)
	{
		return this.GetRegistryTrack((int)trackIdx).RaceCounter;
	}

	public int[] GetWinCounter(byte trackIdx)
	{
		return this.GetRegistryTrack((int)trackIdx).WinCounter;
	}

	public void SetRaceCounter(byte trackIdx, int[] counter)
	{
		this.GetRegistryTrack((int)trackIdx).RaceCounter = counter;
	}

	public void SetWinCounter(byte trackIdx, int[] counter)
	{
		this.GetRegistryTrack((int)trackIdx).WinCounter = counter;
	}

	public string DefaultUserName
	{
		get
		{
			return this.GetRegistryString(KartOptions.RegistryKey.DEFAULT_USER_NAME);
		}
		set
		{
			this.SetRegistryString(KartOptions.RegistryKey.DEFAULT_USER_NAME, value);
		}
	}

	public string LastUserName
	{
		get
		{
			Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
			if (facebook.LoggedIn && this.fbUserName_ == string.Empty)
			{
				this.fbUserName_ = FiaUtil.GenerateUserName(facebook.UserName);
			}
			return (!facebook.LoggedIn) ? this.DefaultUserName : this.fbUserName_;
		}
	}

	public bool IsDefaultUserNameSet()
	{
		string defaultUserName = this.DefaultUserName;
		return defaultUserName != null && defaultUserName != string.Empty;
	}

	public int Quest
	{
		get
		{
			return this.GetRegistryInt(KartOptions.RegistryKey.QUEST);
		}
		set
		{
			this.SetRegistryInt(KartOptions.RegistryKey.QUEST, value);
		}
	}

	public bool IsQuestFlagOn(KartOptions.QuestFlag f)
	{
		return (this.Quest & (int)f) != 0;
	}

	public void UpdateQuestFlag()
	{
		Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
		if (!this.IsQuestFlagOn(KartOptions.QuestFlag.FACEBOOK_LOGIN) && facebook.LoggedIn)
		{
			this.SetQuestFlag(KartOptions.QuestFlag.FACEBOOK_LOGIN, true);
		}
		if (!this.IsQuestFlagOn(KartOptions.QuestFlag.FACEBOOK_FRIEND) && facebook.LoggedIn && facebook.FriendDict.Count >= 4)
		{
			this.SetQuestFlag(KartOptions.QuestFlag.FACEBOOK_FRIEND, true);
		}
	}

	public KartOptions.QuestFlag MapTextureIndexToQuestFlag(int index)
	{
		if (index > 7)
		{
			index -= 8;
			return (KartOptions.QuestFlag)(1048576 << index);
		}
		return (KartOptions.QuestFlag)(256 << index);
	}

	public void SetQuestFlag(KartOptions.QuestFlag f, bool on)
	{
		int num = this.Quest;
		if (on)
		{
			num |= (int)f;
		}
		else
		{
			num &= (int)(~(int)f);
		}
		this.Quest = num;
	}

	public long GameQuest
	{
		get
		{
			int registryInt = this.GetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_0);
			int registryInt2 = this.GetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_1);
			return (long)(registryInt + registryInt2);
		}
		set
		{
			int num = (int)value;
			int num2 = (int)(value >> 32);
			this.SetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_0, num);
			this.SetRegistryInt(KartOptions.RegistryKey.GAMEQUEST_1, num2);
		}
	}

	public bool IsGameQuestFlagOn(KartOptions.GameQuestFlag f)
	{
		long num = 1L << (int)(f & KartOptions.GameQuestFlag.QUEST_31);
		return (this.GameQuest & num) != 0L;
	}

	public void UpdateGameGameQuestFlag()
	{
	}

	public void SetGameQuestFlag(KartOptions.GameQuestFlag f, bool on)
	{
		long num = this.GameQuest;
		long num2 = 1L << (int)(f & KartOptions.GameQuestFlag.QUEST_31);
		if (on)
		{
			num |= num2;
		}
		else
		{
			num &= ~num2;
		}
		this.GameQuest = num;
	}

	public string Race
	{
		get
		{
			return this.GetRegistryString(KartOptions.RegistryKey.RACE);
		}
		set
		{
			this.SetRegistryString(KartOptions.RegistryKey.RACE, value);
		}
	}

	public string ProgramVersion
	{
		get
		{
			return this.PROGRAM_VERSION;
		}
	}

	public int MultiplayerVersion
	{
		get
		{
			return this.MULTIPLAYER_VERSION;
		}
	}

	public int ApiVersion
	{
		get
		{
			return this.API_VERSION;
		}
	}

	public static KartOptions instance_;

	private string PROGRAM_VERSION = "2.0.5";

	private int VERSION_ = 1;

	private int MULTIPLAYER_VERSION = 1;

	private int API_VERSION = 2;

	private string fbUserName_ = string.Empty;

	private bool bgm_ = true;

	private bool fx_ = true;

	private RegistryValue[] registryOthers_ = new RegistryValue[]
	{
		new RegistryInt(1),
		new RegistryString(),
		new RegistryInt(0),
		new RegistryInt(0),
		new RegistryInt(0),
		new RegistryString(),
		new RegistryInt(2),
		new RegistryInt(0),
		new RegistryInt(0),
		new RegistryInt(TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[0]),
		new RegistryInt(TrackAssetDefinitionManager.RANDOM_IDX_ARRAY[0])
	};

	private RegistryTrack[] registryTrack_;

	private enum RegistryKey
	{
		VERSION,
		DEFAULT_USER_NAME,
		QUEST,
		GAMEQUEST_0,
		GAMEQUEST_1,
		RACE,
		CONTROLLER,
		CHARACTER,
		KART,
		LAST_SELECTED_ITEM_TRACK,
		LAST_SELECTED_SPEED_TRACK,
		TRACK_INFO,
		SIZE
	}

	public enum QuestFlag
	{
		FACEBOOK_LOGIN = 1,
		FACEBOOK_PUBLISHING,
		FACEBOOK_FRIEND = 4,
		CHANCHAN_BOOSTER = 8,
		TUTORIAL = 16,
		TUTORIAL2 = 32,
		TUTORIAL_MULTI = 64,
		CONTROLLER_0 = 65536,
		CONTROLLER_1 = 131072,
		CONTROLLER_2 = 262144
	}

	public enum GameQuestFlag
	{
		QUEST_00,
		QUEST_01,
		QUEST_02,
		QUEST_03,
		QUEST_04,
		QUEST_05,
		QUEST_06,
		QUEST_07,
		QUEST_08,
		QUEST_09,
		QUEST_10,
		QUEST_11,
		QUEST_12,
		QUEST_13,
		QUEST_14,
		QUEST_15,
		QUEST_16,
		QUEST_17,
		QUEST_18,
		QUEST_19,
		QUEST_20,
		QUEST_21,
		QUEST_22,
		QUEST_23,
		QUEST_24,
		QUEST_25,
		QUEST_26,
		QUEST_27,
		QUEST_28,
		QUEST_29,
		QUEST_30,
		QUEST_31,
		QUEST_32,
		QUEST_33,
		QUEST_34,
		QUEST_35,
		QUEST_36,
		QUEST_37,
		QUEST_38,
		QUEST_39,
		QUEST_40,
		QUEST_41,
		QUEST_42,
		QUEST_43,
		QUEST_44,
		QUEST_45,
		QUEST_46,
		QUEST_47,
		QUEST_48,
		QUEST_49,
		QUEST_50,
		QUEST_51,
		QUEST_52,
		QUEST_53,
		QUEST_54,
		QUEST_55,
		QUEST_56,
		QUEST_57,
		QUEST_58,
		QUEST_59,
		QUEST_60,
		QUEST_61,
		QUEST_62,
		QUEST_63
	}
}
