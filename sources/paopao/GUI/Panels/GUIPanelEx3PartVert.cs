using System;
using UnityEngine;

public class GUIPanelEx3PartVert : GUIPanelEx3Part
{
	public GUIPanelEx3PartVert(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap)
		: this(type, f, tex, layer, fontGap, 1f)
	{
	}

	public GUIPanelEx3PartVert(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap, float mHeight)
	{
		int num = f.Length;
		if (num == 13)
		{
			this.Initialize(type, f, tex, layer, fontGap);
		}
		else if (num == 7)
		{
			float[] array = new float[13];
			array[0] = f[0];
			array[1] = f[1];
			array[2] = f[2];
			array[3] = f[3];
			float num2 = (f[6] - f[5] - mHeight - 4f) * 0.5f;
			array[4] = f[4];
			array[5] = f[5];
			array[6] = array[5] + num2;
			array[7] = f[4];
			array[8] = array[6] + 2f + 1f;
			array[9] = array[8];
			array[10] = f[4];
			array[11] = array[6] + 2f + mHeight + 2f;
			array[12] = array[11] + num2;
			this.Initialize(type, array, tex, layer, fontGap);
		}
	}

	private void Initialize(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap)
	{
		Vector3[] array = new Vector3[] { fontGap, fontGap, fontGap };
		Vector3[] array2 = array;
		int num = 0;
		array2[num].x = array2[num].x - (f[6] - f[5]);
		Vector3[] array3 = array;
		int num2 = 1;
		array3[num2].x = array3[num2].x - (f[9] - f[8]);
		Vector3[] array4 = array;
		int num3 = 2;
		array4[num3].x = array4[num3].x - (f[12] - f[11]);
		float num4 = f[2] - f[0];
		float[] array5 = new float[8];
		array5[0] = f[0];
		array5[1] = f[1];
		array5[2] = f[2];
		array5[3] = f[1] + (f[6] - f[5]);
		array5[4] = f[4];
		array5[5] = f[5];
		array5[6] = array5[4] + num4;
		array5[7] = f[6];
		float[] array6 = new float[8];
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[0] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[0]);
		array5[0] = f[0];
		array5[1] = f[3] - (f[12] - f[11]);
		array5[2] = f[2];
		array5[3] = f[3];
		array5[4] = f[10];
		array5[5] = f[11];
		array5[6] = array5[4] + num4;
		array5[7] = f[12];
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[2] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[2]);
		array5[0] = f[0];
		array5[1] = f[1] + (f[6] - f[5]);
		array5[2] = f[2];
		array5[3] = f[3] - (f[12] - f[11]);
		array5[4] = f[7];
		array5[5] = f[8];
		array5[6] = array5[4] + num4;
		array5[7] = f[9];
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[1] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[1]);
		this.minRange_ = f[6] - f[5] + (f[12] - f[11]);
	}

	public override void ResizeByWindowSpace(float yMin, float yMax)
	{
		if (yMax - yMin < this.minRange_)
		{
			float left_ = this.parts_[0].Panel.left_;
			float height = this.parts_[0].Panel.GetHeight();
			this.parts_[0].SetRectByWindowSpace(left_, yMin);
			this.parts_[2].SetRectByWindowSpace(left_, yMin + height);
			this.parts_[1].Visible = false;
		}
		else
		{
			float left_2 = this.parts_[0].Panel.left_;
			float right_ = this.parts_[0].Panel.right_;
			float height2 = this.parts_[0].Panel.GetHeight();
			this.parts_[0].SetRectByWindowSpace(left_2, yMin);
			this.parts_[2].SetRectByWindowSpace(left_2, yMax - height2);
			this.parts_[1].SetRectByWindowSpace(left_2, yMin + height2, right_, yMax - height2);
			this.parts_[1].Visible = true;
		}
	}
}
