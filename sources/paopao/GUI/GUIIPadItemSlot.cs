using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIIPadItemSlot : MonoBehaviourEx
{
	private void Awake()
	{
		this.RegistMonoBehaviour(3);
		this.panelManager_ = new GUIPanelManager();
		float[][] array = new float[3][];
		array[0] = new float[]
		{
			12f * (float)Screen.width / 800f,
			174f * (float)Screen.height / 480f,
			76f * (float)Screen.width / 800f,
			238f * (float)Screen.height / 480f,
			0f,
			0f,
			64f,
			64f
		};
		int num = 1;
		float[] array2 = new float[] { 0f, 0f, 0f, 0f, 0f, 64f, 32f, 96f };
		array2[0] = 82f * (float)Screen.width / 800f;
		array2[1] = 201f * (float)Screen.height / 480f;
		array2[2] = 114f * (float)Screen.width / 800f;
		array2[3] = 233f * (float)Screen.height / 480f;
		array[num] = array2;
		array[2] = new float[] { 224f, 76f, 288f, 140f, 0f, 64f, 32f, 96f };
		this.panelInfo_ = array;
	}

	private void Start()
	{
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.panelManager_.SetCamera(CameraManager.Instance.guiCam_);
		this.panels_ = new GUIPanelEx[3];
		for (int i = 0; i < 3; i++)
		{
			this.panels_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.panelInfo_[i], fiaTexture, 1, GUIFontCalculator.DEFAULT_GAP);
			this.panelManager_.RegistGUIInterface(this.panels_[i]);
			this.panels_[i].Visible = false;
			this.panels_[i].VerticeColor = this.PANEL_COLOR[i];
		}
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
	}

	private void Update()
	{
		if (!this.timeEvent_.IsFinish())
		{
			this.timeEvent_.Update(Time.deltaTime);
			if (this.timeEvent_.IsEventOccurred())
			{
				this.panels_[2].Visible = this.timeEvent_.GetRemainCount() % 2 != 0;
			}
			if (this.timeEvent_.IsFinish())
			{
				this.panels_[2].Visible = false;
			}
			if (this.panelManager_.Update())
			{
				this.panelManager_.UpdateMesh(ref this.mesh_);
			}
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.UPDATE_ITEMSLOTS)
		{
			UpdateGUIItemSlotMessge updateGUIItemSlotMessge = (UpdateGUIItemSlotMessge)msg;
			if (updateGUIItemSlotMessge != null)
			{
				for (int i = 0; i < updateGUIItemSlotMessge.itemSlots_.Length; i++)
				{
					if (updateGUIItemSlotMessge.itemSlots_[i] == GameItem.NONE)
					{
						this.panels_[i].Visible = false;
					}
					else
					{
						this.panels_[i].SetUV(this.ITEM_UVS[(int)updateGUIItemSlotMessge.itemSlots_[i]]);
						this.panels_[i].Visible = true;
					}
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.GET_ITEM)
		{
			MonoBehaviourMessage1Param<GameItem> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<GameItem>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				this.panels_[2].Visible = true;
				this.panels_[2].SetUV(this.ITEM_UVS[(int)monoBehaviourMessage1Param.param_]);
				this.timeEvent_.Reset(0.2f, 3);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param2 = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param2 != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param2.param_);
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			base.gameObject.SetActiveRecursively(true);
		}
		if (this.panelManager_.Update())
		{
			this.panelManager_.UpdateMesh(ref this.mesh_);
		}
	}

	private int[] ITEM_UVS = new int[] { 0, 1, 2, 3, 4, 5, 5, 6, 7 };

	private float[][] panelInfo_;

	private Color[] PANEL_COLOR = new Color[]
	{
		Color.white,
		Color.gray,
		Color.white
	};

	private MeshRenderer meshRenderer_;

	private GUIPanelManager panelManager_;

	private GUIPanelEx[] panels_;

	private Mesh mesh_;

	private TimeEvent timeEvent_ = new TimeEvent();

	private enum PanelType
	{
		ITEMSLOT_0,
		ITEMSLOT_1,
		GET_ITEM,
		SIZE
	}
}
