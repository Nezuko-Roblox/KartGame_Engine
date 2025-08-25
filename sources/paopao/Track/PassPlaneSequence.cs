using System;
using System.IO;
using UnityEngine;

public class PassPlaneSequence
{
	public PassPlaneSequence()
	{
		this.startPassingPlane_ = new int[1];
		this.endPassingPlane_ = new int[1];
	}

	public PassPlaneSequence(int length)
	{
		this.sequence_ = new PassPlaneSequenceElement[length];
		for (int i = 0; i < length; i++)
		{
			this.sequence_[i] = new PassPlaneSequenceElement((i - 1 + length) % length, (i + 1) % length);
		}
		this.startPassingPlane_ = new int[1];
		this.endPassingPlane_ = new int[] { length - 1 };
		this.GenerateBranchId();
	}

	public PassPlaneSequence(BinaryAsset sequenceInfo)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(sequenceInfo.content_));
		if (binaryReader != null)
		{
			binaryReader.ReadInt16();
			int num = (int)binaryReader.ReadInt16();
			this.sequence_ = new PassPlaneSequenceElement[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = (int)binaryReader.ReadInt16();
				int num3 = (int)binaryReader.ReadInt16();
				int[] array = new int[num3];
				for (int j = 0; j < num3; j++)
				{
					array[j] = (int)binaryReader.ReadInt16();
				}
				int num4 = (int)binaryReader.ReadInt16();
				int[] array2 = new int[num4];
				for (int k = 0; k < num4; k++)
				{
					array2[k] = (int)binaryReader.ReadInt16();
				}
				this.sequence_[num2] = new PassPlaneSequenceElement(array, array2);
			}
			binaryReader.Close();
		}
		this.startPassingPlane_ = new int[1];
		this.endPassingPlane_ = new int[] { this.sequence_.Length - 1 };
		this.GenerateBranchId();
	}

	public PassPlaneSequence(TextAsset sequenceInfo)
	{
		StringReader stringReader = new StringReader(sequenceInfo.text);
		if (stringReader != null)
		{
			string text = string.Empty;
			text = stringReader.ReadLine();
			int num = int.Parse(text);
			this.sequence_ = new PassPlaneSequenceElement[num];
			text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				text = stringReader.ReadLine();
				if (text != null)
				{
					char[] array = new char[] { ' ', '\t' };
					string[] array2 = text.Split(array);
					if (array2.Length == 3)
					{
						int num2 = int.Parse(array2[0]);
						if (this.sequence_[num2] != null)
						{
							Debug.LogError("already exist sequence index : " + num2.ToString());
						}
						char[] array3 = new char[] { '/' };
						this.sequence_[num2] = new PassPlaneSequenceElement(array2[1].Split(array3, StringSplitOptions.RemoveEmptyEntries), array2[2].Split(array3, StringSplitOptions.RemoveEmptyEntries));
					}
					else
					{
						Debug.LogError("token size is not 3 " + FiaUtil.AddSquareBracket((float)array2.Length) + text);
					}
				}
			}
		}
		stringReader.Close();
		this.startPassingPlane_ = new int[1];
		this.endPassingPlane_ = new int[] { this.sequence_.Length - 1 };
		this.GenerateBranchId();
	}

	private void GenerateBranchIdRecursive(int planeIdx, BranchIdElem[] elems, int elemNo)
	{
		PassPlaneSequenceElement passPlaneSequenceElement = this.sequence_[planeIdx];
		if (passPlaneSequenceElement.branchId_ != null)
		{
			return;
		}
		if (passPlaneSequenceElement.IsMerge())
		{
			elemNo--;
		}
		passPlaneSequenceElement.branchId_ = new BranchId(elems, elemNo);
		if (passPlaneSequenceElement.IsBranch())
		{
			elems[elemNo].node_ = (byte)planeIdx;
			elemNo++;
		}
		for (int i = 0; i < passPlaneSequenceElement.Next.Length; i++)
		{
			if (passPlaneSequenceElement.IsBranch())
			{
				elems[elemNo - 1].branch_ = (byte)passPlaneSequenceElement.Next[i];
			}
			this.GenerateBranchIdRecursive(passPlaneSequenceElement.Next[i], elems, elemNo);
		}
	}

	private void GenerateBranchId()
	{
		BranchIdElem[] array = new BranchIdElem[16];
		this.GenerateBranchIdRecursive(0, array, 0);
	}

	public int[] GetPrev(int passPlaneIdx)
	{
		if (!MathHelper.IsBetweenIE(passPlaneIdx, 0, this.sequence_.Length))
		{
			return this.endPassingPlane_;
		}
		return this.sequence_[passPlaneIdx].Prev;
	}

	public int[] GetNext(int passPlaneIdx)
	{
		if (!MathHelper.IsBetweenIE(passPlaneIdx, 0, this.sequence_.Length))
		{
			return this.startPassingPlane_;
		}
		return this.sequence_[passPlaneIdx].Next;
	}

	public bool IsNextPlane(int prevPlane, int nextPlane)
	{
		if (prevPlane == -1)
		{
			return this.IsFirstPlane(nextPlane);
		}
		int[] next = this.sequence_[prevPlane].Next;
		foreach (int num in next)
		{
			if (nextPlane == num)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsFirstPlane(int plane)
	{
		return plane == 0;
	}

	public bool IsLastPlane(int plane)
	{
		return this.sequence_.Length - 1 == plane;
	}

	public bool IsBranch(int plane)
	{
		return this.sequence_[plane].IsBranch();
	}

	public bool IsMerge(int plane)
	{
		return this.sequence_[plane].IsMerge();
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < this.sequence_.Length; i++)
		{
			text += string.Format("{0} {1}\n", i, this.sequence_[i].branchId_.ToString());
		}
		return text;
	}

	public bool IsExclusivePlane(int lparam, int rparam)
	{
		return lparam != -1 && rparam != -1 && BranchId.IsExclusive(this.sequence_[lparam].branchId_, this.sequence_[rparam].branchId_);
	}

	private PassPlaneSequenceElement[] sequence_;

	private int[] startPassingPlane_;

	private int[] endPassingPlane_;

	private enum PassPlaneSequenceTokenType
	{
		INDEX,
		PREV_PLANE,
		POST_PLANE,
		SIZE
	}
}
