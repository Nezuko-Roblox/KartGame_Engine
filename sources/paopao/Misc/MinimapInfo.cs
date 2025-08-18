using System;
using UnityEngine;

public class MinimapInfo
{
	public Vector3 Center
	{
		get
		{
			return new Vector3(0f, 0f, 0f);
		}
	}

	public float Scale
	{
		get
		{
			return 0.3f;
		}
	}

	public Vector3 MinimapSize
	{
		get
		{
			return new Vector3(256f, 0f, 256f);
		}
	}

	public float Margin
	{
		get
		{
			return 4f;
		}
	}
}
