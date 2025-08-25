using System;
using UnityEngine;

public class CrashEffect : MonoBehaviour
{
	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (this.crashWhite_ == null || this.crashStar_ == null || this.crashPang_ == null)
		{
			return;
		}
		if (KartManager.Instance.goPlayKart_.isCrash_ && KartManager.Instance.goPlayKart_.crashVelocity_ >= this.EMIT_LIMIT_VELOCITY)
		{
			InGameStatistics.Instance.Others.Collision++;
			this.crashStar_.EmitStart();
			this.crashWhite_.EmitStart();
			this.crashPang_.Run();
		}
	}

	public EmitController crashStar_;

	public EmitController crashWhite_;

	public CrashPang crashPang_;

	private float EMIT_LIMIT_VELOCITY = 10f;
}
