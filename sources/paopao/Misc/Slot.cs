using System;

public class Slot
{
	public string id_;

	public int index_;

	public string name_;

	public Slot.State state_;

	public int rank_;

	public float raceEndTime_;

	public enum State
	{
		LOADING,
		LOADING_DONE,
		RACING,
		GOAL_IN,
		RACE_OVER
	}
}
