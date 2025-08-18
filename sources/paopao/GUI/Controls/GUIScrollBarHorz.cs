using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIScrollBarHorz : GUIScrollBar
{
	protected override GUIPanelEx3Part CreateGUIPanel(FiaTexture mainTex)
	{
		return new GUIPanelEx3PartHorz(0, this.info_.scrollbarInfo_, mainTex, 2, GUIFontCalculator.DEFAULT_GAP, 3f);
	}

	public override void Recalculate()
	{
		float num = this.info_.scrollbarInfo_[2] - this.info_.scrollbarInfo_[0];
		this.barLength_ = num * this.info_.viewRegion_.width / (this.info_.listRegion_.width + this.info_.viewRegion_.width);
		this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[0], this.info_.scrollbarInfo_[0] + this.barLength_);
	}

	public override void SetScrollPosition(float minRange, float maxRange, float range, float moveFactor)
	{
		if (MathHelper.IsBetweenII(range, minRange, maxRange))
		{
			float num = this.info_.scrollbarInfo_[0] + (this.info_.scrollbarInfo_[2] - this.info_.scrollbarInfo_[0] - this.barLength_) * (range - maxRange) / (minRange - maxRange);
			this.scrollBar_.ResizeByWindowSpace(num, num + this.barLength_);
		}
		else if (range < minRange)
		{
			float num2 = this.barLength_ * (1f - 2f * Mathf.Abs(range - minRange) / (moveFactor * this.info_.viewRegion_.width));
			this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[2] - num2, this.info_.scrollbarInfo_[2]);
		}
		else
		{
			float num3 = this.barLength_ * (1f - 2f * Mathf.Abs(range - maxRange) / (moveFactor * this.info_.viewRegion_.width));
			this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[0], this.info_.scrollbarInfo_[0] + num3);
		}
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}
}
