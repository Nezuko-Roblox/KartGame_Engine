using System;
using UnityEngine;

public struct PerAirData
{
	public void Initialize()
	{
		this.trans = Matrix4x4.identity;
	}

	public Matrix4x4 trans;

	public int uvIdx;
}
