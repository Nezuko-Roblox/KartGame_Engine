using System;
using System.Collections.Generic;

public class KartAssetDefinition : AssetDefinition
{
	public KartAssetDefinition()
		: base(AssetType.KART)
	{
	}

	public override bool Initialize(XMLElement elem)
	{
		if (!base.Initialize(elem))
		{
			return false;
		}
		this.speed_ = (float)elem.getDoubleAttribute("speed", 0.0);
		this.acceleration_ = (float)elem.getDoubleAttribute("acceleration", 0.0);
		this.handling_ = (float)elem.getDoubleAttribute("handling", 0.0);
		this.assetName_ = elem.getStringAttribute("asset", string.Empty);
		this.level_ = (float)elem.getDoubleAttribute("level", 0.0);
		this.randomRange_ = (float)elem.getDoubleAttribute("randomrange", 0.20000000298023224);
		this.levelType_ = elem.getIntAttribute("leveltype", 0);
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
		return string.Format("{0} , {1} , {2} , {3} , {4}", new object[] { this.name_, this.assetName_, this.speed_, this.acceleration_, this.handling_ });
	}

	public float Speed
	{
		get
		{
			return this.speed_;
		}
	}

	public float Acceleration
	{
		get
		{
			return this.acceleration_;
		}
	}

	public float Handling
	{
		get
		{
			return this.handling_;
		}
	}

	public float Level
	{
		get
		{
			return this.level_;
		}
	}

	public float RandomRange
	{
		get
		{
			return this.randomRange_;
		}
	}

	public int LevelType
	{
		get
		{
			return this.levelType_;
		}
	}

	protected string assetName_;

	protected float speed_;

	protected float acceleration_;

	protected float handling_;

	protected float level_;

	protected float randomRange_;

	protected int levelType_;
}
