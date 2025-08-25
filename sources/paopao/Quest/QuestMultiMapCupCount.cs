using System;
using UnityEngine;

public class QuestMultiMapCupCount : QuestBase
{
	public QuestMultiMapCupCount(int cupCount, int[] mapIds)
		: base(cupCount)
	{
		this.mapIds_ = mapIds;
		this.cupCount_ = cupCount;
	}

	public override void Refresh()
	{
		int[][] winCounter = Statistics.Instance.WinCounter;
		int num = 0;
		foreach (int num2 in this.mapIds_)
		{
			num += Mathf.Min(winCounter[num2][0], 3);
			num += Mathf.Min(winCounter[num2][1], 3);
		}
		this.current_ = num;
	}

	private int[] mapIds_;

	private int cupCount_;
}
