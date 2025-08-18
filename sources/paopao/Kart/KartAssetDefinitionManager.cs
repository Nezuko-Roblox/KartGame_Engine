using System;
using System.Collections.Generic;

public class KartAssetDefinitionManager : AssetDefinitionManager
{
	public static KartAssetDefinitionManager Instance
	{
		get
		{
			if (KartAssetDefinitionManager.instance_ == null)
			{
				KartAssetDefinitionManager.instance_ = new KartAssetDefinitionManager();
			}
			return KartAssetDefinitionManager.instance_;
		}
	}

	public override AssetDefinition CreateAssetDefinition()
	{
		return new KartAssetDefinition();
	}

	public override int GetMainIconIdx(int idx)
	{
		return idx + 24;
	}

	public void GetAssetsByLevelType(int leveltype, out List<int> l)
	{
		l = new List<int>();
		int num = 0;
		foreach (AssetDefinition assetDefinition in this.assets_)
		{
			KartAssetDefinition kartAssetDefinition = (KartAssetDefinition)assetDefinition;
			if (kartAssetDefinition.LevelType == leveltype)
			{
				l.Add(num);
			}
			num++;
		}
	}

	public void GetRandomAsset(int leveltype, int count, ref List<int> l)
	{
		if (count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		this.GetAssetsByLevelType(leveltype, out list);
		if (list.Count == 0)
		{
			return;
		}
		List<int> list2;
		FiaUtil.GetRandomList(0, list.Count - 1, out list2);
		for (int i = 0; i < count; i++)
		{
			l.Add(list[list2[i % list.Count]]);
		}
	}

	public static KartAssetDefinitionManager instance_;
}
