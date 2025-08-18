using System;
using UnityEngine;

public class GUISpeedArrow : GUIPanelEx
{
	public override void Initialize(ArrayEx<float> f, Rect uvRect, Rect orgRect, int layer, Vector3 fontGap)
	{
		base.Initialize(f, uvRect, orgRect, layer, fontGap);
		Vector2 leftTopByWindowPos = this.GetLeftTopByWindowPos();
		this.axisPosition_.x = this.axisPosition_.x + leftTopByWindowPos.x;
		this.axisPosition_.y = this.axisPosition_.y + leftTopByWindowPos.y;
		this.axisPosition_ = GUIBase.ConvertWSToUS(this.axisPosition_);
		this.panel_.GetVertices(out this.speedArrowVertices_);
		this.panel_.GetVertices(out this.transformedArrowVertices_);
	}

	public void Update(float speed)
	{
		float num = this.speedValue_.y - this.speedValue_.x;
		float num2 = this.degree_.y - this.degree_.x;
		speed = Mathf.Clamp(speed, this.speedValue_.x, this.speedValue_.y);
		float num3 = this.degree_.x + (speed - this.speedValue_.x) / num * num2;
		Quaternion quaternion = Quaternion.AngleAxis(num3, -Vector3.forward);
		for (int i = 0; i < 4; i++)
		{
			this.transformedArrowVertices_[i] = this.axisPosition_ + quaternion * (this.speedArrowVertices_[i] - this.axisPosition_);
		}
		this.dirtyFlag_ |= 8;
	}

	public override void GetVerticesUVs(ref Vector3[] vertices, ref Vector2[] uvs, int idx)
	{
		for (int i = 0; i < 4; i++)
		{
			vertices[idx + i] = this.transformedArrowVertices_[i];
		}
		this.calc_.GetUV(this.uv_, ref uvs, idx);
	}

	private Vector2 degree_ = new Vector2(-90f, 180f);

	private Vector2 speedValue_ = new Vector2(0f, 360f);

	private Vector3 axisPosition_ = new Vector3(70.5f, 8f, 0f);

	private Vector3[] speedArrowVertices_;

	private Vector3[] transformedArrowVertices_;
}
