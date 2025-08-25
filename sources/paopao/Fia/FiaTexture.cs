using System;
using UnityEngine;

public class FiaTexture
{
	public FiaTexture(GUIAtlas atlas, string name)
	{
		this.atlas_ = atlas;
		int idx = this.atlas_.GetIdx(name);
		this.uv_ = this.atlas_.uvs_[idx];
		this.orgRect_ = this.atlas_.orgRect_[idx];
	}

	public FiaTexture(Texture tex)
	{
		this.tex_ = tex;
		this.uv_ = new Rect(0f, 0f, 1f, 1f);
		this.orgRect_ = new Rect(0f, 0f, (float)this.tex_.width, (float)this.tex_.height);
	}

	public Rect UV
	{
		get
		{
			return this.uv_;
		}
	}

	public Rect OrgRect
	{
		get
		{
			return this.orgRect_;
		}
	}

	private GUIAtlas atlas_;

	private Texture tex_;

	private Rect uv_;

	private Rect orgRect_;
}
