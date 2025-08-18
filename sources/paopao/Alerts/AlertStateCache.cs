using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AlertStateCache
{
	public AlertStateCache(AlertStateType type)
	{
		this.key_ = string.Format("{0}_STATE", type.ToString());
	}

	public Dictionary<string, AlertState.StateEnum> Load()
	{
		string @string = PlayerPrefs.GetString(this.key_, null);
		if (@string == null || @string == string.Empty)
		{
			return null;
		}
		Dictionary<string, AlertState.StateEnum> dictionary = new Dictionary<string, AlertState.StateEnum>();
		foreach (string text in @string.Split(new char[] { ',' }))
		{
			string[] array2 = text.Split(new char[] { ':' });
			string text2 = array2[0];
			AlertState.StateEnum stateEnum = (AlertState.StateEnum)int.Parse(array2[1]);
			dictionary.Add(text2, stateEnum);
		}
		return dictionary;
	}

	public void Save(Dictionary<string, AlertState.StateEnum> state)
	{
		StringBuilder stringBuilder = null;
		foreach (string text in state.Keys)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
			}
			else
			{
				stringBuilder.Append(",");
			}
			stringBuilder.Append(text);
			stringBuilder.Append(":");
			stringBuilder.Append((int)state[text]);
		}
		PlayerPrefs.SetString(this.key_, stringBuilder.ToString());
	}

	private const string KEY_TEMPLATE = "{0}_STATE";

	private string key_;
}
