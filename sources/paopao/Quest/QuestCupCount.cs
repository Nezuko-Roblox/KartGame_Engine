using System;
using UnityEngine;

public class QuestCupCount : QuestBase
{
	public QuestCupCount(int _max)
		: base(_max)
	{
	}

	public override void Refresh()
	{
		int assetCount = TrackAssetDefinitionManager.Instance.GetAssetCount();
		this.current_ = 0;
		int[][] winCounter = Statistics.Instance.WinCounter;
		for (int i = 0; i < assetCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				this.current_ += Mathf.Min(winCounter[i][j], 3);
			}
		}
	}
}
