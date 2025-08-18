using System;
using UnityEngine;

public struct Matrix3
{
	public static Matrix3 CreateMtx()
	{
		return new Matrix3
		{
			m = new float[3, 3]
		};
	}

	public static Matrix3 CreateMtxCol(Vector3 col0, Vector3 col1, Vector3 col2)
	{
		Matrix3 matrix = default(Matrix3);
		matrix.m = new float[3, 3];
		for (int i = 0; i < 3; i++)
		{
			matrix.m[i, 0] = col0[i];
			matrix.m[i, 1] = col1[i];
			matrix.m[i, 2] = col2[i];
		}
		return matrix;
	}

	public static Matrix3 CreateMtxIdentity()
	{
		return Matrix3.CreateMtx(1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f);
	}

	public static Matrix3 CreateMtx(float _11, float _12, float _13, float _21, float _22, float _23, float _31, float _32, float _33)
	{
		Matrix3 matrix = default(Matrix3);
		matrix.m = new float[3, 3];
		matrix.m[0, 0] = _11;
		matrix.m[0, 1] = _12;
		matrix.m[0, 2] = _13;
		matrix.m[1, 0] = _21;
		matrix.m[1, 1] = _22;
		matrix.m[1, 2] = _23;
		matrix.m[2, 0] = _31;
		matrix.m[2, 1] = _32;
		matrix.m[2, 2] = _33;
		return matrix;
	}

	public void SetValue(ref Matrix3 mat)
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				this.m[i, j] = mat.m[i, j];
			}
		}
	}

	public Vector3 getRow(int row)
	{
		return new Vector3(this.m[row, 0], this.m[row, 1], this.m[row, 2]);
	}

	public Vector3 getCol(int col)
	{
		return new Vector3(this.m[0, col], this.m[1, col], this.m[2, col]);
	}

	public void setCol(int col, Vector3 v)
	{
		for (int i = 0; i < 3; i++)
		{
			this.m[i, col] = v[i];
		}
	}

	public void setCol(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		this.setCol(0, v1);
		this.setCol(1, v2);
		this.setCol(2, v3);
	}

	public static Vector3 operator *(Matrix3 mat, Vector3 v)
	{
		return new Vector3(mat.m[0, 0] * v.x + mat.m[0, 1] * v.y + mat.m[0, 2] * v.z, mat.m[1, 0] * v.x + mat.m[1, 1] * v.y + mat.m[1, 2] * v.z, mat.m[2, 0] * v.x + mat.m[2, 1] * v.y + mat.m[2, 2] * v.z);
	}

	public static Matrix3 operator *(Matrix3 mat1, Matrix3 mat2)
	{
		return Matrix3.CreateMtx(mat1.m[0, 0] * mat2.m[0, 0] + mat1.m[0, 1] * mat2.m[1, 0] + mat1.m[0, 2] * mat2.m[2, 0], mat1.m[0, 0] * mat2.m[0, 1] + mat1.m[0, 1] * mat2.m[1, 1] + mat1.m[0, 2] * mat2.m[2, 1], mat1.m[0, 0] * mat2.m[0, 2] + mat1.m[0, 1] * mat2.m[1, 2] + mat1.m[0, 2] * mat2.m[2, 2], mat1.m[1, 0] * mat2.m[0, 0] + mat1.m[1, 1] * mat2.m[1, 0] + mat1.m[1, 2] * mat2.m[2, 0], mat1.m[1, 0] * mat2.m[0, 1] + mat1.m[1, 1] * mat2.m[1, 1] + mat1.m[1, 2] * mat2.m[2, 1], mat1.m[1, 0] * mat2.m[0, 2] + mat1.m[1, 1] * mat2.m[1, 2] + mat1.m[1, 2] * mat2.m[2, 2], mat1.m[2, 0] * mat2.m[0, 0] + mat1.m[2, 1] * mat2.m[1, 0] + mat1.m[2, 2] * mat2.m[2, 0], mat1.m[2, 0] * mat2.m[0, 1] + mat1.m[2, 1] * mat2.m[1, 1] + mat1.m[2, 2] * mat2.m[2, 1], mat1.m[2, 0] * mat2.m[0, 2] + mat1.m[2, 1] * mat2.m[1, 2] + mat1.m[2, 2] * mat2.m[2, 2]);
	}

	public float[,] m;
}
