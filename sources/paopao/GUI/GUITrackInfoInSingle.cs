using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUITrackInfoInSingle : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = new GUIPanelEx3PartHorz(0, new float[] { 8f, 54f, 266f, 284f, 425f, 792f, 510f }, fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.back_.RegistPanelManager(this.panelManager_);
		float[] array = new float[]
		{
			18f, 88f, 256f, 240f, 2f, 22f, 240f, 220f, 2f, 516f,
			240f, 552f
		};
		ScrollBarInfo scrollBarInfo = new ScrollBarInfo();
		scrollBarInfo.isVert_ = true;
		scrollBarInfo.viewRegion_ = GUIBase.ConvertWSToUS(array, 0);
		scrollBarInfo.listRegion_ = GUIBase.ConvertWSToUS(array, 4);
		scrollBarInfo.material_ = this.meshRenderer_.material;
		scrollBarInfo.texture_ = fiaTexture;
		scrollBarInfo.scrollbarInfo_ = new float[] { 266f, 88f, 270f, 240f, 208f, 1011f, 1022f };
		GUIScrollBar guiscrollBar = null;
		if (this.scrollBarPrefab_ != null && scrollBarInfo != null)
		{
			guiscrollBar = (GUIScrollBar)global::UnityEngine.Object.Instantiate(this.scrollBarPrefab_);
			guiscrollBar.transform.parent = base.transform.parent;
			guiscrollBar.info_ = scrollBarInfo;
			guiscrollBar.Initialize();
		}
		this.contents_ = new GUIUVScrollImage(0, array, fiaTexture, 2, guiscrollBar);
		this.contents_.RegistPanelManager(this.panelManager_);
		this.contents_.SetUV(0);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.contents_.Update();
	}

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx title_;

	private GUIButton yep_;

	private GUIUVScrollImage contents_;

	private MouseManager mouseManager_;

	public GUIScrollBar scrollBarPrefab_;
}
