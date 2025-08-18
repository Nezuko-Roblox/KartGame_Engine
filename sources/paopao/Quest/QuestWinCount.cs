using System;

public class QuestWinCount : QuestBase
{
	public QuestWinCount(int _max, ulong trackFlag, byte raceFlag)
		: base(_max)
	{
		this.trackFlag_ = trackFlag;
		this.raceFlag_ = raceFlag;
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		int[][] winCounter = Statistics.Instance.WinCounter;
		for (int i = 0; i < assetCount; i++)
		{
			if (((1L << (i & 31)) & (long)this.trackFlag_) != 0L)
			{
				for (int j = 0; j < 4; j++)
				{
					if (((1 << j) & (int)this.raceFlag_) != 0)
					{
						this.current_ += winCounter[i][j];
					}
				}
			}
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		for (int i = 0; i < assetCount; i++)
		{
			if (((1L << (i & 31)) & (long)this.trackFlag_) != 0L)
			{
				text += TrackAssetDefinitionManager.Instance.GetAssetDefinition(i).Name;
				text += "\n";
			}
		}
		text += "\n";
		string[] array = new string[] { "single item", "single speed", "wifi item", "wifi speed" };
		for (int j = 0; j < 4; j++)
		{
			if (((1 << j) & (int)this.raceFlag_) != 0)
			{
				text += array[j];
				text += "\n";
			}
		}
		return text;
	}

	protected byte raceFlag_;

	protected ulong trackFlag_;
}
