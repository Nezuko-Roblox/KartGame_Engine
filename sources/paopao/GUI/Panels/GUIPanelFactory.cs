using System;
using System.Collections.Generic;
using UnityEngine;

public class GUIPanelFactory
{
	public GUIPanelFactory()
	{
		this.builds_.Add(0, new GUIPanelExBuilder());
	}

	public static GUIPanelFactory Instance
	{
		get
		{
			if (GUIPanelFactory.instance_ == null)
			{
				GUIPanelFactory.instance_ = new GUIPanelFactory();
			}
			return GUIPanelFactory.instance_;
		}
	}

	public void RegistBuilder(int type, GUIPanelBuilder builder)
	{
		if (!this.builds_.ContainsKey(type))
		{
			this.builds_.Add(type, builder);
		}
	}

	public void Clear()
	{
		this.builds_.Clear();
	}

	public GUIPanelEx CreateByWindowSpace(GUIPanelLayout layout, FiaTexture tex, Vector3 fontGap)
	{
		return this.CreateByWindowSpace(layout.builder_, layout.f_, tex, layout.layer_, fontGap);
	}

	public GUIPanelEx CreateByWindowSpace(float[] f, FiaTexture tex, int layer)
	{
		return this.CreateByWindowSpace(0, f, tex, layer);
	}

	public GUIPanelEx CreateByWindowSpace(int type, float[] f, FiaTexture tex, int layer)
	{
		return this.CreateByWindowSpace(type, f, tex, layer, GUIFontCalculator.DEFAULT_GAP);
	}

	public GUIPanelEx CreateByWindowSpace(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap)
	{
		if (tex != null && this.builds_.ContainsKey(type))
		{
			this.temporary_.Copy(f);
			if (this.temporary_.Length == 8)
			{
				this.temporary_[1] = (float)Screen.height - this.temporary_[1];
				this.temporary_[3] = (float)Screen.height - this.temporary_[3];
				this.temporary_[5] = tex.OrgRect.height - this.temporary_[5];
				this.temporary_[7] = tex.OrgRect.height - this.temporary_[7];
			}
			else
			{
				if (this.temporary_.Length != 6)
				{
					return null;
				}
				this.temporary_[1] = (float)Screen.height - this.temporary_[1];
				this.temporary_[3] = tex.OrgRect.height - this.temporary_[3];
				this.temporary_[5] = tex.OrgRect.height - this.temporary_[5];
			}
			GUIPanelEx guipanelEx = this.builds_[type].Build();
			guipanelEx.Initialize(this.temporary_, tex.UV, tex.OrgRect, layer, fontGap);
			return guipanelEx;
		}
		return null;
	}

	public GUIPanelEx CreateByWindowSpace(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap, int screenWidth, int screenHeight)
	{
		if (tex != null && this.builds_.ContainsKey(type))
		{
			this.temporary_.Copy(f);
			if (this.temporary_.Length == 8)
			{
				this.temporary_[1] = (float)Screen.height - this.temporary_[1];
				this.temporary_[3] = (float)Screen.height - this.temporary_[3];
				this.temporary_[5] = tex.OrgRect.height - this.temporary_[5];
				this.temporary_[7] = tex.OrgRect.height - this.temporary_[7];
			}
			else
			{
				if (this.temporary_.Length != 6)
				{
					return null;
				}
				this.temporary_[1] = (float)Screen.height - this.temporary_[1];
				this.temporary_[3] = tex.OrgRect.height - this.temporary_[3];
				this.temporary_[5] = tex.OrgRect.height - this.temporary_[5];
			}
			GUIPanelEx guipanelEx = this.builds_[type].Build();
			guipanelEx.Initialize(this.temporary_, tex.UV, tex.OrgRect, layer, fontGap);
			return guipanelEx;
		}
		return null;
	}

	private Dictionary<int, GUIPanelBuilder> builds_ = new Dictionary<int, GUIPanelBuilder>();

	private ArrayEx<float> temporary_ = new ArrayEx<float>(16);

	public static GUIPanelFactory instance_;
}
