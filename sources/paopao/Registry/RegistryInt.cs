using System;
using UnityEngine;

public class RegistryInt : RegistryValue
{
	public RegistryInt(int defaultValue)
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = false;
		this.value_ = 0;
		this.default_ = defaultValue;
	}

	public override void ReadRegistry(string key)
	{
		this.isDirty_ = false;
		this.isReadRegistry_ = true;
		this.value_ = PlayerPrefs.GetInt(key, this.default_);
	}

	public override void SaveRegistry(string key)
	{
		if (this.isDirty_)
		{
			PlayerPrefs.SetInt(key, this.value_);
			this.isDirty_ = false;
		}
	}

	public override void Reset()
	{
		this.value_ = this.default_;
		this.isDirty_ = true;
		this.isReadRegistry_ = true;
	}

	public int Value
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
		return this.value_.ToString();
	}

	private int value_;

	private int default_;
}
