using System;
using UnityEngine;

public class DriftData
{
	public float[] tu = new float[4];

	public float[] tv = new float[4];

	public float scale;

	public Matrix4x4 trans;

	public uint age;

	public Matrix4x4 finalTrans;
}
