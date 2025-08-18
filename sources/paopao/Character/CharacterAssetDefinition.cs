using System;
using System.Collections.Generic;

public class CharacterAssetDefinition : AssetDefinition
{
	public CharacterAssetDefinition()
		: base(AssetType.CHARACTER)
	{
	}

	public override bool Initialize(XMLElement elem)
	{
		if (!base.Initialize(elem))
		{
			return false;
		}
		this.boneName_ = elem.getStringAttribute("bone", string.Empty);
		this.modelingName_ = elem.getStringAttribute("modeling", string.Empty);
		this.noAniName_ = elem.getStringAttribute("noani", string.Empty);
		return true;
	}

	public override string GetMainAsset()
	{
		return this.boneName_;
	}

	public override void GetAssets(ref List<string> assetbundleList)
	{
		assetbundleList.Add(this.boneName_);
		assetbundleList.Add(this.modelingName_);
		if (this.noAniName_ != string.Empty)
		{
			assetbundleList.Add(this.noAniName_);
		}
	}

	public override string ToString()
	{
		return string.Format("{0} , {1} , {2} , {3} ", new object[] { this.name_, this.boneName_, this.modelingName_, this.noAniName_ });
	}

	protected string boneName_;

	protected string modelingName_;

	protected string noAniName_;
}
