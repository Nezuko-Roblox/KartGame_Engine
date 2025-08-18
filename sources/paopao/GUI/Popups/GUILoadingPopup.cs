using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUILoadingPopup : FiaGUILayer
{
	public GUILoadingPopup()
	{
		float[][] array = new float[5][];
		int num = 0;
		float[] array2 = new float[8];
		array2[2] = (float)Screen.width;
		array2[3] = (float)Screen.height;
		array[num] = array2;
		array[1] = new float[] { 57f, 152f, 2f, 2f, 688f, 176f };
		array[2] = new float[] { 90f, 221f, 2f, 180f, 500f, 216f };
		array[3] = new float[] { 90f, 185f, 2f, 294f, 500f, 402f };
		array[4] = new float[] { 593f, 193f, 2f, 403f, 120f, 495f };
		this.PANEL_INFO_FOR_IPHONE = array;
		float[][] array3 = new float[5][];
		int num2 = 0;
		float[] array4 = new float[] { 0f, 0f, 0f, 0f, 152f, 476f, 153f, 477f };
		array4[2] = (float)Screen.width;
		array4[3] = (float)Screen.height;
		array3[num2] = array4;
		array3[1] = new float[] { 50f, 121f, 2f, 2f, 382f, 84f };
		array3[2] = new float[] { 58f, 151f, 2f, 86f, 316f, 104f };
		array3[3] = new float[] { 58f, 133f, 2f, 146f, 316f, 200f };
		array3[4] = new float[] { 349f, 141f, 2f, 202f, 52f, 240f };
		this.PANEL_INFO_FOR_IPAD = array3;
		base..ctor();
	}

	public override void DoInit()
	{
		base.DoInit();
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.PANEL_INFO_FOR_IPAD : this.PANEL_INFO_FOR_IPHONE);
		this.RegistMonoBehaviour(22);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.popupMask_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[0], fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.popupMask_);
		this.popupBack_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[1], fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.popupBack_);
		this.popupMessage_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[2], fiaTexture, 1, GUIFontCalculator.Y2_GAP);
		this.panelManager_.RegistGUIInterface(this.popupMessage_);
		this.popupMessage_.Visible = false;
		this.popupLongMessage_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[3], fiaTexture, 1, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.popupLongMessage_);
		this.popupLongMessage_.Visible = false;
		this.popupProgress_ = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[4], fiaTexture, 2, GUIFontCalculator.X2_GAP);
		this.panelManager_.RegistGUIInterface(this.popupProgress_);
		base.gameObject.SetActiveRecursively(false);
	}

	private void ProgressAnimation()
	{
		int uv = this.popupProgress_.UV;
		this.popupProgress_.UV = (uv + 1) % 4;
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		this.popupMessage_.Visible = false;
		this.popupLongMessage_.Visible = false;
		if (msg.type_ == MonoBehaviourMessageType.NETWORK_JOIN_MESSAGE)
		{
			base.gameObject.SetActiveRecursively(true);
			this.popupMessage_.SetUV(3);
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				this.popupLongMessage_.Visible = true;
				if (monoBehaviourMessage1Param.param_ == 1)
				{
					base.gameObject.SetActiveRecursively(true);
					base.InvokeRepeating("ProgressAnimation", 0.1f, 0.1f);
				}
				else if (monoBehaviourMessage1Param.param_ == 0 || monoBehaviourMessage1Param.param_ == 3)
				{
					base.CancelInvoke("ProgressAnimation");
					base.gameObject.SetActiveRecursively(false);
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.FACEBOOK_MESSAGE)
		{
			base.gameObject.SetActiveRecursively(true);
			this.popupMessage_.SetUV(0);
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param2 != null)
			{
				this.popupMessage_.Visible = true;
				if (monoBehaviourMessage1Param2.param_ == 0)
				{
					base.gameObject.SetActiveRecursively(true);
					base.InvokeRepeating("ProgressAnimation", 0.1f, 0.1f);
				}
				else if (monoBehaviourMessage1Param2.param_ == 1 || monoBehaviourMessage1Param2.param_ == 1)
				{
					base.CancelInvoke("ProgressAnimation");
					base.gameObject.SetActiveRecursively(false);
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.RANKING_LOADING_MESSAGE)
		{
			base.gameObject.SetActiveRecursively(true);
			this.popupMessage_.SetUV(1);
			MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param3 = (MonoBehaviourMessage1Param<int>)msg;
			if (monoBehaviourMessage1Param3 != null)
			{
				this.popupMessage_.Visible = true;
				if (monoBehaviourMessage1Param3.param_ == 0)
				{
					this.inputAuthorityBackUp_ = StageController.Instance.InputAutority;
					StageController.Instance.InputAutority = this.inputAuthority_;
					base.gameObject.SetActiveRecursively(true);
					base.InvokeRepeating("ProgressAnimation", 0.1f, 0.1f);
				}
				else if (monoBehaviourMessage1Param3.param_ == 1)
				{
					base.Invoke("Hide", 0.5f);
				}
			}
		}
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	protected void Hide()
	{
		base.CancelInvoke("ProgressAnimation");
		StageController.Instance.InputAutority = this.inputAuthorityBackUp_;
		base.gameObject.SetActiveRecursively(false);
	}

	public void BackButtonAction(string msg)
	{
	}

	private GUIPanelEx popupMask_;

	private GUIPanelEx popupBack_;

	private GUIPanelEx popupMessage_;

	private GUIPanelEx popupLongMessage_;

	private GUIPanelEx popupProgress_;

	protected byte inputAuthority_ = 4;

	protected byte inputAuthorityBackUp_;

	private float[][] panelInfo_;

	private float[][] PANEL_INFO_FOR_IPHONE;

	private float[][] PANEL_INFO_FOR_IPAD;

	private enum PanelType
	{
		POPUP_MASK,
		POPUP_BACK,
		POPUP_MESSAGE,
		POPUP_LONG_MESSAGE,
		POPUP_PROGRESS
	}
}
