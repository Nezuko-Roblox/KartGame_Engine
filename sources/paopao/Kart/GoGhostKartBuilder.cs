using System;

public class GoGhostKartBuilder : GoKartBuilder
{
	public GoGhostKartBuilder(string recordname, bool isResourceDirectory)
	{
		this.recordName_ = recordname;
		this.isResourceDirectory_ = isResourceDirectory;
	}

	public override GoKart Build()
	{
		return new GoGhostKart(this.recordName_, this.isResourceDirectory_);
	}

	private string recordName_;

	private bool isResourceDirectory_;
}
