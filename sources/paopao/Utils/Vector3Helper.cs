using System;
using UnityEngine;

public class Vector3Helper
{
	public static void SetVector3(ref Vector3 v, float x, float y, float z)
	{
		v.x = x;
		v.y = y;
		v.z = z;
	}

	public static void SetVector3(ref Vector3 v, float t)
	{
		Vector3Helper.SetVector3(ref v, t, t, t);
	}

	public static Vector3 CreateVector3(string x, string y, string z)
	{
		return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
	}

	public static string ToStringVector3(Vector3 v)
	{
		return string.Concat(new string[]
		{
			" [ ",
			v.x.ToString(),
			" , ",
			v.y.ToString(),
			" , ",
			v.z.ToString(),
			" ] "
		});
	}

	public static bool IsZero(Vector3 v)
	{
		return v.x == 0f && v.y == 0f && v.z == 0f;
	}
}
