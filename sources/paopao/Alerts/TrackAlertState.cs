using System;
using System.Collections.Generic;

public class TrackAlertState : AlertState
{
	public TrackAlertState(Dictionary<string, AlertState.StateEnum> inits, Dictionary<string, AlertState.EventType> events, AlertStateCache cache)
		: base(inits, events, cache)
	{
	}

	public override void CheckUnlockedItems()
	{
		TrackAssetDefinitionManager.Instance.Refresh();
		List<AssetDefinition> assetDefinitionList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
		foreach (AssetDefinition assetDefinition in assetDefinitionList)
		{
			if (!assetDefinition.Lock)
			{
				base.Unlock(assetDefinition.Id);
			}
		}
	}

	public override void Click(string id, bool commit)
	{
		bool flag = false;
		AlertState.StateEnum state = base.GetState(id);
		if (state == AlertState.StateEnum.LOCKED_UPDATED)
		{
			base.SetState(id, AlertState.StateEnum.LOCKED_READ);
			flag = true;
		}
		if (flag && commit)
		{
			base.Commit();
		}
	}

	public override void Unlock(string id, bool commit)
	{
		bool flag = false;
		AlertState.StateEnum state = base.GetState(id);
		if (state == AlertState.StateEnum.LOCKED_UPDATED || state == AlertState.StateEnum.LOCKED_READ)
		{
			base.SetState(id, AlertState.StateEnum.UNLOCKED_UPDATED);
			flag = true;
		}
		if (flag && commit)
		{
			base.Commit();
		}
	}

	public override void Ride(string id, bool commit)
	{
		bool flag = false;
		AlertState.StateEnum state = base.GetState(id);
		if (state == AlertState.StateEnum.UNLOCKED_UPDATED)
		{
			base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
			flag = true;
		}
		if (flag && commit)
		{
			base.Commit();
		}
	}
}
