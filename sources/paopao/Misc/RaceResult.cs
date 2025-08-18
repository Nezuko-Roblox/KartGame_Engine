using System;

public class RaceResult
{
	public RaceResult()
	{
		this.elems_ = new RaceResultElem[KartManager.Instance.GetGoKartCount()];
	}

	public RaceResult(int num)
	{
		this.elems_ = new RaceResultElem[num];
	}

	public bool IsBestRecord
	{
		get
		{
			return this.isBestRecord_;
		}
		set
		{
			this.isBestRecord_ = value;
		}
	}

	public bool IsMonthlyBestRecord
	{
		get
		{
			return this.isMonthlyBestRecords_;
		}
		set
		{
			this.isMonthlyBestRecords_ = value;
		}
	}

	public void SetResult(int kartIndex, int rank, float raceTime)
	{
		if (!MathHelper.IsBetweenIE(rank, 0, this.elems_.Length))
		{
			return;
		}
		this.elems_[rank] = new RaceResultElem(kartIndex, raceTime);
	}

	public void SetResult(int kartIndex, int rank, float raceTime, string name)
	{
		if (!MathHelper.IsBetweenIE(rank, 0, this.elems_.Length))
		{
			return;
		}
		this.elems_[rank] = new RaceResultElem(kartIndex, raceTime, name);
	}

	public bool IsSettingComplete()
	{
		for (int i = 0; i < this.elems_.Length; i++)
		{
			if (this.elems_[i] == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsUserWinner()
	{
		return this.elems_[0].kartIndex_ == KartManager.PLAYER_KART_IDX && !this.elems_[0].IsRetire();
	}

	public bool IsRetire(int kartIdx)
	{
		for (int i = 0; i < this.elems_.Length; i++)
		{
			if (this.elems_[i].kartIndex_ == kartIdx)
			{
				return this.elems_[i].IsRetire();
			}
		}
		return true;
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < this.elems_.Length; i++)
		{
			if (this.elems_[i] != null)
			{
				text += this.elems_[i].ToString();
				text += "\n";
			}
		}
		return text;
	}

	public RaceResultElem[] elems_;

	protected bool isBestRecord_;

	protected bool isMonthlyBestRecords_;
}
