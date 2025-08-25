using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIWifi : FiaGUILayer
{
	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(1065);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.refresh_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 470f, 78f, 618f, 180f, 790f, 228f }, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		this.refresh_.RegistPanelManager(this.panelManager_);
		this.mouseManager_ = new MouseManager();
		this.mouseManager_.Insert(0, this.refresh_);
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		int pushedKey = this.mouseManager_.GetPushedKey();
		this.refresh_.SetUV((pushedKey != 0) ? 0 : 1);
	}

	protected override void AfterPanelUpdate()
	{
		if (this.mouseManager_.GetSelectedKey() == 0)
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
			base.SendMessage(514, monoBehaviourMessage2Param.Initialize(4, 0));
		}
	}

	private const int MOUSE_NOTIFIER_ID_REFRESH = 0;

	private MouseManager mouseManager_;

	private GUIPanelEx refresh_;
}
