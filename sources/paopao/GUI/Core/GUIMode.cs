using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIMode : FiaGUILayer
{
	public GUIMode()
	{
		int[] array = new int[]
		{
			-1, -1, -1, -1, 5, 6, 5, 6, 5, 6,
			4, 6, 4, 6, 0, -1, -1, -1, -1, -1,
			5, -1, -1, -1, -1, -1, -1, -1, 0, -1
		};
		array[14] = GUIMode.STAGE_BUTTON_INDEX[(int)KartManager.Instance.parameter_.GaragePrevStage];
		array[28] = GUIMode.STAGE_BUTTON_INDEX[(int)KartManager.Instance.parameter_.PrevStage];
		this.BUTTON_INDEX = array;
		base..ctor();
	}

	public override void DoInit()
	{
		this.RegistMonoBehaviour(269);
		GUIMode.isRankings_ = IsRankings.NO;
		StageController.Instance.BeginStage();
		this.toStage_ = (MonoBehaviourMessage2Param<int, int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SIMPLE_MESSAGE);
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.panelManager_.SetCamera(Camera.main);
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 614f, 2f, 802f, 52f };
		array[0] = 306f * (float)Screen.width / 800f;
		array[1] = 10f * (float)Screen.height / 480f;
		array[2] = 494f * (float)Screen.width / 800f;
		array[3] = 60f * (float)Screen.height / 480f;
		this.title_ = instance.CreateByWindowSpace(num, array, fiaTexture, 3, new Vector3(2f, 2f, 12f));
		this.panelManager_.RegistGUIInterface(this.title_);
		StageType stage = KartManager.Instance.parameter_.Stage;
		this.title_.SetUV(GUIMode.STAGE_TITLE_INDEX[(int)stage]);
		if (stage == StageType.STORE)
		{
			this.title_.Visible = false;
		}
		if (stage == StageType.WIFI)
		{
			GUIPanelFactory instance2 = GUIPanelFactory.Instance;
			int num2 = 0;
			float[] array2 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 306f, 84f };
			array2[0] = 477f * (float)Screen.width / 800f;
			array2[1] = 396f * (float)Screen.height / 480f;
			array2[2] = 781f * (float)Screen.width / 800f;
			array2[3] = 478f * (float)Screen.height / 480f;
			this.race_ = instance2.CreateByWindowSpace(num2, array2, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		}
		else
		{
			GUIPanelFactory instance3 = GUIPanelFactory.Instance;
			int num3 = 0;
			float[] array3 = new float[] { 0f, 0f, 0f, 0f, 308f, 2f, 612f, 84f };
			array3[0] = 477f * (float)Screen.width / 800f;
			array3[1] = 396f * (float)Screen.height / 480f;
			array3[2] = 781f * (float)Screen.width / 800f;
			array3[3] = 478f * (float)Screen.height / 480f;
			this.race_ = instance3.CreateByWindowSpace(num3, array3, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
		}
		this.panelManager_.RegistGUIInterface(this.race_);
		this.race_.Visible = stage != StageType.GARAGE && stage != StageType.INFO && stage != StageType.STORE;
		this.race_.SetUV(0);
		this.mouseManager_.Insert(2, this.race_);
		int stage2 = (int)KartManager.Instance.parameter_.Stage;
		for (int i = 0; i < 2; i++)
		{
			this.lrbuttons_[i] = null;
			if (this.BUTTON_INDEX[stage2 * 2 + i] != -1)
			{
				int num4 = this.BUTTON_INDEX[stage2 * 2 + i];
				float[] array4 = new float[11];
				Array.Copy(this.BUTTON_POS[i], 0, array4, 0, this.BUTTON_POS[i].Length);
				Array.Copy(this.BUTTON_INFO[i], 0, array4, 4, this.BUTTON_INFO[i].Length);
				this.lrbuttons_[i] = new GUIButton(0, array4, fiaTexture, 3, new Vector3(129f, 0f, 1f), new Vector3(2f, 2f, 104f));
				this.lrbuttons_[i].SetNameUV(num4);
				this.panelManager_.RegistGUIInterface(this.lrbuttons_[i]);
				this.mouseManagerLR_.Insert(i, this.lrbuttons_[i]);
			}
		}
		if (KartManager.Instance.parameter_.Stage != StageType.INFO)
		{
			GUIKartViewer.Instance.ChangeKartCharacter((byte)KartOptions.Instance.Kart, (byte)KartOptions.Instance.Character);
			GUIKartViewer.Instance.Show();
			return;
		}
		GUIKartViewer.Instance.Hide();
	}

	protected override void Update()
	{
		this.mouseManager_.Update();
		this.mouseManagerLR_.Update();
		int num = this.mouseManager_.GetPushedKey();
		if (num == -1)
		{
			num = this.mouseManagerLR_.GetPushedKey();
		}
		for (int i = 0; i < 2; i++)
		{
			if (this.lrbuttons_[i] != null)
			{
				this.lrbuttons_[i].SetPushed(num == i);
			}
		}
		StageType stage = KartManager.Instance.parameter_.Stage;
		if (stage == StageType.SINGLE_ITEM || stage == StageType.SINGLE_SPEED)
		{
			if (GUIMode.isRankings_ == IsRankings.NO || GUIMode.isRankings_ == IsRankings.TRANSITION_TRACK_IN || GUIMode.isRankings_ == IsRankings.TRANSITION_TRACK_OUT)
			{
				this.lrbuttons_[0].SetNameUV(this.BUTTON_INDEX[(int)(stage * StageType.SINGLE_ITEM)]);
				this.title_.SetUV(GUIMode.STAGE_TITLE_INDEX[(int)stage]);
			}
			else if (GUIMode.isRankings_ == IsRankings.YES || GUIMode.isRankings_ == IsRankings.TRANSITION_RANKINGS_IN || GUIMode.isRankings_ == IsRankings.TRANSITION_RANKINGS_OUT)
			{
				this.lrbuttons_[0].SetNameUV((int)stage);
				this.title_.SetUV(1);
			}
		}
		if (stage == StageType.WIFI)
		{
			if (this.servers_ != null && this.servers_.Length > 0)
			{
				this.race_.UV = ((num != 2) ? 0 : 2);
			}
			else
			{
				this.race_.UV = ((num != 2) ? (((int)(Time.time * 2f % 2f) != 1) ? 0 : 1) : 2);
			}
		}
		else
		{
			this.race_.UV = ((num != 2) ? (((int)(Time.time * 2f % 2f) != 1) ? 0 : 1) : 2);
		}
		if (stage == StageType.WAITROOM_CLIENT || stage == StageType.WAITROOM_HOST)
		{
			BaseWaitRoomStage baseWaitRoomStage = (BaseWaitRoomStage)NetworkManager.Inst.Stage;
			if (baseWaitRoomStage != null)
			{
				this.race_.Visible = !baseWaitRoomStage.ready_;
			}
		}
		base.Update();
		int num2 = this.mouseManager_.GetSelectedKey();
		if (num2 == -1)
		{
			num2 = this.mouseManagerLR_.GetSelectedKey();
		}
		if (this.backButtonSelected)
		{
			this.backButtonSelected = false;
			num2 = 0;
		}
		this.ButtonActionWithIndex(num2);
	}

	public void BackButtonAction(string msg)
	{
		this.backButtonSelected = true;
	}

	public void ButtonActionWithIndex(int selectedIndex)
	{
		if (selectedIndex != -1)
		{
			switch (selectedIndex)
			{
			case 0:
			case 1:
				if (GUIMode.isRankings_ != IsRankings.TRANSITION_TRACK_IN && GUIMode.isRankings_ != IsRankings.TRANSITION_RANKINGS_IN && GUIMode.isRankings_ != IsRankings.TRANSITION_TRACK_OUT && GUIMode.isRankings_ != IsRankings.TRANSITION_RANKINGS_OUT)
				{
					if (selectedIndex == 0 && GUIMode.isRankings_ == IsRankings.YES)
					{
						GUIRankingHeaderInSingle.isMoveToTrack_ = true;
					}
					else
					{
						int nameUV = this.lrbuttons_[selectedIndex].GetNameUV();
						if (nameUV == 0)
						{
							MonoBehaviourExCenter.Instance.SendMessage(0, 514, this.toStage_.Initialize(1, 0));
						}
						else if (nameUV == 1)
						{
							StageController.Instance.ChangeStage(KartManager.Instance.parameter_.PrevStage);
						}
						else
						{
							for (int i = 0; i < GUIMode.STAGE_BUTTON_INDEX.Length; i++)
							{
								if (GUIMode.STAGE_BUTTON_INDEX[i] == nameUV)
								{
									StageController.Instance.ChangeStage((StageType)i);
									break;
								}
							}
						}
						StageController.Instance.PlaySound(StageController.FxType.CLICK);
					}
				}
				break;
			case 2:
				switch (KartManager.Instance.parameter_.Stage)
				{
				case StageType.SINGLE_ITEM:
				case StageType.SINGLE_SPEED:
					GUIMode.isRankings_ = IsRankings.NO;
					StageController.Instance.ChangeStage(StageType.GAME);
					StageController.Instance.PlaySound(StageController.FxType.START);
					break;
				case StageType.WIFI:
					MonoBehaviourExCenter.Instance.SendMessage(0, 514, this.toStage_.Initialize(1, 0));
					StageController.Instance.PlaySound(StageController.FxType.CLICK);
					break;
				case StageType.WAITROOM_HOST:
				case StageType.WAITROOM_CLIENT:
					MonoBehaviourExCenter.Instance.SendMessage(0, 515, this.toStage_.Initialize(1, 0));
					StageController.Instance.PlaySound(StageController.FxType.START);
					break;
				}
				break;
			}
		}
	}

	public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
	{
		if (msg.type_ == MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST)
		{
			MonoBehaviourMessage2Param<int, Peer[]> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<int, Peer[]>)msg;
			if (monoBehaviourMessage2Param != null)
			{
				this.servers_ = monoBehaviourMessage2Param.rparam_;
			}
		}
	}

	private MouseManager mouseManagerLR_ = new MouseManager(1);

	private MouseManager mouseManager_ = new MouseManager(2);

	private GUIPanelEx title_;

	private GUIPanelEx race_;

	private GUIButton[] lrbuttons_ = new GUIButton[2];

	private bool backButtonSelected;

	private Peer[] servers_;

	public static IsRankings isRankings_ = IsRankings.NO;

	private float[][] BUTTON_POS = new float[][]
	{
		new float[] { 12f, 6f, 190f, 62f },
		new float[] { 598f, 6f, 776f, 62f }
	};

	private float[][] BUTTON_INFO = new float[][]
	{
		new float[] { 766f, 396f, 893f, 614f, 158.5f, 804f, 181.5f },
		new float[] { 766f, 454f, 893f, 614f, 158f, 804f, 181f }
	};

	private static int[] STAGE_BUTTON_INDEX = new int[]
	{
		-1, 5, 2, 3, 4, 1, 1, 6, -1, -1,
		-1, -1, -1, -1, 7
	};

	private static int[] STAGE_TITLE_INDEX = new int[]
	{
		-1, -1, 3, 5, 2, 4, 4, 0, -1, -1,
		-1, -1, -1, -1, 6
	};

	private int[] BUTTON_INDEX;

	private MonoBehaviourMessage2Param<int, int> toStage_;

	private enum ButtonIndex
	{
		CREATE,
		WAITROOM,
		ITEM,
		SPEED,
		WIFI,
		MAIN,
		GARAGE,
		STORE
	}

	private enum TitleIndex
	{
		GARAGE,
		RANKINGS,
		WIFI,
		ITEM,
		WAITROOM,
		SPEED,
		STORE
	}
}
