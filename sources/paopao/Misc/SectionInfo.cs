using System;
using UnityEngine;

public class SectionInfo
{
	public SectionInfo(GoKart kart, int maxLap)
	{
		this.kart_ = kart;
		this.lapTime_ = new float[maxLap];
		for (int i = 0; i < this.lapTime_.Length; i++)
		{
			this.lapTime_[i] = 0f;
		}
	}

	public float GetBestLapTime()
	{
		float num = 0f;
		for (int i = 0; i < this.lap_ - 1; i++)
		{
			float num2 = this.lapTime_[i] - ((i != 0) ? this.lapTime_[i - 1] : 0f);
			num = ((i != 0) ? Mathf.Min(num2, num) : num2);
		}
		return num;
	}

	public override string ToString()
	{
		return string.Format(" rank[ {0} ] section : [ {1}/{2}/{3}/{4}/{5} ] ", new object[] { this.rank_, this.lap_, this.lastCorrectDirectionPassingPlane_, this.latestPassingPlane_, this.passCorrect_, this.distance_ });
	}

	public void ResetForRestarting()
	{
		this.lastKartPos_ = Vector3.zero;
		for (int i = 0; i < this.lapTime_.Length; i++)
		{
			this.lapTime_[i] = 0f;
		}
		this.passCorrect_ = true;
		this.distancePlane_ = -1;
		this.distance_ = 0f;
		this.latestPassingPlane_ = -1;
		this.lastCorrectDirectionPassingPlane_ = -1;
		this.rank_ = -1;
		this.rankValue = 0.0;
		this.needReset_ = false;
	}

	public const int NO_PLANE = -1;

	public GoKart kart_;

	public Vector3 lastKartPos_ = Vector3.zero;

	public int lap_;

	public float[] lapTime_;

	public bool passCorrect_ = true;

	public int distancePlane_ = -1;

	public float distance_;

	public int latestPassingPlane_ = -1;

	public int lastCorrectDirectionPassingPlane_ = -1;

	public int rank_ = -1;

	public double rankValue;

	public double networkRankValue;

	public bool needReset_;
}
