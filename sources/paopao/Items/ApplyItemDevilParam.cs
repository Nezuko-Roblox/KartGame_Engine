using System;

public class ApplyItemDevilParam : ApplyItemParam
{
	public ApplyItemDevilParam(int senderId, bool devilStart)
		: base(senderId)
	{
		this.isDevilStart_ = devilStart;
	}

	public bool isDevilStart_;
}
