using System;
using System.IO;

public abstract class ItemParam : Serializable
{
	public abstract void WriteTo(BinaryWriter writer);

	public abstract void ReadFrom(BinaryReader reader);
}
