using System;
using System.Collections.Generic;

public class CharacterAlertState : BaseKartCharacterAlertState
{
	public CharacterAlertState(Dictionary<string, AlertState.StateEnum> inits, Dictionary<string, AlertState.EventType> events, AlertStateCache cache)
		: base(inits, events, cache)
	{
	}

	public override void CheckUnlockedItems()
	{
		CharacterAssetDefinitionManager.Instance.Refresh();
		List<AssetDefinition> assetDefinitionList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
		foreach (AssetDefinition assetDefinition in assetDefinitionList)
		{
			if (!assetDefinition.Lock)
			{
				base.Unlock(assetDefinition.Id);
			}
		}
	}
}
