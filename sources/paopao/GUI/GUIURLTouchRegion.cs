using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIURLTouchRegion : MouseNotifier
{
	public GUIURLTouchRegion(Rect _touchRegion)
	{
		this.touchRegion_ = _touchRegion;
		this.touchRegion_.y = (float)Screen.height - this.touchRegion_.y - this.touchRegion_.height;
	}

	public Rect TouchRegion
	{
		get
		{
			return this.touchRegion_;
		}
	}

	public bool Contains(Vector3 pos)
	{
		return this.touchRegion_.Contains(pos);
	}

	public int GetPriority()
	{
		return 1;
	}

	public bool IsEnabled()
	{
		return true;
	}

	private Rect touchRegion_;
}
