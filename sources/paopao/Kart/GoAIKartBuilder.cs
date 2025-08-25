using System;

public class GoAIKartBuilder : GoKartBuilder
{
	public GoAIKartBuilder(int kartIndex, string[] recordname, bool isRecordInAssetBundle)
	{
		this.recordName_ = new string[recordname.Length];
		for (int i = 0; i < recordname.Length; i++)
		{
			this.recordName_[i] = recordname[i];
		}
		this.recordName_ = recordname;
		this.kartIndex_ = kartIndex;
		this.isRecordInAssetBundle_ = isRecordInAssetBundle;
	}

	public override GoKart Build()
	{
		return new GoAIKart(this.kartIndex_, this.recordName_, this.isRecordInAssetBundle_);
	}

	private string[] recordName_;

	private int kartIndex_;

	private bool isRecordInAssetBundle_;
}
