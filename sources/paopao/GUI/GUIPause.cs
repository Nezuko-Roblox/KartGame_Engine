using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIPause : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(19);
		this.panelManager_ = new GUIPanelManager();
		this.buttonInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.BUTTONINFO_FOR_IPHONE : this.BUTTONINFO_FOR_IPHONE);
		this.touchRegionInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.TOUCHREGIONINFO_FOR_IPAD : this.TOUCHREGIONINFO_FOR_IPHONE);
	}

	private void Start()
	{
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.panelManager_.SetCamera(CameraManager.Instance.guiCam_);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 530f, 58f, 533f, 61f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.back_ = instance.CreateByWindowSpace(num, array, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.back_);
		this.buttons_ = new GUIButton[this.buttonInfo_.Length];
		for (int i = 0; i < this.buttonInfo_.Length; i++)
		{
			this.buttons_[i] = new GUIButton(0, this.buttonInfo_[i], fiaTexture, 3, new Vector3((i != 0) ? 149f : 298f, 0f, 1f));
			this.mouseManager_.Insert(i, this.buttons_[i]);
			this.panelManager_.RegistGUIInterface(this.buttons_[i]);
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.mesh_.Clear();
		this.panelManager_.Update();
		this.panelManager_.UpdateMesh(ref this.mesh_);
		base.gameObject.SetActiveRecursively(false);
	}

	private void Update()
	{
		this.mouseManager_.Update();
		int pushedKey = this.mouseManager_.GetPushedKey();
		bool flag = false;
		for (int i = 0; i < this.buttons_.Length; i++)
		{
			flag |= this.buttons_[i].SetPushed(pushedKey == i);
		}
		if (flag && this.panelManager_.Update())
		{
			this.mesh_.Clear();
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		if (this.mouseManager_.IsSelected() || this.backButtonSelected)
		{
			int num = this.mouseManager_.GetSelectedKey();
			if (this.backButtonSelected)
			{
				this.backButtonSelected = false;
				num = 0;
			}
			if (StageController.IsInstantiated())
			{
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
			this.mouseManager_.InitSelectionData();
			if (num == 0)
			{
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.RESUME));
			}
			else
			{
				if (num == 1)
				{
					base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.RESTART));
					base.gameObject.SetActiveRecursively(false);
					return;
				}
				if (num == 2)
				{
					MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
					monoBehaviourMessage1Param.Initialize(true);
					base.SendMessage(20, monoBehaviourMessage1Param);
					base.gameObject.SetActiveRecursively(false);
					return;
				}
				if (num == 3)
				{
					MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<bool>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_UI);
					monoBehaviourMessage1Param2.Initialize(true);
					base.SendMessage(21, monoBehaviourMessage1Param2);
					base.gameObject.SetActiveRecursively(false);
					return;
				}
			}
		}
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.PAUSE)
		{
			base.gameObject.SetActiveRecursively(true);
			this.panelManager_.Update();
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private MeshRenderer meshRenderer_;

	private GUIPanelManager panelManager_;

	private Mesh mesh_;

	private MouseManager mouseManager_ = new MouseManager();

	private bool backButtonSelected;

	private float[][] BUTTONINFO_FOR_IPHONE = new float[][]
	{
		new float[]
		{
			64f, 45f, 583f, 137f, 577f, 930f, 724f, 2f, 856f, 276f,
			896f
		},
		new float[]
		{
			193f, 144f, 629f, 236f, 726f, 930f, 873f, 2f, 898f, 276f,
			938f
		},
		new float[]
		{
			239f, 243f, 675f, 335f, 726f, 930f, 873f, 2f, 940f, 276f,
			980f
		},
		new float[]
		{
			285f, 342f, 721f, 434f, 726f, 930f, 873f, 2f, 982f, 276f,
			1022f
		}
	};

	private float[][] TOUCHREGIONINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 42f, 39f, 359f, 109f },
		new float[] { 113f, 109f, 385f, 165f },
		new float[] { 139f, 165f, 411f, 221f },
		new float[] { 165f, 221f, 437f, 287f }
	};

	private float[][] TOUCHREGIONINFO_FOR_IPAD = new float[][]
	{
		new float[] { 42f, 39f, 359f, 109f },
		new float[] { 113f, 109f, 385f, 165f },
		new float[] { 139f, 165f, 411f, 221f },
		new float[] { 165f, 221f, 437f, 287f }
	};

	private float[][] buttonInfo_;

	private float[][] touchRegionInfo_;

	private GUIButton[] buttons_;

	private GUIPanelEx back_;

	private enum ButtonInfo
	{
		RETURN_TO_RACE,
		RESTART,
		CONTROLLER,
		QUIT
	}
}
