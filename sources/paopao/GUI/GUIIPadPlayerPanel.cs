using System;

public class GUIIPadPlayerPanel : GUIPanelEx
{
	public GUIIPadPlayerPanel()
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
		base.SetRectByWindowSpace(this.panelInfo_[num][0], this.panelInfo_[num][1] + this.gapY_ * (float)panelIndex, this.panelInfo_[num][2], this.panelInfo_[num][3] + this.gapY_ * (float)panelIndex);
		this.isShort_ = isShort;
		this.panelIndex_ = panelIndex;
	}

	private bool isShort_ = true;

	private int panelIndex_;

	private float[][] panelInfo_;

	private float[][] PANELINFO_FOR_IPAD = new float[][]
	{
		new float[] { -2f, 161f, 162f, 187f },
		new float[] { -2f, 161f, 258f, 187f }
	};

	private float[][] PANELINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 0f, 10f, 146f, 30f },
		new float[] { 0f, 10f, 212f, 30f }
	};

	private float gapY_;
}
