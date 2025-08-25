using System;

public class MonoBehaviourMessage2Param<T1, T2> : MonoBehaviourMessage
{
	public MonoBehaviourMessage2Param(MonoBehaviourMessageType type)
		: base(type)
	{
	}

	public MonoBehaviourMessage2Param<T1, T2> Initialize(T1 lparam, T2 rparam)
	{
		this.lparam_ = lparam;
		this.rparam_ = rparam;
		return this;
	}

	public T1 lparam_;

	public T2 rparam_;
}
