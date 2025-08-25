using System;
using System.IO;
using UnityEngine;

public class PassPlane
{
	public PassPlane(ref BinaryReader streamReader)
	{
		float[] array = new float[18];
		for (int i = 0; i < 18; i++)
		{
			array[i] = streamReader.ReadSingle();
		}
		this.Initialize(array);
	}

	public PassPlane(string str)
	{
		char[] array = new char[] { ' ', '\t' };
		string[] array2 = str.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length == 18)
		{
			float[] array3 = new float[18];
			for (int i = 0; i < 18; i++)
			{
				array3[i] = float.Parse(array2[i]);
			}
			this.Initialize(array3);
		}
	}

	private void Initialize(float[] tokenF)
	{
		if (tokenF.Length != 18)
		{
			return;
		}
		this.SetVector(ref this.pt1_, tokenF, 0, 1, 2);
		this.SetVector(ref this.pt2_, tokenF, 0, 2, 3);
		this.SetVector(ref this.normal_, tokenF, 4);
		this.SetVector(ref this.toNextPlane_, tokenF, 5);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				ref Vector3 ptr = ref this.centerPos_;
				int num2;
				int num = (num2 = j);
				float num3 = ptr[num2];
				this.centerPos_[num] = num3 + tokenF[i * 3 + j];
			}
		}
		this.centerPos_ /= 4f;
	}

	public void SetVector(ref Vector3 v, float[] t, int idx)
	{
		int num = idx * 3;
		for (int i = 0; i < 3; i++)
		{
			v[i] = t[num + i];
		}
	}

	public void SetVector(ref Vector3[] v, float[] t, int v1, int v2, int v3)
	{
		int[] array = new int[] { v1, v2, v3 };
		for (int i = 0; i < 3; i++)
		{
			this.SetVector(ref v[i], t, array[i]);
		}
	}

	private void MakeOutputString()
	{
		this.toString_ = string.Empty;
		for (int i = 0; i < 3; i++)
		{
			this.toString_ += this.pt1_[i].ToString();
		}
		this.toString_ += "\n";
		for (int j = 0; j < 3; j++)
		{
			this.toString_ += this.pt2_[j].ToString();
		}
		this.toString_ += "\n";
		this.toString_ += this.normal_.ToString();
		this.toString_ += "\n";
		this.toString_ += this.toNextPlane_.ToString();
	}

	public override string ToString()
	{
		this.MakeOutputString();
		return this.toString_;
	}

	public Vector3 GetRegenPos()
	{
		if (this.isRegenPosSetting_)
		{
			return this.regenPos_;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(this.centerPos_, -Vector3.up, out raycastHit, Mathf.Abs(this.centerPos_.y - this.pt1_[0].y), 256))
		{
			this.regenPos_ = raycastHit.point;
		}
		else
		{
			this.regenPos_ = this.centerPos_;
		}
		this.isRegenPosSetting_ = true;
		return this.regenPos_;
	}

	public bool GetRegenPos(ref Vector3 regenPos)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(this.centerPos_, -Vector3.up, out raycastHit, Mathf.Abs(this.centerPos_.y - this.pt1_[0].y), 256))
		{
			regenPos = raycastHit.point;
			return true;
		}
		regenPos = this.centerPos_;
		return false;
	}

	public float GetDistanceFromCenterToBottom()
	{
		return Mathf.Abs(this.centerPos_.y - this.pt1_[0].y);
	}

	public const int TOKEN_NO = 18;

	private const int VERTEX_NO = 4;

	public Vector3[] pt1_ = new Vector3[3];

	public Vector3[] pt2_ = new Vector3[3];

	public Vector3 normal_;

	public Vector3 toNextPlane_;

	public Vector3 centerPos_ = Vector3.zero;

	private bool isRegenPosSetting_;

	public Vector3 regenPos_ = Vector3.zero;

	private string toString_;
}
