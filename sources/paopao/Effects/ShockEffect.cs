using System;
using UnityEngine;

public class ShockEffect : MonoBehaviour
{
	private void Start()
	{
		if (base.particleEmitter == null)
		{
		}
	}

	private void Update()
	{
		if (base.particleEmitter == null)
		{
			return;
		}
		if (KartManager.Instance.goPlayKart_.IsShock() && KartManager.Instance.goPlayKart_.shockVelocity_ >= 10f)
		{
			base.particleEmitter.Emit();
		}
	}
}
