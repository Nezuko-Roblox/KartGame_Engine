using System;
using UnityEngine;

public class GUIPanelEx3PartHorz : GUIPanelEx3Part
{
	public GUIPanelEx3PartHorz(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap)
	{
		int num = f.Length;
		if (num == 13)
		{
			this.Initialize(type, f, tex, layer, fontGap);
		}
		else if (num == 7)
		{
			this.Initialize_f7(type, f, tex, layer, fontGap, 1f);
		}
	}

	public GUIPanelEx3PartHorz(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap, float mWidth)
	{
		if (f.Length == 7)
		{
			this.Initialize_f7(type, f, tex, layer, fontGap, mWidth);
		}
	}

	private void Initialize_f7(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap, float mWidth)
	{
		float[] array = new float[13];
		array[0] = f[0];
		array[1] = f[1];
		array[2] = f[2];
		array[3] = f[3];
		float num = (f[6] - f[4] - mWidth - 4f) * 0.5f;
		array[4] = f[4];
		array[5] = f[5];
		array[6] = array[4] + num;
		array[7] = array[6] + 2f;
		array[8] = f[5];
		array[9] = array[7] + mWidth - 1f;
		array[10] = array[9] + 1f + 2f;
		array[11] = f[5];
		array[12] = array[10] + num;
		this.Initialize(type, array, tex, layer, fontGap);
	}

	private void Initialize(int type, float[] f, FiaTexture tex, int layer, Vector3 fontGap)
	{
		Vector3[] array = new Vector3[] { fontGap, fontGap, fontGap };
		Vector3[] array2 = array;
		int num = 0;
		array2[num].x = array2[num].x - (f[6] - f[4]);
		Vector3[] array3 = array;
		int num2 = 1;
		array3[num2].x = array3[num2].x - (f[9] - f[7]);
		Vector3[] array4 = array;
		int num3 = 2;
		array4[num3].x = array4[num3].x - (f[12] - f[10]);
		float num4 = f[3] - f[1];
		float[] array5 = new float[8];
		array5[0] = f[0];
		array5[1] = f[1];
		array5[2] = f[0] + (f[6] - f[4]);
		array5[3] = f[3];
		array5[4] = f[4];
		array5[5] = f[5];
		array5[6] = f[6];
		array5[7] = array5[5] + num4;
		float[] array6 = new float[8];
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[0] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[0]);
		array5[0] = f[2] - (f[12] - f[10]);
		array5[1] = f[1];
		array5[2] = f[2];
		array5[3] = f[3];
		array5[4] = f[10];
		array5[5] = f[11];
		array5[6] = f[12];
		array5[7] = array5[5] + num4;
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[2] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[2]);
		array5[0] = f[0] + (f[6] - f[4]);
		array5[1] = f[1];
		array5[2] = f[2] - (f[12] - f[10]);
		array5[3] = f[3];
		array5[4] = f[7];
		array5[5] = f[8];
		array5[6] = f[9];
		array5[7] = array5[5] + num4;
		array5.CopyTo(array6, 0);
		array6[0] = array6[0] * (float)Screen.width / 800f;
		array6[1] = array6[1] * (float)Screen.height / 480f;
		array6[2] = array6[2] * (float)Screen.width / 800f;
		array6[3] = array6[3] * (float)Screen.height / 480f;
		this.parts_[1] = GUIPanelFactory.Instance.CreateByWindowSpace(type, array6, tex, layer, array[1]);
		this.rect_ = new Rect(f[0] * (float)Screen.width / 800f, (float)Screen.height - f[3] * (float)Screen.height / 480f, (f[2] - f[0]) * (float)Screen.width / 800f, (f[3] - f[1]) * (float)Screen.height / 480f);
		this.minRange_ = f[6] - f[4] + (f[12] - f[10]);
	}

	public override void ResizeByWindowSpace(float xMin, float xMax)
	{
		if (xMax - xMin < this.minRange_)
		{
			float num = (float)Screen.height - this.parts_[0].Panel.top_;
			float width = this.parts_[0].Panel.GetWidth();
			this.parts_[0].SetRectByWindowSpace(xMin, num);
			this.parts_[2].SetRectByWindowSpace(xMin + width, num);
			this.parts_[1].Visible = false;
			this.rect_.xMin = xMin;
			this.rect_.xMax = xMin + width;
		}
		else
		{
			float num2 = (float)Screen.height - this.parts_[0].Panel.top_;
			float num3 = (float)Screen.height - this.parts_[0].Panel.bottom_;
			float width2 = this.parts_[0].Panel.GetWidth();
			this.parts_[0].SetRectByWindowSpace(xMin, num2);
			this.parts_[2].SetRectByWindowSpace(xMax - width2, num2);
			this.parts_[1].SetRectByWindowSpace(xMin + width2, num2, xMax - width2, num3);
			this.parts_[1].Visible = true;
			this.rect_.xMin = xMin;
			this.rect_.xMax = xMax;
		}
	}
}
