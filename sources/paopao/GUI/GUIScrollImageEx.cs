using System;
using UnityEngine;

public class GUIScrollImageEx : GUIListCtrl
{
	public GUIScrollImageEx()
	{
		this.urlForCells_ = new string[] { "http://www.nexon.com/", "http://www.nexonmobile.com/", "http://www.nexon.net/", "http://kart.nexon.com", "http://global.nexonmobile.com/App/?app=kart_r" };
		this.URL_TOUCH_REGION_INFO_FOR_IPHONE = new Rect[]
		{
			new Rect(0f, 2f * (float)Screen.height / 480f, 578f * (float)Screen.width / 800f, 120f * (float)Screen.height / 480f),
			new Rect(0f, 122f * (float)Screen.height / 480f, 578f * (float)Screen.width / 800f, 120f * (float)Screen.height / 480f),
			new Rect(0f, 254f * (float)Screen.height / 480f, 578f * (float)Screen.width / 800f, 120f * (float)Screen.height / 480f),
			new Rect(0f, 386f * (float)Screen.height / 480f, 578f * (float)Screen.width / 800f, 120f * (float)Screen.height / 480f),
			new Rect(0f, 1518f * (float)Screen.height / 480f, 578f * (float)Screen.width / 800f, 120f * (float)Screen.height / 480f)
		};
	}

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
		maxRange = this.imageRegion_.yMax - this.viewRegion_.height + 150f;
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
		this.urlSelectedPanel_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 288f, 220f, 290f, 222f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.urlSelectedPanel_);
		this.urlSelectedPanel_.VerticeColor = FiaColor.darkGrey;
		this.urlSelectedPanel_.Alpha = 0.7f;
		this.urlSelectedPanel_.Visible = false;
	}

	protected override void Update()
	{
		base.Update();
		int pushedKey = this.mouseManager_.GetPushedKey();
		if (pushedKey > -1)
		{
			Rect touchRegion = this.urlTouchRegions_[pushedKey].TouchRegion;
			touchRegion.y = (float)Screen.height - touchRegion.y - touchRegion.height;
			this.urlSelectedPanel_.SetRectByWindowSpace(touchRegion.xMin, touchRegion.yMin, touchRegion.xMax, touchRegion.yMax);
			this.urlSelectedPanel_.Visible = true;
		}
		else
		{
			this.urlSelectedPanel_.Visible = false;
		}
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey > -1)
		{
			Application.OpenURL(this.urlForCells_[selectedKey]);
		}
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
		this.urlTouchRegionInfo_ = this.URL_TOUCH_REGION_INFO_FOR_IPHONE;
		this.urlTouchRegions_ = new GUIURLTouchRegion[5];
		for (int i = 0; i < 5; i++)
		{
			Rect[] array = this.urlTouchRegionInfo_;
			int num5 = i;
			array[num5].x = array[num5].x + this.viewRegion_.x;
			Rect[] array2 = this.urlTouchRegionInfo_;
			int num6 = i;
			array2[num6].y = array2[num6].y + this.viewRegion_.y;
			this.urlTouchRegions_[i] = new GUIURLTouchRegion(this.urlTouchRegionInfo_[i]);
			this.mouseManager_.Insert(i, this.urlTouchRegions_[i]);
		}
		base.ResetLocalPosition(0f);
	}

	private GUIPanelEx panel_;

	public Rect imageRegion_;

	public Rect viewRegion_;

	public Rect scrollbarTex_;

	public Material scrollMat_;

	private GUIPanelEx urlSelectedPanel_;

	protected string[] urlForCells_;

	protected Rect[] urlTouchRegionInfo_;

	protected Rect[] URL_TOUCH_REGION_INFO_FOR_IPHONE;

	protected GUIURLTouchRegion[] urlTouchRegions_;

	protected enum CellNames
	{
		MORE_GAMES,
		NEXON,
		NEXON_MOBILE,
		NEXON_AMERICA,
		ONLINE_KARTRIDER,
		SIZE
	}
}
