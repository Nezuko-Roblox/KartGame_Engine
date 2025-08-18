using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIGarage : FiaGUILayer
{
	public override void DoInit()
	{
		this.RegistMonoBehaviour(1049);
		this.mouseManager_ = new MouseManager();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 95f, 623f, 251f, 724f };
		array[0] = 632f * (float)Screen.width / 800f;
		array[1] = 373f * (float)Screen.height / 480f;
		array[2] = 788f * (float)Screen.width / 800f;
		array[3] = 474f * (float)Screen.height / 480f;
		this.character_ = instance.CreateByWindowSpace(num, array, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.character_);
		GUIPanelFactory instance2 = GUIPanelFactory.Instance;
		int num2 = 0;
		float[] array2 = new float[] { 0f, 0f, 0f, 0f, 95f, 520f, 251f, 621f };
		array2[0] = 470f * (float)Screen.width / 800f;
		array2[1] = 373f * (float)Screen.height / 480f;
		array2[2] = 626f * (float)Screen.width / 800f;
		array2[3] = 474f * (float)Screen.height / 480f;
		this.kart_ = instance2.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.kart_);
		this.mouseManager_.Insert(0, this.character_);
		this.mouseManager_.Insert(1, this.kart_);
	}

	protected override void FirstUpdate()
	{
		this.selectedId_ = 1;
		this.character_.SetUV((this.selectedId_ != 0) ? 0 : 1);
		this.kart_.SetUV((this.selectedId_ != 1) ? 0 : 1);
		base.SendMessage(1048, ((MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_SHOPLIST)).Initialize(this.selectedId_));
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey != -1 && selectedKey != this.selectedId_)
		{
			this.selectedId_ = selectedKey;
			KartOptions.Instance.SaveRegistry();
			base.SendMessage(1048, ((MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_SHOPLIST)).Initialize(this.selectedId_));
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
		this.character_.SetUV((this.selectedId_ != 0) ? 0 : 1);
		this.kart_.SetUV((this.selectedId_ != 1) ? 0 : 1);
	}

	private GUIPanelEx character_;

	private GUIPanelEx kart_;

	private MouseManager mouseManager_;

	private int selectedId_;
}
