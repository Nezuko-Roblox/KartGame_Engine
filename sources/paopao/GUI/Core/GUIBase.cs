using System;
using UnityEngine;

public class GUIBase
{
	public static Vector3 ConvertWSToUS(Vector3 u)
	{
		return new Vector3(u.x, (float)Screen.height - u.y, u.z);
	}

	public static Vector2 ConvertWSToUS(Vector2 u)
	{
		return new Vector2(u.x, (float)Screen.height - u.y);
	}

	public static Rect ConvertWSToUS(float left, float top, float right, float bottom)
	{
		return new Rect(left, (float)Screen.height - bottom, right - left, bottom - top);
	}

	public static Rect ConvertWSToUS(Rect r)
	{
		return new Rect(r.xMin, (float)Screen.height - r.yMax, r.width, r.height);
	}

	public static Rect ConvertWSToUS(float[] f, int startIdx)
	{
		return GUIBase.ConvertWSToUS(f[startIdx], f[startIdx + 1], f[startIdx + 2], f[startIdx + 3]);
	}

	public static GUIType GetGUIType()
	{
		return GUIType.IPHONE;
	}
}
