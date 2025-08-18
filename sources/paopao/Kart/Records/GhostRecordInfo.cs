using System;

public class GhostRecordInfo
{
	public GhostRecordInfo(GhostFilenameInfo info, GhostRecordType type)
	{
		this.filenameInfo_ = info;
		this.type_ = type;
		this.randomValue_ = 10;
	}

	public GhostRecordInfo(string filepath, GhostRecordType type)
	{
		this.filenameInfo_ = new GhostFilenameInfo(filepath);
		this.type_ = type;
		this.randomValue_ = 10;
	}

	public int GenerateRandomValue(float standardFinishTime)
	{
		return this.randomValue_;
	}

	public GhostRecordType type_;

	public int randomValue_;

	public GhostFilenameInfo filenameInfo_;
}
