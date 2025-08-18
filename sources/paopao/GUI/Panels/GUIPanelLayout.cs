using System;

public class GUIPanelLayout
{
	public GUIPanelLayout(float[] f, int layer, int builder)
	{
		this.f_ = f;
		this.layer_ = layer;
		this.builder_ = builder;
	}

	public GUIPanelLayout(float[] f, int layer)
	{
		this.f_ = f;
		this.layer_ = layer;
	}

	public float[] f_;

	public int layer_ = 1;

	public int builder_;
}
