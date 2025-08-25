using System;
using UnityEngine;

public class WarpMessage : MonoBehaviourMessage
{
	public WarpMessage()
		: base(MonoBehaviourMessageType.WARP)
	{
	}

	public WarpMessage Initialize(Vector3 pos, Quaternion rot, bool flush, bool resetVel)
	{
		this.pos_ = pos;
		this.rot_ = rot;
		this.isFlush_ = flush;
		this.isResetVel_ = resetVel;
		return this;
	}

	public Vector3 pos_;

	public Quaternion rot_;

	public bool isFlush_;

	public bool isResetVel_;
}
