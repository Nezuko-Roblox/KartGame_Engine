using System;
using System.Collections.Generic;

public class TrackAssetDefinition : AssetDefinition
{
	public TrackAssetDefinition()
		: base(AssetType.TRACK)
	{
		this.medalTime_ = new float[6];
	}

	public override bool Initialize(XMLElement elem)
	{
		if (!base.Initialize(elem))
		{
			return false;
		}
		this.assetName_ = elem.getStringAttribute("asset", string.Empty);
		this.maxLap_ = elem.getIntAttribute("maxlap", 1);
		string stringAttribute = elem.getStringAttribute("medal_time", string.Empty);
		if (stringAttribute != string.Empty)
		{
			char[] array = new char[] { ' ', '\t' };
			string[] array2 = stringAttribute.Split(array, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == this.medalTime_.Length)
			{
				for (int i = 0; i < this.medalTime_.Length; i++)
				{
					this.medalTime_[i] = float.Parse(array2[i]);
				}
			}
		}
		return true;
	}

	public override string GetMainAsset()
	{
		return this.assetName_;
	}

	public override void GetAssets(ref List<string> assetbundleList)
	{
		assetbundleList.Add(this.assetName_);
	}

	public override string ToString()
	{
		return string.Format("{0} , {1} , {2} ", this.name_, this.assetName_, this.maxLap_);
	}

	public int MaxLap
	{
		get
		{
			return this.maxLap_;
		}
	}

	public float[] MedalTime
	{
		get
		{
			return this.medalTime_;
		}
	}

	public float GetMedalTime(GameMode gameMode, MedalType medalType)
	{
		if (medalType == MedalType.COMPLETE)
		{
			return 0f;
		}
		return this.medalTime_[(int)(gameMode * (GameMode)3 + (int)medalType)];
	}

	public float[] GetMedalTime(GameMode gameMode)
	{
		float[] array = new float[3];
		Array.Copy(this.medalTime_, (int)(gameMode * (GameMode)3), array, 0, 3);
		return array;
	}

	protected string assetName_;

	protected int maxLap_;

	protected float[] medalTime_;
}
