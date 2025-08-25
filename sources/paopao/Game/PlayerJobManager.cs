using System;
using System.Collections.Generic;

public class PlayerJobManager
{
	public void Update()
	{
		if (this.jobs_.Count == 0)
		{
			return;
		}
		foreach (PlayerJob playerJob in this.jobs_)
		{
			playerJob.Update();
		}
		this.jobs_.RemoveAll(new Predicate<PlayerJob>(PlayerJobManager.IsJobFinish));
	}

	public void AddJob(PlayerJob job)
	{
		job.Initialize();
		this.jobs_.Add(job);
	}

	public void Clear()
	{
		this.jobs_.Clear();
	}

	public static bool IsJobFinish(PlayerJob job)
	{
		return job.IsFinish();
	}

	private List<PlayerJob> jobs_ = new List<PlayerJob>();
}
