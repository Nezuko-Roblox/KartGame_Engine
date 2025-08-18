using System;

public class Statistics
{
	public static Statistics Instance
	{
		get
		{
			if (Statistics.instance_ == null)
			{
				Statistics.instance_ = new Statistics();
			}
			return Statistics.instance_;
		}
	}

	public void Initialize()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.raceCompleteCounter_ = new int[assetCount][];
		this.winCounter_ = new int[assetCount][];
		for (int i = 0; i < assetCount; i++)
		{
			this.raceCompleteCounter_[i] = new int[4];
			Array.Copy(KartOptions.Instance.GetRaceCounter((byte)i), this.raceCompleteCounter_[i], 4);
			this.winCounter_[i] = new int[4];
			Array.Copy(KartOptions.Instance.GetWinCounter((byte)i), this.winCounter_[i], 4);
		}
	}

	public void RaceComplete(bool winner)
	{
		if (this.raceCompleteCounter_ == null)
		{
			return;
		}
		StageType stage = KartManager.Instance.parameter_.Stage;
		if (stage != StageType.GAME && stage != StageType.GAME_WIFI)
		{
			return;
		}
		int num = ((stage != StageType.GAME) ? 1 : 0);
		int track_ = (int)KartManager.Instance.parameter_.track_;
		int num2 = ((KartManager.Instance.parameter_.gameMode_ != GameMode.SINGLE_ITEM) ? 1 : 0);
		this.raceCompleteCounter_[track_][num * 2 + num2]++;
		KartOptions.Instance.SetRaceCounter((byte)track_, this.raceCompleteCounter_[track_]);
		KartOptions.QuestFlag questFlag = (KartOptions.QuestFlag)(65536 << (int)iOSController.Instance.Type);
		KartOptions.Instance.SetQuestFlag(questFlag, true);
		if (track_ == 11 && KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED && winner && this.WinCounter[11][1] >= 2)
		{
			KartOptions.QuestFlag questFlag2 = KartOptions.QuestFlag.CHANCHAN_BOOSTER;
			if (!((RigidbodyFPSWalker)KartManager.Instance.goPlayKart_.controller_).UsedNormalBooster)
			{
				KartOptions.Instance.SetQuestFlag(questFlag2, true);
			}
		}
	}

	public void IncreaseWinCount()
	{
		if (this.winCounter_ == null)
		{
			return;
		}
		StageType stage = KartManager.Instance.parameter_.Stage;
		if (stage != StageType.GAME && stage != StageType.GAME_WIFI)
		{
			return;
		}
		int num = ((stage != StageType.GAME) ? 1 : 0);
		int track_ = (int)KartManager.Instance.parameter_.track_;
		int num2 = ((KartManager.Instance.parameter_.gameMode_ != GameMode.SINGLE_ITEM) ? 1 : 0);
		this.winCounter_[track_][num * 2 + num2]++;
		KartOptions.Instance.SetWinCounter((byte)track_, this.winCounter_[track_]);
	}

	public int[][] RaceCompleteCounter
	{
		get
		{
			return this.raceCompleteCounter_;
		}
	}

	public int[][] WinCounter
	{
		get
		{
			return this.winCounter_;
		}
	}

	public static Statistics instance_;

	private int[][] raceCompleteCounter_;

	private int[][] winCounter_;
}
