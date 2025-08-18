using System;
using System.Collections.Generic;

public class KartAlertState : BaseKartCharacterAlertState
{
	public KartAlertState(Dictionary<string, AlertState.StateEnum> inits, Dictionary<string, AlertState.EventType> events, AlertStateCache cache)
		: base(inits, events, cache)
	{
	}

	public override void CheckUnlockedItems()
	{
		KartAssetDefinitionManager.Instance.Refresh();
		List<AssetDefinition> assetDefinitionList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
		foreach (AssetDefinition assetDefinition in assetDefinitionList)
		{
			if (!assetDefinition.Lock)
			{
				base.Unlock(assetDefinition.Id);
			}
		}
	}
}
