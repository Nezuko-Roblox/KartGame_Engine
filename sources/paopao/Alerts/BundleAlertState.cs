using System;
using System.Collections.Generic;
using UnityEngine;

public class BundleAlertState : AlertState
{
	public BundleAlertState(Dictionary<string, AlertState.StateEnum> inits, Dictionary<string, AlertState.EventType> events, AlertStateCache cache)
		: base(inits, events, cache)
	{
	}

	public override void CheckUnlockedItems()
	{
	}

	public override void Click(string id, bool commit)
	{
		bool flag = false;
		switch (base.GetState(id))
		{
		case AlertState.StateEnum.LOCKED_UPDATED:
			base.SetState(id, AlertState.StateEnum.LOCKED_READ);
			flag = true;
			break;
		case AlertState.StateEnum.UNLOCKED_UPDATED:
			base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
			flag = true;
			break;
		}
		if (flag && commit)
		{
			base.Commit();
		}
	}

	public override void Unlock(string id, bool commit)
	{
		bool flag = false;
		Debug.Log("__________________________________Unlock:" + id);
		AlertState.StateEnum state = base.GetState(id);
		if (state == AlertState.StateEnum.LOCKED_UPDATED || state == AlertState.StateEnum.LOCKED_READ)
		{
			base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
			flag = true;
		}
		if (flag && commit)
		{
			base.Commit();
		}
	}

	public override void Ride(string id, bool commit)
	{
	}
}
