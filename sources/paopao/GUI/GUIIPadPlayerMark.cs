using System;

public class GUIIPadPlayerMark : GUIPanelEx
{
	public GUIIPadPlayerMark()
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

	public void Update(int panelIndex, bool isShort)
	{
		if (this.isShort_ == isShort && this.panelIndex_ == panelIndex)
		{
			return;
		}
		int num = ((!isShort) ? 1 : 0);
		this.SetRectByWindowSpace(this.panelInfo_[num][0], this.panelInfo_[num][1] + this.gapY_ * (float)panelIndex);
		this.isShort_ = isShort;
		this.panelIndex_ = panelIndex;
	}

	private bool isShort_ = true;

	private int panelIndex_;

	private float[][] panelInfo_;

	private float[][] PANELINFO_FOR_IPAD = new float[][]
	{
		new float[] { 161f, 159f },
		new float[] { 257f, 159f }
	};

	private float[][] PANELINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 146f, 7f },
		new float[] { 212f, 7f }
	};

	private float gapY_;
}
