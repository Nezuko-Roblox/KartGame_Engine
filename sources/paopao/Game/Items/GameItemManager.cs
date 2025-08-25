using System;
using UnityEngine;

public class GameItemManager
{
	public static GameItemManager Instance
	{
		get
		{
			if (GameItemManager.instance_ == null)
			{
				GameItemManager.instance_ = new GameItemManager();
			}
			return GameItemManager.instance_;
		}
	}

	public GameItem GenerateItem(int kartIndex)
	{
		if (KartManager.Instance.goCourse_ == null)
		{
			return GameItem.NONE;
		}
		if (KartManager.Instance.goCourse_.IsKartGoalIn(kartIndex))
		{
			return GameItem.NONE;
		}
		int kartNoInRacing = KartManager.Instance.goCourse_.GetKartNoInRacing();
		int rank = KartManager.Instance.goCourse_.GetRank(kartIndex);
		int goKartCount = KartManager.Instance.GetGoKartCount();
		if (this.ITEM_PROBABILITY[goKartCount - 1] == null || !MathHelper.IsBetweenIE(rank, 0, goKartCount))
		{
			return (GameItem)global::UnityEngine.Random.Range(0, 9);
		}
		if (kartNoInRacing == 1)
		{
			return GameItem.BOOSTER;
		}
		int num = global::UnityEngine.Random.Range(0, 100);
		for (int i = 0; i < 10; i++)
		{
			if (MathHelper.IsBetweenIE(num, 0, this.ITEM_PROBABILITY[goKartCount - 1][i, rank]))
			{
				return (GameItem)i;
			}
			num -= this.ITEM_PROBABILITY[goKartCount - 1][i, rank];
		}
		return GameItem.NONE;
	}

	public static GameItemManager instance_;

	private int[][,] ITEM_PROBABILITY = new int[][,]
	{
		default(int[,]),
		new int[,]
		{
			{ 20, 40 },
			{ 40, 0 },
			{ 0, 10 },
			{ 10, 15 },
			{ 10, 15 },
			{ 0, 0 },
			{ 0, 10 },
			{ 10, 10 },
			{ 10, 0 },
			{ 0, 0 }
		},
		new int[,]
		{
			{ 10, 25, 40 },
			{ 70, 0, 0 },
			{ 0, 10, 10 },
			{ 0, 25, 15 },
			{ 0, 20, 15 },
			{ 0, 0, 0 },
			{ 0, 0, 10 },
			{ 0, 20, 10 },
			{ 20, 0, 0 },
			{ 0, 0, 0 }
		},
		new int[,]
		{
			{ 10, 20, 30, 40 },
			{ 70, 0, 0, 0 },
			{ 0, 10, 10, 0 },
			{ 0, 25, 20, 17 },
			{ 0, 25, 20, 17 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 10 },
			{ 0, 20, 20, 16 },
			{ 20, 0, 0, 0 },
			{ 0, 0, 0, 0 }
		},
		default(int[,]),
		new int[,]
		{
			{ 10, 20, 20, 30, 40, 50 },
			{ 70, 0, 0, 0, 0, 0 },
			{ 0, 10, 10, 0, 0, 0 },
			{ 0, 25, 25, 25, 20, 15 },
			{ 0, 25, 25, 25, 20, 15 },
			{ 0, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 10 },
			{ 0, 20, 20, 20, 20, 10 },
			{ 20, 0, 0, 0, 0, 0 },
			{ 0, 0, 0, 0, 0, 0 }
		}
	};
}
