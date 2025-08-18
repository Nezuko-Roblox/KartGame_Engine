using System;

public class QuestRaceComplete : QuestBase
{
	public QuestRaceComplete(ulong trackFlag, byte raceFlag, QuestRaceComplete.enOperator op)
		: base(0)
	{
		this.trackFlag_ = trackFlag;
		this.raceFlag_ = raceFlag;
		this.operator_ = op;
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		this.max_ = 0;
		int[][] raceCompleteCounter = Statistics.Instance.RaceCompleteCounter;
		for (int i = 0; i < assetCount; i++)
		{
			if (((1L << (i & 31)) & (long)this.trackFlag_) != 0L)
			{
				if (this.operator_ == QuestRaceComplete.enOperator.OR)
				{
					this.max_++;
					for (int j = 0; j < 4; j++)
					{
						if (((1 << j) & (int)this.raceFlag_) != 0 && raceCompleteCounter[i][j] > 0)
						{
							this.current_++;
							break;
						}
					}
				}
				else if (this.operator_ == QuestRaceComplete.enOperator.AND)
				{
					for (int k = 0; k < 4; k++)
					{
						if (((1 << k) & (int)this.raceFlag_) != 0)
						{
							this.max_++;
							if (raceCompleteCounter[i][k] > 0)
							{
								this.current_++;
							}
						}
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
		text += this.operator_.ToString();
		return text + "\n";
	}

	protected byte raceFlag_;

	protected ulong trackFlag_;

	protected QuestRaceComplete.enOperator operator_;

	public enum enOperator
	{
		AND,
		OR
	}
}
