using System;
using UnityEngine;

public class GUIQuitPopup : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(21);
		this.panelManager_ = new GUIPanelManager();
		this.buttonInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.BUTTONINFO_FOR_IPHONE : this.BUTTONINFO_FOR_IPHONE);
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.PANELINFO_FOR_IPHONE : this.PANELINFO_FOR_IPHONE);
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
		this.buttons_ = new GUIButton[2];
		for (int i = 0; i < 2; i++)
		{
			this.buttons_[i] = new GUIButton(0, this.buttonInfo_[i], fiaTexture, 2, new Vector3(77f, 0f, 1f));
			this.mouseManager_.Insert(i, this.buttons_[i]);
			this.panelManager_.RegistGUIInterface(this.buttons_[i]);
		}
		this.panels_ = new GUIPanelEx[2];
		for (int j = 0; j < 2; j++)
		{
			this.panels_[j] = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[j], fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.panels_[j]);
		}
		this.panels_[0].UV = this.controlIndex_;
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
		if (this.backButtonSelected)
		{
			this.backButtonSelected = false;
			num = 0;
		}
		this.ButtonActionWithIndex(num);
		int pushedKey = this.mouseManager_.GetPushedKey();
		for (int i = 0; i < this.buttons_.Length; i++)
		{
			flag |= this.buttons_[i].SetPushed(pushedKey == i);
		}
		if (flag && this.panelManager_.Update())
		{
			this.mesh_.Clear();
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	public void ButtonActionWithIndex(int selectedKey)
	{
		if (selectedKey == 1)
		{
			base.gameObject.SetActiveRecursively(false);
			base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.QUIT));
			return;
		}
		if (selectedKey == 0)
		{
			base.SendMessage(19, new MonoBehaviourMessage(MonoBehaviourMessageType.PAUSE));
			base.gameObject.SetActiveRecursively(false);
		}
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
		new float[] { 288f, 262f, 436f, 252f, 694f, 282f },
		new float[] { 127f, 137f, 437f, 338f, 591f, 390f }
	};

	private float[][] panelInfo_;

	private float[][] BUTTONINFO_FOR_IPHONE = new float[][]
	{
		new float[]
		{
			242f, 379f, 458f, 429f, 572f, 972f, 647f, 2f, 953f, 218f,
			980f
		},
		new float[]
		{
			477f, 379f, 693f, 429f, 572f, 972f, 647f, 2f, 924f, 218f,
			951f
		}
	};

	private float[][] buttonInfo_;

	private float[][] BACKINFO = new float[][]
	{
		new float[] { 112f, 144f, 728f, 450f, 2f, 502f, 329f },
		new float[] { 84f, 109f, 419f, 271f, 2f, 278f, 201f }
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
		SIZE
	}

	private enum ButtonInfo
	{
		CANCEL,
		CONFIRM,
		SIZE
	}
}
