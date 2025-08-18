using System;
using UnityEngine;

public class ApproachObject
{
	public ApproachObject(GameObject obj)
	{
		this.root_ = obj;
		this.ObjectSetting();
	}

	private void ObjectSetting()
	{
		if (this.root_ == null)
		{
			return;
		}
		foreach (object obj in this.root_.transform)
		{
			Transform transform = (Transform)obj;
			if (transform.name == "idle")
			{
				this.idle_ = transform.gameObject;
			}
			else if (transform.name == "touched")
			{
				this.touched_ = transform.gameObject;
			}
			else if (transform.name == "touched_bubble")
			{
				this.touchedBubble_ = transform.gameObject;
			}
		}
	}

	public ApproachObject.ApproachState GetApproachState()
	{
		if (this.root_ == null)
		{
			return ApproachObject.ApproachState.NONE;
		}
		if (!this.root_.active)
		{
			return ApproachObject.ApproachState.DESTROYED;
		}
		if (this.idle_.active)
		{
			return ApproachObject.ApproachState.ACTIVE;
		}
		if (this.touched_.active)
		{
			return (!this.touched_.animation.isPlaying) ? ApproachObject.ApproachState.EXPLORED : ApproachObject.ApproachState.EXPLORE;
		}
		return ApproachObject.ApproachState.DESTROYED;
	}

	public void SetApproachState(ApproachObject.ApproachState state)
	{
		if (state == ApproachObject.ApproachState.ACTIVE)
		{
			this.root_.active = true;
			this.idle_.SetActiveRecursively(true);
			this.touched_.SetActiveRecursively(false);
			this.touchedBubble_.SetActiveRecursively(false);
		}
		else if (state == ApproachObject.ApproachState.EXPLORE)
		{
			this.root_.active = true;
			this.idle_.SetActiveRecursively(false);
			this.touched_.SetActiveRecursively(true);
			this.touchedBubble_.SetActiveRecursively(true);
		}
		else if (state == ApproachObject.ApproachState.DESTROYED)
		{
			this.root_.SetActiveRecursively(false);
		}
	}

	private GameObject root_;

	private GameObject idle_;

	private GameObject touched_;

	private GameObject touchedBubble_;

	public enum ApproachState
	{
		NONE,
		ACTIVE,
		EXPLORE,
		EXPLORED,
		DESTROYED
	}

	public enum ApproachAnim
	{
		IDLE,
		EXPLORE
	}
}
