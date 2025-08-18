using System;
using UnityEngine;

internal class DefaultNameMouseNotifier : MouseNotifier
{
	public DefaultNameMouseNotifier(Rect rc)
	{
		this.rc_ = GUIBase.ConvertWSToUS(rc);
	}

	public bool Contains(Vector3 pos)
	{
		return this.rc_.Contains(pos);
	}

	public int GetPriority()
	{
		return 1;
	}

	public bool IsEnabled()
	{
		return true;
	}

	private Rect rc_;
}
