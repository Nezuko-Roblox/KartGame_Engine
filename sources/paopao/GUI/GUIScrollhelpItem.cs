using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIScrollhelpItem : GUIListCtrl
{
	protected override int GetCompareIndex()
	{
		return 1;
	}

	protected override Rect GetAvailableRegion()
	{
		return GUIBase.ConvertWSToUS(this.viewRegion_);
	}

	protected override void GetMinMaxRange(out float minRange, out float maxRange)
	{
		maxRange = this.imageRegion_.yMax - this.viewRegion_.height;
		minRange = this.imageRegion_.yMin;
	}

	protected override void InitializeListctrl()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.panel_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[]
		{
			this.viewRegion_.xMin,
			this.viewRegion_.yMin,
			0f,
			0f,
			fiaTexture.OrgRect.width,
			fiaTexture.OrgRect.height
		}, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.panel_);
	}

	protected override ScrollBarInfo GetScrollbarInfo()
	{
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.material_ = this.scrollMat_;
		scrollBarInfo.texture_ = new FiaTexture(this.scrollMat_.mainTexture);
		scrollBarInfo.scrollbarInfo_ = new float[]
		{
			this.viewRegion_.xMax + 15f,
			this.viewRegion_.yMin,
			this.viewRegion_.xMax + 15f + this.scrollbarTex_.width,
			this.viewRegion_.yMax,
			this.scrollbarTex_.xMin,
			this.scrollbarTex_.yMin,
			this.scrollbarTex_.yMax
		};
		scrollBarInfo.viewRegion_ = this.GetAvailableRegion();
		float num;
		float num2;
		this.GetMinMaxRange(out num, out num2);
		scrollBarInfo.listRegion_ = new Rect(0f, 0f, 0f, num2 - num);
		return scrollBarInfo;
	}

	public override void DoInit()
	{
		float num = this.viewRegion_.left * (float)Screen.width / 800f;
		float num2 = this.viewRegion_.top * (float)Screen.height / 480f;
		float num3 = this.viewRegion_.width * (float)Screen.width / 800f;
		float num4 = this.viewRegion_.height * (float)Screen.height / 480f;
		this.viewRegion_ = new Rect(num, num2, num3, num4);
		base.DoInit();
		base.ResetLocalPosition(0f);
	}

	private GUIPanelEx panel_;

	public Rect imageRegion_;

	public Rect viewRegion_;

	public Rect scrollbarTex_;

	public Material scrollMat_;
}
