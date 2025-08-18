using System;

public abstract class RegistryValue
{
	public abstract void ReadRegistry(string key);

	public abstract void SaveRegistry(string key);

	public abstract void Reset();

	public bool isReadRegistry_;

	protected bool isDirty_;
}
