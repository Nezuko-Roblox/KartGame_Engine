using System;
using UnityEngine;

public class MathHelper
{
	// Note: this type is marked as 'beforefieldinit'.
	static MathHelper()
	{
		int[] array = new int[3];
		array[0] = 1;
		array[1] = 2;
		MathHelper.next = array;
	}

	public static Quaternion CreateQuaternion(float w, Vector3 v)
	{
		return new Quaternion(v.x, v.y, v.z, w);
	}

	public static void QuaMulScala(ref Quaternion q, float v)
	{
		q.x *= v;
		q.y *= v;
		q.z *= v;
		q.w *= v;
	}

	public static void QuaNormalize(ref Quaternion q)
	{
		MathHelper.QuaMulScala(ref q, 1f / Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z));
	}

	public static void QuaAdd(ref Quaternion q1, Quaternion q2)
	{
		q1.w += q2.w;
		q1.x += q2.x;
		q1.y += q2.y;
		q1.z += q2.z;
	}

	public static void QuaUnaryNegative(ref Quaternion q)
	{
		q.x *= -1f;
		q.y *= -1f;
		q.z *= -1f;
		q.w *= -1f;
	}

	public static Quaternion ToQuaternion(Vector3 left, Vector3 dir, Vector3 up)
	{
		float[] array = new float[4];
		float[,] array2 = new float[3, 3];
		for (int i = 0; i < 3; i++)
		{
			array2[i, 0] = left[i];
			array2[i, 1] = up[i];
			array2[i, 2] = dir[i];
		}
		float num = array2[0, 0] + array2[1, 1] + array2[2, 2];
		if (num > 0f)
		{
			float num2 = Mathf.Sqrt(num + 1f);
			array[0] = 0.5f * num2;
			num2 = 0.5f / num2;
			array[1] = (array2[2, 1] - array2[1, 2]) * num2;
			array[2] = (array2[0, 2] - array2[2, 0]) * num2;
			array[3] = (array2[1, 0] - array2[0, 1]) * num2;
		}
		else
		{
			int num3 = 0;
			if (array2[1, 1] > array2[0, 0])
			{
				num3 = 1;
			}
			if (array2[2, 2] > array2[num3, num3])
			{
				num3 = 2;
			}
			int num4 = MathHelper.next[num3];
			int num5 = MathHelper.next[num4];
			float num2 = Mathf.Sqrt(array2[num3, num3] - array2[num4, num4] - array2[num5, num5] + 1f);
			array[num3 + 1] = 0.5f * num2;
			num2 = 0.5f / num2;
			array[0] = (array2[num5, num4] - array2[num4, num5]) * num2;
			array[num4 + 1] = (array2[num4, num3] + array2[num3, num4]) * num2;
			array[num5 + 1] = (array2[num5, num3] + array2[num3, num5]) * num2;
		}
		return new Quaternion(array[1], array[2], array[3], array[0]);
	}

	public static Quaternion ToQuaternionEx(Vector3 left, Vector3 dir, Vector3 up)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		for (int i = 0; i < 3; i++)
		{
			identity[i, 0] = left[i];
			identity[i, 2] = dir[i];
			identity[i, 1] = up[i];
		}
		Quaternion quaternion = default(Quaternion);
		quaternion.w = Mathf.Sqrt(Mathf.Max(0f, 1f + identity[0, 0] + identity[1, 1] + identity[2, 2])) / 2f;
		quaternion.x = Mathf.Sqrt(Mathf.Max(0f, 1f + identity[0, 0] - identity[1, 1] - identity[2, 2])) / 2f;
		quaternion.y = Mathf.Sqrt(Mathf.Max(0f, 1f - identity[0, 0] + identity[1, 1] - identity[2, 2])) / 2f;
		quaternion.z = Mathf.Sqrt(Mathf.Max(0f, 1f - identity[0, 0] - identity[1, 1] + identity[2, 2])) / 2f;
		quaternion.x *= Mathf.Sign(quaternion.x * (identity[2, 1] - identity[1, 2]));
		quaternion.y *= Mathf.Sign(quaternion.y * (identity[0, 2] - identity[2, 0]));
		quaternion.z *= Mathf.Sign(quaternion.z * (identity[1, 0] - identity[0, 1]));
		return quaternion;
	}

	public static Quaternion ToQuaternion(string x, string y, string z, string w)
	{
		return new Quaternion(float.Parse(x), float.Parse(y), float.Parse(z), float.Parse(w));
	}

	public static bool RayFaceIntersect(Vector3[] V, Vector3 S, Vector3 D)
	{
		Vector3 vector = V[1] - V[0];
		Vector3 vector2 = V[2] - V[0];
		Vector3 vector3 = Vector3.Cross(D, vector2);
		float num = Vector3.Dot(vector, vector3);
		if (num > -0.0001f && (double)num < 0.0001)
		{
			return false;
		}
		float num2 = 1f / num;
		Vector3 vector4 = S - V[0];
		float num3 = num2 * Vector3.Dot(vector4, vector3);
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		Vector3 vector5 = Vector3.Cross(vector4, vector);
		float num4 = num2 * Vector3.Dot(D, vector5);
		if (num4 < 0f || num3 + num4 > 1f)
		{
			return false;
		}
		float num5 = num2 * Vector3.Dot(vector2, vector5);
		return num5 >= 0f && num5 <= 1f;
	}

	public static bool IsBetweenII(int value, int min, int max)
	{
		return value >= min && value <= max;
	}

	public static bool IsBetweenII(float value, float min, float max)
	{
		return value >= min && value <= max;
	}

	public static bool IsBetweenIE(int value, int min, int max)
	{
		return value >= min && value < max;
	}

	public static bool IsBetweenIE(float value, float min, float max)
	{
		return value >= min && value < max;
	}

	public static Vector3 BEZ3(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		return a * (1f - t) * (1f - t) * (1f - t) + b * 3f * (1f - t) * (1f - t) * t + c * 3f * (1f - t) * t * t + d * t * t * t;
	}

	public static int[] next;
}
