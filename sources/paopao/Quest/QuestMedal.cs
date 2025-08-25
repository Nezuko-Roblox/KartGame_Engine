using System;

public class QuestMedal : QuestBase
{
	public QuestMedal(ulong trackFlag, byte raceFlag, MedalType medalType)
		: base(0)
	{
		this.trackFlag_ = trackFlag;
		this.raceFlag_ = raceFlag;
		this.medalType_ = medalType;
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		this.max_ = 0;
		for (int i = 0; i < assetCount; i++)
		{
			if (((1L << (i & 31)) & (long)this.trackFlag_) != 0L)
			{
				TrackAssetDefinition trackAssetDefinition = (TrackAssetDefinition)TrackAssetDefinitionManager.Instance.GetAssetDefinition(i);
				for (int j = 0; j < 2; j++)
				{
					if (((1 << j) & (int)this.raceFlag_) != 0)
					{
						this.max_++;
						GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo((byte)i, j);
						if (bestInfo != null && (this.medalType_ == MedalType.COMPLETE || bestInfo.finishTime_ <= trackAssetDefinition.MedalTime[(int)(j * 3 + this.medalType_)]))
						{
							this.current_++;
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
		return text + this.medalType_.ToString();
	}

	protected byte raceFlag_;

	protected ulong trackFlag_;

	private MedalType medalType_;
}
