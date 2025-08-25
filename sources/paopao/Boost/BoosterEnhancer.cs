using System;
using UnityEngine;

public class BoosterEnhancer : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		int num = 1 << collider.gameObject.layer;
		if (num == 8192)
		{
			RigidbodyFPSWalker rigidbodyFPSWalker = collider.gameObject.GetComponent(typeof(RigidbodyFPSWalker)) as RigidbodyFPSWalker;
			if (rigidbodyFPSWalker != null && (!this._checkBoosterUse || rigidbodyFPSWalker.goPlayKart_.isRealBoost()))
			{
				rigidbodyFPSWalker.goPlayKart_.m_KartWLVel *= this._velocityFactor;
			}
		}
	}

	public bool _checkBoosterUse = true;

	public float _velocityFactor = 1f;
}
