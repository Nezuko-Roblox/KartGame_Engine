using System;
using UnityEngine;

public class GUIControls : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(20);
		this.panelManager_ = new GUIPanelManager();
		this.buttonInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.BUTTONINFO_FOR_IPAD : this.BUTTONINFO_FOR_IPHONE);
		this.touchRegionInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.TOUCHREGIONINFO_FOR_IPAD : this.TOUCHREGIONINFO_FOR_IPHONE);
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.PANELINFO_FOR_IPAD : this.PANELINFO_FOR_IPHONE);
	}

	private void Start()
	{
		int guitype = (int)GUIBase.GetGUIType();
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.panelManager_.SetCamera(CameraManager.Instance.guiCam_);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 870f, 2f, 873f, 5f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.bg_ = instance.CreateByWindowSpace(num, array, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.bg_);
		this.back_ = new GUIPanelEx3PartHorz(0, this.BACKINFO[guitype], fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.buttons_ = new GUIButton[1];
		for (int i = 0; i < 1; i++)
		{
			this.buttons_[i] = new GUIButton(0, this.buttonInfo_[i], fiaTexture, 3, new Vector3(149f, 0f, 1f));
			this.mouseManager_.Insert(i, this.buttons_[i]);
			this.panelManager_.RegistGUIInterface(this.buttons_[i]);
		}
		Vector3[] array2 = new Vector3[]
		{
			new Vector3(2f, 2f, 12f),
			GUIFontCalculator.DEFAULT_GAP,
			GUIFontCalculator.X2_GAP,
			GUIFontCalculator.X2_GAP
		};
		this.panels_ = new GUIPanelEx[4];
		for (int j = 0; j < 4; j++)
		{
			float[] array3 = new float[8];
			array3[0] = this.panelInfo_[j][0];
			array3[1] = this.panelInfo_[j][1];
			array3[2] = array3[0] + this.panelInfo_[j][4] - this.panelInfo_[j][2];
			array3[3] = array3[1] + this.panelInfo_[j][5] - this.panelInfo_[j][3];
			array3[4] = this.panelInfo_[j][2];
			array3[5] = this.panelInfo_[j][3];
			array3[6] = this.panelInfo_[j][4];
			array3[7] = this.panelInfo_[j][5];
			array3[0] = array3[0] * (float)Screen.width / 800f;
			array3[1] = array3[1] * (float)Screen.height / 480f;
			array3[2] = array3[2] * (float)Screen.width / 800f;
			array3[3] = array3[3] * (float)Screen.height / 480f;
			this.panels_[j] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array3, fiaTexture, 2, array2[j]);
			this.panelManager_.RegistGUIInterface(this.panels_[j]);
		}
		this.controlIndex_ = (int)iOSController.Instance.Type;
		this.mouseManager_.Insert(2, this.panels_[2]);
		this.mouseManager_.Insert(3, this.panels_[3]);
		this.panels_[0].UV = this.controlIndex_;
		this.LeftRightButtonSetting();
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		base.gameObject.SetActiveRecursively(false);
	}

	private void Update()
	{
		bool flag = false;
		this.mouseManager_.Update();
		int num = this.mouseManager_.GetSelectedKey();
		if (num == 2 || num == 3)
		{
			int num2 = this.controlIndex_;
			int num3 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 3 : 0);
			int num4 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 5 : 2);
			this.controlIndex_ = Mathf.Clamp(this.controlIndex_ + ((num != 2) ? 1 : (-1)), num3, num4);
			int num5 = this.controlIndex_ - num3;
			if (this.controlIndex_ != num2)
			{
				this.panels_[0].UV = num5;
				flag = true;
				this.LeftRightButtonSetting();
			}
			if (StageController.IsInstantiated())
			{
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
		}
		int pushedKey = this.mouseManager_.GetPushedKey();
		for (int i = 0; i < this.buttons_.Length; i++)
		{
			flag |= this.buttons_[i].SetPushed(pushedKey == i);
		}
		flag |= this.panels_[2].SetUV((pushedKey != 2) ? 0 : 1);
		flag |= this.panels_[3].SetUV((pushedKey != 3) ? 0 : 1);
		if (flag && this.panelManager_.Update())
		{
			this.mesh_.Clear();
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		if (this.backButtonSelected)
		{
			this.backButtonSelected = false;
			num = 0;
		}
		if (num == 0)
		{
			this.ReturnToMenuAction(string.Empty);
			return;
		}
	}

	public void ReturnToMenuAction(string msg)
	{
		if (StageController.IsInstantiated())
		{
			StageController.Instance.PlaySound(StageController.FxType.CLICK);
		}
		iOSController.Instance.Type = (iOSControllerType)this.controlIndex_;
		KartOptions.Instance.Controller = this.controlIndex_;
		KartOptions.Instance.SaveRegistry();
		base.SendMessage(19, new MonoBehaviourMessage(MonoBehaviourMessageType.PAUSE));
		base.gameObject.SetActiveRecursively(false);
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	private void LeftRightButtonSetting()
	{
		int num = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 3 : 0);
		int num2 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 2 : 2);
		this.panels_[2].Visible = this.controlIndex_ > num;
		this.panels_[3].Visible = this.controlIndex_ < num2;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				if (monoBehaviourMessage1Param.param_ && sender != 0)
				{
					base.gameObject.SetActiveRecursively(true);
					this.mouseManager_.InitSelectionData();
					this.controlIndex_ = (int)iOSController.Instance.Type;
					int num = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 3 : 0);
					int num2 = this.controlIndex_ - num;
					this.panels_[0].UV = num2;
					this.LeftRightButtonSetting();
					if (this.panelManager_.Update())
					{
						this.mesh_.Clear();
						this.panelManager_.UpdateMesh(ref this.mesh_);
					}
				}
				else if (!monoBehaviourMessage1Param.param_)
				{
					base.gameObject.SetActiveRecursively(false);
				}
			}
		}
	}

	private MeshRenderer meshRenderer_;

	private GUIPanelManager panelManager_;

	private Mesh mesh_;

	private MouseManager mouseManager_ = new MouseManager();

	private bool backButtonSelected;

	private float[][] PANELINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 202f, 187f, 2f, 2f, 434f, 250f },
		new float[] { 127f, 137f, 437f, 284f, 591f, 336f },
		new float[] { 58f, 260f, 592f, 284f, 642f, 360f },
		new float[] { 732f, 260f, 696f, 284f, 746f, 360f }
	};

	private float[][] PANELINFO_FOR_IPAD = new float[][]
	{
		new float[] { 144f, 127f, 2f, 2f, 228f, 138f },
		new float[] { 94f, 105f, 230f, 162f, 316f, 162f },
		new float[] { 74f, 171f, 318f, 162f, 343f, 201f },
		new float[] { 408f, 171f, 372f, 162f, 397f, 201f }
	};

	private float[][] panelInfo_;

	private float[][] BUTTONINFO_FOR_IPHONE = new float[][] { new float[]
	{
		64f, 45f, 583f, 137f, 726f, 930f, 873f, 2f, 982f, 276f,
		1022f
	} };

	private float[][] BUTTONINFO_FOR_IPAD = new float[][] { new float[]
	{
		57f, 53f, 345f, 105f, 286f, 458f, 397f, 2f, 486f, 154f,
		510f
	} };

	private float[][] buttonInfo_;

	private float[][] TOUCHREGIONINFO_FOR_IPHONE = new float[][] { new float[] { 42f, 39f, 359f, 109f } };

	private float[][] TOUCHREGIONINFO_FOR_IPAD = new float[][] { new float[] { 42f, 39f, 359f, 109f } };

	private float[][] touchRegionInfo_;

	private float[][] BACKINFO = new float[][]
	{
		new float[] { 112f, 144f, 728f, 450f, 2f, 502f, 329f },
		new float[] { 81f, 109f, 419f, 271f, 2f, 278f, 201f }
	};

	private GUIPanelEx3PartHorz back_;

	private GUIPanelEx bg_;

	private GUIPanelEx[] panels_;

	private GUIButton[] buttons_;

	private int controlIndex_;

	private enum PanelInfo
	{
		CONTENTS,
		HEADER,
		LEFT,
		RIGHT,
		SIZE
	}

	private enum ButtonInfo
	{
		MENU,
		SIZE
	}
}
