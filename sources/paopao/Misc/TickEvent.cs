using System;

public class TickEvent
{
	public TickEvent(int frequency)
	{
		this.tick_ = 0;
		this.frequency_ = frequency;
	}

	public void Update()
	{
		this.tick_++;
	}

	public bool IsEventOccurred()
	{
		return this.tick_ % this.frequency_ == 0;
	}

	private int tick_;

	private int frequency_;
}
