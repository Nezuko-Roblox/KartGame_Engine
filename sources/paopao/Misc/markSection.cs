using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
internal class markSection
{
	public Vector3 pos = Vector3.zero;

	public Vector3 normal = Vector3.zero;

	public Vector4 tangent = Vector4.zero;

	public Vector3 posl = Vector3.zero;

	public Vector3 posr = Vector3.zero;

	public float intensity;

	public int lastIndex;
}
