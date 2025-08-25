using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIBackgroundHost : FiaGUILayer
{
	public override void DoInit()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 482f, 322f };
		array[2] = 480f * (float)Screen.width / 800f;
		array[3] = 320f * (float)Screen.height / 480f;
		this.back_ = instance.CreateByWindowSpace(num, array, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		GUIPanelFactory instance2 = GUIPanelFactory.Instance;
		int num2 = 0;
		float[] array2 = new float[] { 0f, 0f, 0f, 0f, 484f, 2f, 484f, 74f };
		array2[0] = 10f * (float)Screen.width / 800f;
		array2[1] = 68f * (float)Screen.height / 480f;
		array2[2] = 270f * (float)Screen.width / 800f;
		array2[3] = 140f * (float)Screen.height / 480f;
		this.trackback_ = instance2.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.trackback_);
	}

	private GUIPanelEx back_;

	private GUIPanelEx trackback_;
}
