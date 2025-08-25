using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIScrollBarVert : GUIScrollBar
{
	protected override GUIPanelEx3Part CreateGUIPanel(FiaTexture mainTex)
	{
		return new GUIPanelEx3PartVert(0, this.info_.scrollbarInfo_, mainTex, 2, GUIFontCalculator.DEFAULT_GAP, 3f);
	}

	public override void Recalculate()
	{
		float num = this.info_.scrollbarInfo_[3] - this.info_.scrollbarInfo_[1];
		this.barLength_ = num * this.info_.viewRegion_.height / (this.info_.listRegion_.height + this.info_.viewRegion_.height);
		this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[1], this.info_.scrollbarInfo_[1] + this.barLength_);
	}

	public override void SetScrollPosition(float minRange, float maxRange, float range, float moveFactor)
	{
		if (MathHelper.IsBetweenII(range, minRange, maxRange))
		{
			float num = this.info_.scrollbarInfo_[1] + (this.info_.scrollbarInfo_[3] - this.info_.scrollbarInfo_[1] - this.barLength_) * (range - minRange) / (maxRange - minRange);
			this.scrollBar_.ResizeByWindowSpace(num, num + this.barLength_);
		}
		else if (range < minRange)
		{
			float num2 = this.barLength_ * (1f - 2f * Mathf.Abs(range - minRange) / (moveFactor * this.info_.viewRegion_.height));
			this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[1], this.info_.scrollbarInfo_[1] + num2);
		}
		else
		{
			float num3 = this.barLength_ * (1f - 2f * Mathf.Abs(range - maxRange) / (moveFactor * this.info_.viewRegion_.height));
			this.scrollBar_.ResizeByWindowSpace(this.info_.scrollbarInfo_[3] - num3, this.info_.scrollbarInfo_[3]);
		}
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}
}
