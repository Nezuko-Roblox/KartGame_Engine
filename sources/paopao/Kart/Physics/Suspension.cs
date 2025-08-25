using System;
using UnityEngine;

public struct Suspension
{
	public void InitVector2(ref Vector2 v, float x, float y)
	{
		v.x = x;
		v.y = y;
	}

	public void Initialize()
	{
		this.wheelOff = new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			this.InitVector2(ref this.wheelOff[i], (i % 2 != 0) ? 1f : (-1f), (i / 2 != 0) ? (-1f) : 1f);
		}
		this.wheelContact = new bool[4];
		this.wheelContactN = new Vector3[4];
		this.maxTravel = 0.3f;
		this.travel = new float[] { 0.3f, 0.3f, 0.3f, 0.3f };
		this.deltaTravel = new float[4];
	}

	public Vector2[] wheelOff;

	public Vector3 contactN;

	public bool[] wheelContact;

	public Vector3[] wheelContactN;

	public float maxTravel;

	public float[] travel;

	public float[] deltaTravel;
}
