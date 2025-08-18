using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIBackground : FiaGUILayer
{
	public GUIBackground()
	{
		float[] array = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 802f, 482f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.position = array;
	}

	public override void DoInit()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		string[] array = new string[this.position.Length];
		for (int i = 0; i < this.position.Length; i++)
		{
			array[i] = this.position[i].ToString();
		}
		Debug.Log("texture name is:" + this.meshRenderer_.material.mainTexture.name + "float[] =" + string.Join(",", array));
		float[] array2 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 802f, 482f };
		array2[2] = (float)Screen.width;
		array2[3] = (float)Screen.height;
		this.position = array2;
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.position, fiaTexture, 7, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
	}

	private GUIPanelEx back_;

	public float[] position;
}
