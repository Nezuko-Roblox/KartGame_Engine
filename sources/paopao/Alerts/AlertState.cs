using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AlertState : FiaEntityState
{
	public AlertState(Dictionary<string, AlertState.StateEnum> inits, Dictionary<string, AlertState.EventType> events, AlertStateCache cache)
	{
		this.states_ = inits;
		this.cache_ = cache;
		this.CheckUnlockedItems();
		this.ProcessEvents(events);
		this.Commit();
	}

	public bool DisplayAlert(string id)
	{
		bool flag = false;
		AlertState.StateEnum state = this.GetState(id);
		foreach (AlertState.StateEnum stateEnum in this.ALERT_STATES)
		{
			if (stateEnum == state)
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	public void Refresh()
	{
		this.CheckUnlockedItems();
		this.Commit();
	}

	public bool DisplayAlert(int id)
	{
		Debug.Log("______________DisplayAlert:" + id);
		return this.DisplayAlert(id.ToString());
	}

	public bool ExistsAlert()
	{
		bool flag = false;
		foreach (AlertState.StateEnum stateEnum in this.ALERT_STATES)
		{
			if (this.states_.ContainsValue(stateEnum))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	public int AlertCount()
	{
		int num = 0;
		foreach (string text in this.states_.Keys)
		{
			AlertState.StateEnum stateEnum = this.states_[text];
			foreach (AlertState.StateEnum stateEnum2 in this.ALERT_STATES)
			{
				if (stateEnum2 == stateEnum)
				{
					num++;
				}
			}
		}
		return num;
	}

	public abstract void CheckUnlockedItems();

	private void ProcessEvents(Dictionary<string, AlertState.EventType> events)
	{
		if (events == null)
		{
			return;
		}
		foreach (KeyValuePair<string, AlertState.EventType> keyValuePair in events)
		{
			string key = keyValuePair.Key;
			switch (keyValuePair.Value)
			{
			case AlertState.EventType.CLICK:
				this.Click(key, false);
				break;
			case AlertState.EventType.RIDE:
				this.Ride(key, false);
				break;
			case AlertState.EventType.UNLOCK:
				this.Unlock(key, false);
				break;
			}
		}
	}

	public void Click(string id)
	{
		this.Click(id, true);
	}

	public void Ride(string id)
	{
		this.Ride(id, true);
	}

	public void Unlock(string id)
	{
		this.Unlock(id, true);
	}

	public void Click(int id)
	{
		this.Click(id.ToString(), true);
	}

	public void Ride(int id)
	{
		this.Ride(id.ToString(), true);
	}

	public void Unlock(int id)
	{
		this.Unlock(id.ToString(), true);
	}

	public abstract void Click(string id, bool commit);

	public abstract void Ride(string id, bool commit);

	public abstract void Unlock(string id, bool commit);

	public AlertState.StateEnum GetState(string id)
	{
		if (!this.states_.ContainsKey(id))
		{
			return AlertState.StateEnum.UNLOCKED_READ;
		}
		return this.states_[id];
	}

	public AlertState.StateEnum GetState(int id)
	{
		return this.GetState(id.ToString());
	}

	protected void SetState(string id, AlertState.StateEnum state)
	{
		if (!this.states_.ContainsKey(id))
		{
			this.states_.Add(id, state);
		}
		else
		{
			this.states_[id] = state;
		}
	}

	protected void Commit()
	{
		this.cache_.Save(this.states_);
	}

	private const AlertState.StateEnum DEFAULT_STATE = AlertState.StateEnum.UNLOCKED_READ;

	private AlertState.StateEnum[] ALERT_STATES = new AlertState.StateEnum[]
	{
		AlertState.StateEnum.LOCKED_UPDATED,
		AlertState.StateEnum.UNLOCKED_UPDATED
	};

	private AlertStateCache cache_;

	private Dictionary<string, AlertState.StateEnum> states_ = new Dictionary<string, AlertState.StateEnum>();

	public enum EventType
	{
		CLICK,
		RIDE,
		UNLOCK
	}

	public enum StateEnum
	{
		LOCKED_UPDATED,
		LOCKED_READ,
		UNLOCKED_UPDATED,
		UNLOCKED_READ
	}
}
