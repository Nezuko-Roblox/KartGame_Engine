using System;

public class QuestTrackCount : QuestBase
{
	public QuestTrackCount(int _max)
		: base(_max)
	{
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		int[][] raceCompleteCounter = Statistics.Instance.RaceCompleteCounter;
		for (int i = 0; i < assetCount; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				if (raceCompleteCounter[i][j] > 0)
				{
					this.current_++;
					break;
				}
			}
		}
	}
}
