using System;

public class BlackBarMessage : MonoBehaviourMessage
{
	public BlackBarMessage()
		: base(MonoBehaviourMessageType.BLACK_BAR)
	{
	}

	public BlackBarMessage Initialize(bool isShow, bool isSmooth, bool isShowOnlyBlackBar)
	{
		this.isShow_ = isShow;
		this.isSmooth_ = isSmooth;
		this.isShowOnlyBlackBar_ = isShowOnlyBlackBar;
		return this;
	}

	public override string ToString()
	{
		return string.Format("[ {0}/{1}/{2} ]", this.isShow_, this.isSmooth_, this.isShowOnlyBlackBar_);
	}

	public bool isShow_;

	public bool isSmooth_;

	public bool isShowOnlyBlackBar_;
}
