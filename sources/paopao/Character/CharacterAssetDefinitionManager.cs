using System;

public class CharacterAssetDefinitionManager : AssetDefinitionManager
{
	public static CharacterAssetDefinitionManager Instance
	{
		get
		{
			if (CharacterAssetDefinitionManager.instance_ == null)
			{
				CharacterAssetDefinitionManager.instance_ = new CharacterAssetDefinitionManager();
			}
			return CharacterAssetDefinitionManager.instance_;
		}
	}

	public override AssetDefinition CreateAssetDefinition()
	{
		return new CharacterAssetDefinition();
	}

	public static CharacterAssetDefinitionManager instance_;
}
