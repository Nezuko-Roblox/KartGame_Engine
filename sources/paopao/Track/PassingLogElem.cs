using System;
using UnityEngine;

public class PassingLogElem
{
	public override string ToString()
	{
		return string.Format("{0} {1} {2} {3} {4} ", new object[]
		{
			this.passingIndex_,
			Vector3Helper.ToStringVector3(this.pos1_),
			Vector3Helper.ToStringVector3(this.pos2_),
			this.isCorrect_,
			this.resultPassing_
		});
	}

	public Vector3 pos1_ = Vector3.zero;

	public Vector3 pos2_ = Vector3.zero;

	public int passingIndex_;

	public bool isCorrect_ = true;

	public int resultPassing_;
}
