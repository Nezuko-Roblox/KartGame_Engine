using System;
using UnityEngine;

public class MonoBehaviourEx : MonoBehaviour
{
	public virtual void RegistMonoBehaviour(int id)
	{
		this.id_ = id;
		MonoBehaviourExCenter.Instance.RegistMonoBehaviour(this.id_, this);
	}

	public void SendMessage(int targetId, MonoBehaviourMessage msg)
	{
		if (this.id_ == -1)
		{
			return;
		}
		MonoBehaviourExCenter.Instance.SendMessage(this.id_, targetId, msg);
	}

	public void BroadcastMessage(MonoBehaviourMessage msg)
	{
		if (this.id_ == -1)
		{
			return;
		}
		MonoBehaviourExCenter.Instance.BroadcastMessage(this.id_, msg);
	}

	public virtual void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
	}

	public virtual void OnLoadStage()
	{
	}

	public virtual void OnUnloadStage()
	{
	}

	public int id_ = -1;
}
