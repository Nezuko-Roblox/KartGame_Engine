using System;
using UnityEngine;

public class RectHelper
{
	public static Vector2 GetSize(Rect rc)
	{
		return new Vector2(rc.width, rc.height);
	}

	public static bool IsValidRect(Rect rc)
	{
		return rc.width != 0f && rc.height != 0f;
	}

	public static Rect CreateRect(string[] tokens)
	{
		return RectHelper.CreateRect(tokens, 0);
	}

	public static Rect CreateRect(string[] tokens, int startIdx)
	{
		return new Rect
		{
			xMin = (float)int.Parse(tokens[startIdx]),
			yMin = (float)int.Parse(tokens[startIdx + 1]),
			xMax = (float)int.Parse(tokens[startIdx + 2]),
			yMax = (float)int.Parse(tokens[startIdx + 3])
		};
	}

	public static Rect zero = new Rect(0f, 0f, 0f, 0f);
}
