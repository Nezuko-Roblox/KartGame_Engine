using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIRoomSearchingPopup : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.backTop_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 78f, 458f, 402f, 95f, 120f, 186f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.panelManager_.RegistGUIInterface(this.backTop_);
		this.backBottom_ = new GUIPanelEx3PartHorz(0, new float[] { 12f, 402f, 458f, 474f, 95f, 446f, 186f }, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.panelManager_.RegistGUIInterface(this.backBottom_);
		this.message1_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 100f, 222f, 473f, 420f, 792f, 462f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.message1_);
		this.message2_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 37f, 409f, 473f, 466f, 891f, 524f }, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.message2_);
		this.progressPoints_ = new GUIPanelEx[3];
		for (int i = 0; i < this.progressPoints_.Length; i++)
		{
			GUIPanelEx[] array = this.progressPoints_;
			int num = i;
			GUIPanelFactory instance = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 0f, 244f, 794f, 420f, 806f, 432f };
			array2[0] = (float)(372 + 8 * i);
			array[num] = instance.CreateByWindowSpace(num2, array2, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.progressPoints_[i]);
			this.progressPoints_[i].Visible = false;
		}
		base.InvokeRepeating("ProgressAnimation", 0.1f, 0.1f);
		base.gameObject.SetActiveRecursively(false);
	}

	public void ProgressAnimation()
	{
		this.point_counter = (this.point_counter + 1) % 12;
		for (int i = 0; i < this.progressPoints_.Length; i++)
		{
			this.progressPoints_[i].Visible = i < this.point_counter / 3;
		}
	}

	private GUIPanelEx3PartHorz backTop_;

	private GUIPanelEx3PartHorz backBottom_;

	private GUIPanelEx message1_;

	private GUIPanelEx message2_;

	private GUIPanelEx[] progressPoints_;

	private GUIPanelEx arrow_;

	private int point_counter;
}
