using System;
using UnityEngine;

public class QuestMapCupCount : QuestBase
{
	public QuestMapCupCount(int _max, int mapId, int itemCupCount, int speedCupCount)
		: base(_max)
	{
		this.mapId_ = mapId;
		this.itemCupCount_ = itemCupCount;
		this.speedCupCount_ = speedCupCount;
	}

	public override void Refresh()
	{
		int[][] winCounter = Statistics.Instance.WinCounter;
		int num = Mathf.Min(winCounter[this.mapId_][0], 2);
		int num2 = Mathf.Min(winCounter[this.mapId_][1], 2);
		this.current_ = num + num2;
	}

	private int mapId_;

	private int itemCupCount_;

	private int speedCupCount_;
}
