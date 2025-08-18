using System;
using UnityEngine;

public class PassPlaneSequenceElement
{
	public PassPlaneSequenceElement(int prev, int next)
	{
		this.prev_ = new int[] { prev };
		this.next_ = new int[] { next };
	}

	public PassPlaneSequenceElement(string[] prev, string[] post)
	{
		if (prev.Length != 0 && post.Length != 0)
		{
			this.SetArray(prev, out this.prev_);
			this.SetArray(post, out this.next_);
		}
		else
		{
			Debug.LogError("size is 0 " + FiaUtil.AddSquareBracket((float)prev.Length) + FiaUtil.AddSquareBracket((float)post.Length));
		}
	}

	public PassPlaneSequenceElement(int[] prev, int[] post)
	{
		this.prev_ = prev;
		this.next_ = post;
	}

	public void SetArray(string[] stringArray, out int[] intArray)
	{
		intArray = new int[stringArray.Length];
		for (int i = 0; i < stringArray.Length; i++)
		{
			if (!int.TryParse(stringArray[i], out intArray[i]))
			{
			}
		}
	}

	public int[] Next
	{
		get
		{
			return this.next_;
		}
	}

	public int[] Prev
	{
		get
		{
			return this.prev_;
		}
	}

	public bool IsBranch()
	{
		return this.next_ != null && this.next_.Length > 1;
	}

	public bool IsMerge()
	{
		return this.prev_ != null && this.prev_.Length > 1;
	}

	private int[] prev_;

	private int[] next_;

	public BranchId branchId_;
}
