using System;

public class TrackAssetDefinitionManager : AssetDefinitionManager
{
	public static TrackAssetDefinitionManager Instance
	{
		get
		{
			if (TrackAssetDefinitionManager.instance_ == null)
			{
				TrackAssetDefinitionManager.instance_ = new TrackAssetDefinitionManager();
			}
			return TrackAssetDefinitionManager.instance_;
		}
	}

	public override AssetDefinition CreateAssetDefinition()
	{
		return new TrackAssetDefinition();
	}

	public bool IsRandomTrackIndex(int idx)
	{
		return !MathHelper.IsBetweenIE(idx, 0, base.GetAssetCount());
	}

	public static TrackAssetDefinitionManager instance_ = null;

	public static int[] RANDOM_IDX_ARRAY = new int[] { -6 };
}
