using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIInfoControllers : FiaGUILayer
{
	public override void DoInit()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.mouseManager_ = new MouseManager();
		int num = KartOptions.Instance.Controller;
		if (num < 0)
		{
			num = 0;
		}
		this.controls_ = new GUIPanelEx[3];
		this.labels_ = new GUIPanelEx[3];
		for (int i = 0; i < 3; i++)
		{
			GUIPanelEx[] array = this.controls_;
			int num2 = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num3 = 0;
			float[] array2 = new float[] { 0f, 90f, 0f, 0f, 582f, 2f, 772f, 52f };
			array2[0] = (float)(188 + i * 194);
			array2[2] = array2[0] + array2[6] - array2[4];
			array2[3] = array2[1] + array2[7] - array2[5];
			array2[0] = array2[0] * (float)Screen.width / 800f;
			array2[1] = array2[1] * (float)Screen.height / 480f;
			array2[2] = array2[2] * (float)Screen.width / 800f;
			array2[3] = array2[3] * (float)Screen.height / 480f;
			array[num2] = instance.CreateByWindowSpace(num3, array2, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
			this.controls_[i].RegistPanelManager(this.panelManager_);
			this.controls_[i].SetUV((num != i) ? 1 : 0);
			this.mouseManager_.Insert(i, this.controls_[i]);
			GUIPanelEx[] array3 = this.labels_;
			int num4 = i;
			GUIPanelFactory instance2 = GUIPanelFactory.Instance;
			int num5 = 0;
			float[] array4 = new float[] { 0f, 103f, 0f, 0f, 774f, 2f, 852f, 24f };
			array4[0] = (float)(243 + i * 194);
			array4[2] = array4[0] + array4[6] - array4[4];
			array4[3] = array4[1] + array4[7] - array4[5];
			array4[0] = array4[0] * (float)Screen.width / 800f;
			array4[1] = array4[1] * (float)Screen.height / 480f;
			array4[2] = array4[2] * (float)Screen.width / 800f;
			array4[3] = array4[3] * (float)Screen.height / 480f;
			array3[num4] = instance2.CreateByWindowSpace(num5, array4, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
			this.labels_[i].RegistPanelManager(this.panelManager_);
			this.labels_[i].SetUV(i);
			this.labels_[i].SetRectByWindowSpace((float)((num != i) ? (243 + i * 194) : (261 + i * 194)), 103f);
		}
		float[] array5 = new float[] { 261f, 179f, 0f, 0f, 2f, 2f, 434f, 250f };
		array5[2] = array5[0] + array5[6] - array5[4];
		array5[3] = array5[1] + array5[7] - array5[5];
		array5[0] = array5[0] * (float)Screen.width / 800f;
		array5[1] = array5[1] * (float)Screen.height / 480f;
		array5[2] = array5[2] * (float)Screen.width / 800f;
		array5[3] = array5[3] * (float)Screen.height / 480f;
		this.type_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, array5, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		this.type_.RegistPanelManager(this.panelManager_);
		this.type_.SetUV(num);
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1)
		{
			for (int i = 0; i < 3; i++)
			{
				this.controls_[i].SetUV((selectedKey != i) ? 1 : 0);
				this.labels_[i].SetRectByWindowSpace((float)((selectedKey != i) ? (243 + i * 194) : (261 + i * 194)) * (float)Screen.width / 800f, 103f * (float)Screen.height / 480f);
			}
			this.type_.SetUV(selectedKey);
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			iOSController.Instance.Type = (iOSControllerType)selectedKey;
			KartOptions.Instance.Controller = selectedKey;
			KartOptions.Instance.SaveRegistry();
		}
	}

	private MouseManager mouseManager_;

	private GUIPanelEx[] controls_;

	private GUIPanelEx[] labels_;

	private GUIPanelEx type_;
}
