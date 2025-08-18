using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIStoreLeftBackground : FiaGUILayer
{
	public override void DoInit()
	{
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 12f, 78f, 248f, 474f, 14f, 484f, 228f, 487f }, fiaTexture, 7, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
	}

	private GUIPanelEx back_;
}
