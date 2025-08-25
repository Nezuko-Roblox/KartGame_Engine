using System;
using UnityEngine;

public class StringHolder : ScriptableObject
{
	public StringHolder(string[] content)
	{
		this.content = content;
	}

	public string[] content;
}
