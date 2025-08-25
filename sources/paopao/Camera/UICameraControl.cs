using System;
using UnityEngine;

public class UICameraControl : MonoBehaviour
{
	private void Start()
	{
		base.camera.pixelRect = new Rect((float)this.left_, (float)this.top_, (float)this.width_, (float)this.height_);
	}

	public int left_;

	public int top_;

	public int width_;

	public int height_;
}
