using System;
using System.Collections;
using System.IO;

public class XMLToHashtable
{
	public Hashtable convertPropertyListXMLToHashtable(string xmlString)
	{
		XMLElement xmlelement = new XMLElement();
		StringReader stringReader = new StringReader(xmlString);
		xmlelement.parseFromReader(stringReader);
		Hashtable hashtable = new Hashtable();
		if (xmlelement.getName() == "plist")
		{
			ArrayList children = xmlelement.getChildren();
			if (children.Count == 1)
			{
				XMLElement xmlelement2 = (XMLElement)children[0];
				hashtable = this.processDict(xmlelement2.getChildren());
			}
		}
		return hashtable;
	}

	public Hashtable processDict(ArrayList children)
	{
		Hashtable hashtable = new Hashtable();
		string text = string.Empty;
		for (int i = 0; i < children.Count; i++)
		{
			XMLElement xmlelement = (XMLElement)children[i];
			if (xmlelement.getName() == "key")
			{
				text = xmlelement.getContents();
			}
			else if (xmlelement.getName() == "dict")
			{
				hashtable.Add(text, this.processDict(xmlelement.getChildren()));
			}
			else if (xmlelement.getName() == "string")
			{
				hashtable.Add(text, xmlelement.getContents());
			}
			else if (xmlelement.getName() == "integer")
			{
				hashtable.Add(text, xmlelement.getContents().ToString());
			}
		}
		return hashtable;
	}

	public void displayHashtable(Hashtable ht)
	{
		foreach (object obj in ht)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
			if (dictionaryEntry.Value.GetType() == typeof(Hashtable))
			{
				this.displayHashtable((Hashtable)dictionaryEntry.Value);
			}
		}
	}
}
