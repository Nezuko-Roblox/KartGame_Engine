using System;
using System.Collections.Generic;
using UnityEngine;

public class AlertStateFactory
{
	private AlertStateFactory()
	{
	}

	public static AlertStateFactory Instance
	{
		get
		{
			if (AlertStateFactory.inst_ == null)
			{
				AlertStateFactory.inst_ = new AlertStateFactory();
			}
			return AlertStateFactory.inst_;
		}
	}

	public AlertState GetAlertState(AlertStateType type)
	{
		if (this.alertStates_[(int)type] == null)
		{
			Dictionary<string, AlertState.StateEnum> dictionary = null;
			Dictionary<string, AlertState.EventType> dictionary2 = null;
			AlertStateCache alertStateCache = new AlertStateCache(type);
			dictionary = alertStateCache.Load();
			if (!this.UpdatedToCurrentVersion(type))
			{
				XMLElement xmlelement = null;
				XMLElement xmlelement2 = null;
				foreach (object obj in this.DefaultXML(type).getChildren())
				{
					XMLElement xmlelement3 = (XMLElement)obj;
					if (xmlelement3.getName() == "inits")
					{
						xmlelement = xmlelement3;
					}
					if (xmlelement3.getName() == "events")
					{
						xmlelement2 = xmlelement3;
					}
				}
				if (dictionary == null)
				{
					dictionary = this.ParseInitXML(xmlelement);
				}
				else
				{
					foreach (KeyValuePair<string, AlertState.StateEnum> keyValuePair in this.ParseInitXML(xmlelement))
					{
						if (dictionary.ContainsKey(keyValuePair.Key))
						{
							dictionary[keyValuePair.Key] = keyValuePair.Value;
						}
						else
						{
							dictionary.Add(keyValuePair.Key, keyValuePair.Value);
						}
					}
				}
				dictionary2 = this.ParseEventXML(xmlelement2);
				this.UpdateVersion(type);
			}
			AlertState alertState = null;
			switch (type)
			{
			case AlertStateType.KART:
				alertState = new KartAlertState(dictionary, dictionary2, alertStateCache);
				break;
			case AlertStateType.CHARACTER:
				alertState = new CharacterAlertState(dictionary, dictionary2, alertStateCache);
				break;
			case AlertStateType.TRACK:
				alertState = new TrackAlertState(dictionary, dictionary2, alertStateCache);
				break;
			case AlertStateType.BUNDLE:
				alertState = new BundleAlertState(dictionary, dictionary2, alertStateCache);
				break;
			}
			this.alertStates_[(int)type] = alertState;
		}
		return this.alertStates_[(int)type];
	}

	private XMLElement DefaultXML(AlertStateType type)
	{
		XMLElement xmlelement = new XMLElement();
		xmlelement.parseString(((TextAsset)Resources.Load("states/default_" + type.ToString().ToLower())).text);
		return xmlelement;
	}

	public Dictionary<string, AlertState.StateEnum> ParseInitXML(XMLElement xml)
	{
		Dictionary<string, AlertState.StateEnum> dictionary = new Dictionary<string, AlertState.StateEnum>();
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			AlertState.StateEnum stateEnum = (AlertState.StateEnum)((int)Enum.Parse(typeof(AlertState.StateEnum), (string)xmlelement.getAttribute("state"), true));
			string text = (string)xmlelement.getAttribute("id");
			if (dictionary.ContainsKey(text))
			{
				dictionary[text] = stateEnum;
			}
			else
			{
				dictionary.Add(text, stateEnum);
			}
		}
		return dictionary;
	}

	public Dictionary<string, AlertState.EventType> ParseEventXML(XMLElement xml)
	{
		Dictionary<string, AlertState.EventType> dictionary = new Dictionary<string, AlertState.EventType>();
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			AlertState.EventType eventType = (AlertState.EventType)((int)Enum.Parse(typeof(AlertState.EventType), (string)xmlelement.getAttribute("type"), true));
			string text = (string)xmlelement.getAttribute("id");
			if (dictionary.ContainsKey(text))
			{
				dictionary[text] = eventType;
			}
			else
			{
				dictionary.Add(text, eventType);
			}
		}
		return dictionary;
	}

	private bool UpdatedToCurrentVersion(AlertStateType type)
	{
		string text = string.Format("{0}_LAST_UPDATE", type.ToString());
		return !(PlayerPrefs.GetString(text, null) != KartOptions.Instance.ProgramVersion);
	}

	private void UpdateVersion(AlertStateType type)
	{
		string text = string.Format("{0}_LAST_UPDATE", type.ToString());
		PlayerPrefs.SetString(text, KartOptions.Instance.ProgramVersion);
	}

	private static AlertStateFactory inst_;

	private AlertState[] alertStates_ = new AlertState[5];
}
