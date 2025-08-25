using System;
using UnityEngine;

public class AISpeedEnhancer : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		int num = 1 << collider.gameObject.layer;
		if (num == 16384)
		{
			AIController aicontroller = collider.transform.parent.gameObject.GetComponent(typeof(AIController)) as AIController;
			if (aicontroller != null)
			{
				float currentDeltaTick = aicontroller.GetCurrentDeltaTick();
				aicontroller.SetSpeedController(new MaintainedLerpSpeedController(currentDeltaTick, this._goalDeltaTick, this._lerpDuration, this._maintainDuration));
			}
		}
	}

	public float _goalDeltaTick = 0.035f;

	public float _lerpDuration = 1.5f;

	public float _maintainDuration = 3f;
}
