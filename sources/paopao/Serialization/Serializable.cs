using System;
using System.IO;

public interface Serializable
{
	void WriteTo(BinaryWriter writer);

	void ReadFrom(BinaryReader reader);
}
