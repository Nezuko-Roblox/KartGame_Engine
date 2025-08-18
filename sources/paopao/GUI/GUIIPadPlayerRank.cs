using System;

public class GUIIPadPlayerRank : GUIPanelEx
{
	public GUIIPadPlayerRank()
	{
		if (GUIBase.GetGUIType() == GUIType.IPAD)
		{
			this.gapY_ = 36f;
			this.panelInfo_ = this.PANELINFO_FOR_IPAD;
		}
		else
		{
			this.gapY_ = 26f;
			this.panelInfo_ = this.PANELINFO_FOR_IPHONE;
		}
	}

	public void Update(int panelIndex)
	{
		if (this.panelIndex_ == panelIndex)
		{
			return;
		}
		this.SetRectByWindowSpace(this.panelInfo_[0], this.panelInfo_[1] + this.gapY_ * (float)panelIndex);
		base.SetUV(panelIndex);
		this.panelIndex_ = panelIndex;
	}

	private int panelIndex_;

	private float[] panelInfo_;

	private float[] PANELINFO_FOR_IPAD = new float[] { 182f, 136f };

	private float[] PANELINFO_FOR_IPHONE = new float[] { 166f, 2f };

	private float gapY_;
}
