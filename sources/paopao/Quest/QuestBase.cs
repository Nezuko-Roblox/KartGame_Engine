using System;

public class QuestBase
{
	public QuestBase(int _max)
	{
		this.max_ = _max;
	}

	public virtual void Refresh()
	{
	}

	public virtual bool IsComplete()
	{
		return this.max_ <= this.current_;
	}

	public int GetGoal()
	{
		return this.max_;
	}

	public int GetCurrent()
	{
		return this.current_;
	}

	public override string ToString()
	{
		return string.Format("[{0}/{1}]\n", this.current_, this.max_);
	}

	protected int max_;

	protected int current_;
}
