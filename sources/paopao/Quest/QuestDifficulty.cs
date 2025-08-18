using System;

public class QuestDifficulty : QuestBase
{
	public QuestDifficulty(ulong trackFlag, byte raceFlag, QuestDifficulty.enOperator op, byte difficulty)
		: base(0)
	{
		this.trackFlag_ = trackFlag;
		this.raceFlag_ = raceFlag;
		this.operator_ = op;
		this.difficulty_ = difficulty;
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		this.max_ = 0;
		int[][] winCounter = Statistics.Instance.WinCounter;
		for (int i = 0; i < assetCount; i++)
		{
			if (((1L << (i & 31)) & (long)this.trackFlag_) != 0L)
			{
				if (this.operator_ == QuestDifficulty.enOperator.OR)
				{
					this.max_++;
					for (int j = 0; j < 4; j++)
					{
						if (((1 << j) & (int)this.raceFlag_) != 0 && winCounter[i][j] >= (int)this.difficulty_)
						{
							this.current_++;
							break;
						}
					}
				}
				else if (this.operator_ == QuestDifficulty.enOperator.AND)
				{
					for (int k = 0; k < 4; k++)
					{
						if (((1 << k) & (int)this.raceFlag_) != 0)
						{
							this.max_++;
							if (winCounter[i][k] >= (int)this.difficulty_)
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

	protected byte difficulty_;

	protected QuestDifficulty.enOperator operator_;

	public enum enOperator
	{
		AND,
		OR
	}
}
