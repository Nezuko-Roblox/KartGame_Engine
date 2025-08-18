using System;
using System.Collections.Generic;

public class MonoBehaviourExCenter
{
	public static MonoBehaviourExCenter Instance
	{
		get
		{
			if (MonoBehaviourExCenter.instance_ == null)
			{
				MonoBehaviourExCenter.instance_ = new MonoBehaviourExCenter();
			}
			return MonoBehaviourExCenter.instance_;
		}
	}

	public void RegistMonoBehaviour(int id, MonoBehaviourEx behaviour)
	{
		if (!this.listener_.ContainsKey(id))
		{
			this.listener_.Add(id, behaviour);
		}
	}

	public void UnregistMonoBehaviour(int id)
	{
		this.listener_.Remove(id);
	}

	public void SendMessage(int sender, int receiver, MonoBehaviourMessage msg)
	{
		if (this.listener_.ContainsKey(receiver))
		{
			this.listener_[receiver].ReceiveMessage(sender, msg);
		}
	}

	public void BroadcastMessage(int sender, MonoBehaviourMessage msg)
	{
		foreach (KeyValuePair<int, MonoBehaviourEx> keyValuePair in this.listener_)
		{
			if (keyValuePair.Key != sender)
			{
				keyValuePair.Value.ReceiveMessage(sender, msg);
			}
		}
	}

	public void LoadStage()
	{
		foreach (KeyValuePair<int, MonoBehaviourEx> keyValuePair in this.listener_)
		{
			keyValuePair.Value.OnLoadStage();
		}
	}

	public void UnloadStage()
	{
		foreach (KeyValuePair<int, MonoBehaviourEx> keyValuePair in this.listener_)
		{
			keyValuePair.Value.OnUnloadStage();
		}
		this.listener_.Clear();
	}

	public override string ToString()
	{
		string text = "total : " + this.listener_.Count.ToString() + "\n";
		foreach (KeyValuePair<int, MonoBehaviourEx> keyValuePair in this.listener_)
		{
			text = text + keyValuePair.Key.ToString() + "\n";
		}
		return text;
	}

	public static MonoBehaviourExCenter instance_;

	private Dictionary<int, MonoBehaviourEx> listener_ = new Dictionary<int, MonoBehaviourEx>();
}
