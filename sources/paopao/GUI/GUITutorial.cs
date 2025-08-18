using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUITutorial : FiaGUILayer
{
	public GUITutorial()
	{
		float[][] array = new float[3][];
		array[0] = new float[] { 96f, 232f, 244f, 290f };
		array[1] = new float[] { 246f, 232f, 394f, 290f };
		this.TOUCHREGION_INFO = array;
		this.step_ = -1;
		base..ctor();
	}

	public override void Initialize()
	{
		this.panelManager_ = new GUIPanelManager();
		this.panelManager_.SetCamera(CameraManager.Instance.guiCam_);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.DoInit();
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.mesh_.Clear();
		this.panelManager_.Update();
		this.panelManager_.UpdateMesh(ref this.mesh_);
		this.initialize_ = true;
	}

	public override void DoInit()
	{
		base.DoInit();
		this.RegistMonoBehaviour(1072);
		this.mouseManager_ = new MouseManager();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 344f, 208f, 345f, 209f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.bg_ = instance.CreateByWindowSpace(num, array, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.bg_);
		this.back_ = new GUIPanelEx3PartVert(0, new float[] { 105f, 86f, 697f, 328f, 260f, 971f, 1022f }, fiaTexture, 4, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.back_.RegistPanelManager(this.panelManager_);
		this.img_ = new GUIPanelEx[6];
		for (int i = 0; i < 6; i++)
		{
			this.img_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.PANEL_INFO[i], fiaTexture, 3, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.img_[i]);
		}
		this.buttons_ = new GUIButton[3];
		for (int j = 0; j < 3; j++)
		{
			this.buttons_[j] = new GUIButton(0, this.BUTTON_INFO[j], fiaTexture, 2, new Vector3(129f, 0f, 1f));
			if (this.TOUCHREGION_INFO[j] != null)
			{
			}
			this.panelManager_.RegistGUIInterface(this.buttons_[j]);
			this.mouseManager_.Insert(j, this.buttons_[j]);
		}
		float[] array2 = new float[]
		{
			120f, 222f, 682f, 310f, 2f, 2f, 564f, 90f, 2f, 92f,
			564f, 180f, 2f, 182f, 564f, 270f, 2f, 272f, 564f, 360f,
			2f, 362f, 564f, 450f, 2f, 452f, 564f, 540f
		};
		this.contents_ = new GUIUVScrollImage(0, array2, fiaTexture, 2, null);
		this.contents_.RegistPanelManager(this.panelManager_);
		this.SetStep(0);
		base.gameObject.SetActiveRecursively(false);
	}

	protected override void BeforePanelUpdate()
	{
		base.BeforePanelUpdate();
		this.contents_.Update();
		this.mouseManager_.Update();
		int pushedKey = this.mouseManager_.GetPushedKey();
		for (int i = 0; i < this.buttons_.Length; i++)
		{
			this.buttons_[i].SetPushed(pushedKey == i);
		}
	}

	protected override void AfterPanelUpdate()
	{
		base.AfterPanelUpdate();
		if (this.mouseManager_.IsSelected())
		{
			if (StageController.IsInstantiated())
			{
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
			int selectedKey = this.mouseManager_.GetSelectedKey();
			if (selectedKey == 0)
			{
				base.gameObject.SetActiveRecursively(false);
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.RESUME));
				KartOptions.Instance.SetQuestFlag(KartOptions.QuestFlag.TUTORIAL, true);
				KartOptions.Instance.SaveRegistry();
			}
			else if (selectedKey == 1)
			{
				this.NextStep();
			}
			else if (selectedKey == 2)
			{
				base.gameObject.SetActiveRecursively(false);
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.RESUME));
				KartOptions.Instance.SetQuestFlag(KartOptions.QuestFlag.TUTORIAL, true);
				KartOptions.Instance.SaveRegistry();
			}
		}
	}

	private void NextStep()
	{
		this.SetStep(this.step_ + 1);
	}

	private void SetStep(int step)
	{
		if (this.step_ == step)
		{
			return;
		}
		this.step_ = step;
		foreach (GUIPanelEx guipanelEx in this.img_)
		{
			guipanelEx.Visible = false;
		}
		this.img_[0].Visible = true;
		switch (this.step_)
		{
		case 0:
			this.img_[1].Visible = true;
			break;
		case 1:
			this.img_[2].Visible = true;
			break;
		case 2:
		case 3:
			this.img_[3].Visible = true;
			break;
		case 4:
			this.img_[4].Visible = true;
			break;
		case 5:
			this.img_[5].Visible = true;
			break;
		}
		bool[] array2;
		if (this.step_ == 5)
		{
			array2 = new bool[]
			{
				default(bool),
				default(bool),
				true
			};
		}
		else
		{
			bool[] array3 = new bool[3];
			array3[0] = true;
			array3[1] = true;
			array2 = array3;
		}
		for (int j = 0; j < this.buttons_.Length; j++)
		{
			this.buttons_[j].Visible = array2[j];
		}
		this.contents_.SetUV(this.step_);
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL && !base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
		}
	}

	private GUIPanelEx bg_;

	private GUIPanelEx3PartVert back_;

	private GUIPanelEx[] img_;

	private GUIButton[] buttons_;

	private GUIUVScrollImage contents_;

	private MouseManager mouseManager_;

	private float[][] PANEL_INFO = new float[][]
	{
		new float[] { 328f, 115f, 813f, 93f, 959f, 169f },
		new float[] { 197f, 101f, 566f, 2f, 914f, 91f },
		new float[] { 197f, 101f, 566f, 93f, 811f, 206f },
		new float[] { 197f, 101f, 566f, 208f, 931f, 282f },
		new float[] { 197f, 101f, 566f, 284f, 723f, 346f },
		new float[] { 197f, 101f, 566f, 348f, 723f, 410f }
	};

	private float[][] BUTTON_INFO = new float[][]
	{
		new float[]
		{
			148f, 345f, 396f, 419f, 2f, 948f, 129f, 2f, 918f, 250f,
			946f
		},
		new float[]
		{
			432f, 345f, 680f, 419f, 2f, 948f, 129f, 2f, 888f, 250f,
			916f
		},
		new float[]
		{
			432f, 345f, 680f, 419f, 2f, 948f, 129f, 2f, 858f, 250f,
			886f
		}
	};

	private float[][] TOUCHREGION_INFO;

	private int step_;

	private enum PanelType
	{
		PHONE,
		STEER,
		BREAK,
		DRIFT,
		ITEM,
		BOOSTER,
		SIZE
	}

	private enum ButtonType
	{
		SKIP,
		NEXT,
		FINISH,
		SIZE
	}
}
