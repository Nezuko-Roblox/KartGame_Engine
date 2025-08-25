using System;
using UnityEngine;

public class GUITrophyPopup : FiaGUILayer
{
	protected override void CameraSetting()
	{
		if (KartManager.Instance.parameter_.Stage == StageType.GAME || KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI)
		{
			this.cam_ = CameraManager.Instance.guiCam_;
			for (int i = 0; i < 32; i++)
			{
				if (((this.cam_.cullingMask >> i) & 1) != 0)
				{
					base.gameObject.layer = i;
					break;
				}
			}
		}
		else
		{
			base.CameraSetting();
		}
	}

	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(266);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.back_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 123f, 172f, 2f, 298f, 556f, 512f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.back_.RegistPanelManager(this.panelManager_);
		this.stars_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 169f, 46f, 2f, 2f, 466f, 148f }, fiaTexture, 5, new Vector3(2f, 2f, 12f));
		this.stars_.RegistPanelManager(this.panelManager_);
		this.stars_.Visible = true;
		this.stars_.SetUV(2);
		this.cups_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 384f, 258f, 558f, 298f, 678f, 338f }, fiaTexture, 1, GUIFontCalculator.Y2_GAP);
		this.cups_.RegistPanelManager(this.panelManager_);
		this.msg_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 225f, 312f, 2f, 514f, 440f, 570f }, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		this.msg_.RegistPanelManager(this.panelManager_);
		base.gameObject.SetActiveRecursively(false);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_INFO)
		{
			MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				base.gameObject.SetActiveRecursively(true);
				this.cups_.UV = monoBehaviourMessage2Param.rparam_;
				this.msg_.UV = monoBehaviourMessage2Param.rparam_;
				this.StarAnimate();
				base.Invoke("DisablePopup", 4f);
			}
		}
	}

	private void StarAnimate()
	{
		this.stars_.Visible = false;
		base.Invoke("StarStep1", 0.05f);
	}

	private void StarStep1()
	{
		this.stars_.SetUV(0);
		this.stars_.Visible = true;
		base.Invoke("StarStep2", 0.05f);
	}

	private void StarStep2()
	{
		this.stars_.SetUV(1);
		base.Invoke("StarStep3", 0.05f);
	}

	private void StarStep3()
	{
		this.stars_.SetUV(2);
		base.Invoke("StarStep4", 0.05f);
	}

	private void StarStep4()
	{
		this.stars_.SetUV(3);
		base.Invoke("StarStep5", 0.1f);
	}

	private void StarStep5()
	{
		this.stars_.SetUV(2);
	}

	private void DisablePopup()
	{
		this.stars_.Visible = false;
		base.gameObject.SetActiveRecursively(false);
		if (KartManager.Instance.parameter_.Stage == StageType.GAME)
		{
			MonoBehaviourExCenter.Instance.SendMessage(0, 7, ((MonoBehaviourMessage2Param<bool, bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESULT)).Initialize(true, KartManager.Instance.result_.IsBestRecord));
		}
	}

	public static bool OpenStaticPopup()
	{
		KartAssetDefinitionManager.Instance.Refresh();
		CharacterAssetDefinitionManager.Instance.Refresh();
		TrackAssetDefinitionManager.Instance.Refresh();
		MonoBehaviourMessage2Param<AssetType, int> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<AssetType, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_INFO);
		MonoBehaviourExCenter.Instance.SendMessage(0, 265, monoBehaviourMessage2Param.Initialize(AssetType.SIZE, -1));
		return true;
	}

	private const float POPUP_VISIBLE_TIME = 4f;

	private GUIPanelEx back_;

	private GUIPanelEx cups_;

	private GUIPanelEx msg_;

	private GUIPanelEx stars_;
}
