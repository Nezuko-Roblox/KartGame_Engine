using System;

public class TimeEvent
{
	public TimeEvent()
	{
		this.Reset(0f, 0);
	}

	public TimeEvent(float frequency, int totalNumber)
	{
		this.Reset(frequency, totalNumber);
	}

	public void Reset(float frequency, int totalNumber)
	{
		this.totalTime_ = 0f;
		this.frequency_ = frequency;
		this.number_ = totalNumber;
		this.isEventOccurred_ = false;
	}

	public void Update(float delta)
	{
		this.isEventOccurred_ = false;
		if (this.number_ != 0)
		{
			this.totalTime_ += delta;
			if (this.totalTime_ >= this.frequency_)
			{
				this.isEventOccurred_ = true;
				this.totalTime_ -= this.frequency_;
				if (this.number_ > 0)
				{
					this.number_--;
				}
			}
		}
	}

	public bool IsEventOccurred()
	{
		return this.isEventOccurred_;
	}

	public bool IsFinish()
	{
		return this.number_ == 0;
	}

	public int GetRemainCount()
	{
		return this.number_;
	}

	private float totalTime_;

	private float frequency_;

	private int number_;

	private bool isEventOccurred_;
}
