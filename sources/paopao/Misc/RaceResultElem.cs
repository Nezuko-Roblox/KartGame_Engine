using System;

public class RaceResultElem
{
	public RaceResultElem(int kartIdx, float raceTime)
	{
		this.kartIndex_ = kartIdx;
		this.raceTime_ = raceTime;
		this.name_ = KartManager.Instance.parameter_.kart_[kartIdx].name_;
	}

	public RaceResultElem(int kartIdx, float raceTime, string name)
	{
		this.kartIndex_ = kartIdx;
		this.raceTime_ = raceTime;
		this.name_ = name;
	}

	public bool IsRetire()
	{
		return this.raceTime_ <= 0f;
	}

	public override string ToString()
	{
		return string.Format("{0} {1} {2}", this.kartIndex_, this.raceTime_, this.name_);
	}

	public int kartIndex_;

	public string name_;

	public float raceTime_;
}
