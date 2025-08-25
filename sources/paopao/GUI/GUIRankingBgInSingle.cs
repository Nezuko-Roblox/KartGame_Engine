using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIRankingBgInSingle : FiaGUILayer
{
	public override void DoInit()
	{
		this.RegistMonoBehaviour(1028);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 194f, 458f, 474f, 2f, 246f, 93f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.back_.RegistPanelManager(this.panelManager_);
	}

	private GUIPanelEx3PartHorz back_;
}
