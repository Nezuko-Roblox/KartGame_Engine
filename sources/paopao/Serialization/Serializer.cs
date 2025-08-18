using System;
using System.IO;
using UnityEngine;

public class Serializer
{
	public static Vector3 ReadVector3(BinaryReader reader)
	{
		return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

	public static void Write(BinaryWriter writer, Vector3 v)
	{
		writer.Write(v.x);
		writer.Write(v.y);
		writer.Write(v.z);
	}

	public static Quaternion ReadQuaternion(BinaryReader reader)
	{
		return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
	}

	public static void Write(BinaryWriter writer, Quaternion q)
	{
		writer.Write(q.x);
		writer.Write(q.y);
		writer.Write(q.z);
		writer.Write(q.w);
	}
}
