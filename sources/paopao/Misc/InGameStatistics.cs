using System;

public class InGameStatistics
{
	public InGameStatistics()
	{
		this.Initialize();
	}

	public static InGameStatistics Instance
	{
		get
		{
			if (InGameStatistics.instance_ == null)
			{
				InGameStatistics.instance_ = new InGameStatistics();
			}
			return InGameStatistics.instance_;
		}
	}

	public void Initialize()
	{
		if (this._total_item_usage == null)
		{
			this._total_item_usage = new InGameStatistics.ItemUsage();
		}
		if (this._effective_item_usage == null)
		{
			this._effective_item_usage = new InGameStatistics.ItemUsage();
		}
		if (this._others == null)
		{
			this._others = new InGameStatistics.OthersStatistics();
		}
		this._total_item_usage.Initialize();
		this._effective_item_usage.Initialize();
		this._others.Initialize();
	}

	public InGameStatistics.ItemUsage TotalItemUsage
	{
		get
		{
			return this._total_item_usage;
		}
	}

	public InGameStatistics.ItemUsage EffectiveItemUsage
	{
		get
		{
			return this._effective_item_usage;
		}
	}

	public InGameStatistics.OthersStatistics Others
	{
		get
		{
			return this._others;
		}
	}

	private static InGameStatistics instance_;

	private InGameStatistics.ItemUsage _total_item_usage;

	private InGameStatistics.ItemUsage _effective_item_usage;

	private InGameStatistics.OthersStatistics _others;

	public class ItemUsage
	{
		public ItemUsage()
		{
			this.Initialize();
		}

		public void Initialize()
		{
			for (int i = 0; i < this._stats.Length; i++)
			{
				this._stats[i] = 0;
			}
		}

		public int Stat(GameItem item)
		{
			return this._stats[(int)item];
		}

		public void SetStat(GameItem item, int value)
		{
			this._stats[(int)item] = value;
		}

		public void IncreaseStat(GameItem item)
		{
			this._stats[(int)item]++;
		}

		private int[] _stats = new int[10];
	}

	public class OthersStatistics
	{
		public OthersStatistics()
		{
			this.Initialize();
		}

		public void Initialize()
		{
			this._shake_booster = 0;
			this._drift_booster = 0;
			this._collision = 0;
		}

		public int ShakeBooster
		{
			get
			{
				return this._shake_booster;
			}
			set
			{
				this._shake_booster = value;
			}
		}

		public int DriftBooster
		{
			get
			{
				return this._drift_booster;
			}
			set
			{
				this._drift_booster = value;
			}
		}

		public int Collision
		{
			get
			{
				return this._collision;
			}
			set
			{
				this._collision = value;
			}
		}

		private int _shake_booster;

		private int _drift_booster;

		private int _collision;
	}
}
