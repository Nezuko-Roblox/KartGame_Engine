using System;
using System.Collections.Generic;

public class KartJobQueue
{
	public static KartJobQueue Instance
	{
		get
		{
			if (KartJobQueue.instance_ == null)
			{
				KartJobQueue.instance_ = new KartJobQueue();
			}
			return KartJobQueue.instance_;
		}
	}

	public void Push(KartJob job)
	{
		this.jobQueue_.AddLast(job);
	}

	public KartJob Pop()
	{
		if (this.IsEmpty())
		{
			return null;
		}
		KartJob value = this.jobQueue_.First.Value;
		this.jobQueue_.RemoveFirst();
		return value;
	}

	public bool IsEmpty()
	{
		return this.jobQueue_.Count == 0;
	}

	public void Clear()
	{
		this.jobQueue_.Clear();
	}

	public static KartJobQueue instance_;

	private LinkedList<KartJob> jobQueue_ = new LinkedList<KartJob>();
}
