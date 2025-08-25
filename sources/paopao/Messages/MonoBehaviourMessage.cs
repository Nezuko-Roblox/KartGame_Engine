using System;

public class MonoBehaviourMessage
{
	public MonoBehaviourMessage()
	{
	}

	public MonoBehaviourMessage(MonoBehaviourMessageType type)
	{
		this.type_ = type;
	}

	public MonoBehaviourMessageType type_;
}
