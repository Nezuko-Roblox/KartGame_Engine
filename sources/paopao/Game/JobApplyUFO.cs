using System;
using UnityEngine;

public class JobApplyUFO : PlayerJob
{
	public override void Initialize()
	{
		KartManager.Instance.goPlayKart_.DragFactor *= 4f;
	}

	public override void Update()
	{
		this.duration_ -= Time.deltaTime;
		if (this.duration_ < 0f)
		{
			KartManager.Instance.goPlayKart_.DragFactor /= 4f;
		}
	}

	public override bool IsFinish()
	{
		return this.duration_ < 0f;
	}

	private const float UFO_DRAG_FACTOR = 4f;

	private float duration_ = 3f;
}
