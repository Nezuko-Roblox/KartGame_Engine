using System;
using UnityEngine;

public class TopViewCameraman : GameCameraman
{
	public override void reset(int style)
	{
	}

	public override int getStyle()
	{
		return 0;
	}

	public override void calc(int tick, ref Vector3 retPos, ref Transform retOrt, ref float retFov)
	{
		base.calc(tick, ref retPos, ref retOrt, ref retFov);
		GoKart kart = base.getKart();
		if (kart == null)
		{
			return;
		}
		retPos = kart.m_kart.transform.position;
		retPos.y += 15f;
		retOrt.localRotation = Quaternion.LookRotation(-Vector3.up, -Vector3.forward);
		retFov = 75f;
	}

	public override string getDescription()
	{
		return "TopView Cameraman";
	}
}
