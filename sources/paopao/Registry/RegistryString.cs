using System;
using UnityEngine;

public class RegistryString : RegistryValue
{
	public RegistryString()
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = false;
		this.value_ = string.Empty;
	}

	public override void ReadRegistry(string key)
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = true;
		this.value_ = PlayerPrefs.GetString(key);
	}

	public override void SaveRegistry(string key)
	{
		if (this.isDirty_)
		{
			PlayerPrefs.SetString(key, this.value_);
			this.isDirty_ = false;
		}
	}

	public override void Reset()
	{
		this.value_ = string.Empty;
		this.isDirty_ = true;
		this.isReadRegistry_ = true;
	}

	public string Value
	{
		get
		{
			return this.value_;
		}
		set
		{
			this.value_ = value;
			this.isDirty_ = true;
		}
	}

	public override string ToString()
	{
		return this.value_;
	}

	private string value_;
}
