using System;

public class MonoBehaviourMessage1Param<T> : MonoBehaviourMessage
{
	public MonoBehaviourMessage1Param(MonoBehaviourMessageType type)
		: base(type)
	{
	}

	public MonoBehaviourMessage1Param<T> Initialize(T param)
	{
		this.param_ = param;
		return this;
	}

	public T param_;
}
