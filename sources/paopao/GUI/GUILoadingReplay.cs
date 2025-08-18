using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUILoadingReplay : FiaGUILayer
{
	public override void DoInit()
	{
		StageController.Instance.BeginStage();
		this.RegistMonoBehaviour(1024);
		this.mainTex = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		this.fontTex = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		this.replyTex = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		this.mouseManager_ = new MouseManager();
		this.reply_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, -295f, -160f, 507f, 322f }, this.replyTex, 1, GUIFontCalculator.DEFAULT_GAP);
		this.reply_.SubMeshIndex = 2;
		this.reply_.RegistPanelManager(this.panelManager_);
		this.message_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 300f, 405f, 2f, 1f, 195f, 29f }, this.fontTex, 2, GUIFontCalculator.DEFAULT_GAP);
		this.message_.SubMeshIndex = 1;
		this.message_.RegistPanelManager(this.panelManager_);
		this.message_.Visible = false;
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 0f, 0f, 0f, 0f, 800f, 480f }, this.mainTex, 3, GUIFontCalculator.DEFAULT_GAP);
		this.back_.SubMeshIndex = 0;
		this.back_.RegistPanelManager(this.panelManager_);
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
	}

	private void UpdateFlickering()
	{
		if (this.message_ != null)
		{
			this.message_.Visible = !this.message_.Visible;
		}
	}

	private void replyHidden()
	{
		this.mouseManager_.Insert(0, this.back_);
		base.InvokeRepeating("UpdateFlickering", 0f, 0.5f);
		this.reply_.Alpha = 0f;
		this.reply_.Visible = !this.reply_.Visible;
	}

	protected override void AfterPanelUpdate()
	{
		if (this.isReadyToMoveNext_ && this.mouseManager_.GetSelectedKey() != -1)
		{
			StageController.Instance.ChangeStage(StageType.MAIN);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SIMPLE_MESSAGE)
		{
			MonoBehaviourMessage2Param<int, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, int>)msg;
			if (monoBehaviourMessage2Param.lparam_ == 0)
			{
				this.isReadyToMoveNext_ = true;
				base.Invoke("replyHidden", 3f);
			}
		}
	}

	private MouseManager mouseManager_;

	private GUIPanelEx back_;

	private GUIPanelEx message_;

	private GUIPanelEx reply_;

	private FiaTexture mainTex;

	private FiaTexture fontTex;

	private FiaTexture replyTex;

	private bool isReadyToMoveNext_;
}
