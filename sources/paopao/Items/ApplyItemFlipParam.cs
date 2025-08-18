using System;

public class ApplyItemFlipParam : ApplyItemParam
{
	public ApplyItemFlipParam(int senderId, bool flipStart)
		: base(senderId)
	{
		this.isFlipStart_ = flipStart;
	}

	public bool isFlipStart_;
}
