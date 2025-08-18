using System;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIIPad : MonoBehaviourEx
{
	public GUIIPad()
	{
		InputStatusEnum[,] array = new InputStatusEnum[3, 5];
		array[0, 0] = InputStatusEnum.NONE;
		array[0, 1] = InputStatusEnum.DRIFT;
		array[0, 2] = InputStatusEnum.NONE;
		array[0, 3] = InputStatusEnum.BREAK;
		array[0, 4] = InputStatusEnum.NONE;
		array[1, 0] = InputStatusEnum.DRIFT;
		array[1, 1] = InputStatusEnum.LEFT;
		array[1, 2] = InputStatusEnum.BREAK;
		array[1, 3] = InputStatusEnum.RIGHT;
		array[1, 4] = InputStatusEnum.DRIFT;
		array[2, 0] = InputStatusEnum.DRIFTL;
		array[2, 1] = InputStatusEnum.LEFT;
		array[2, 2] = InputStatusEnum.BREAK;
		array[2, 3] = InputStatusEnum.RIGHT;
		array[2, 4] = InputStatusEnum.DRIFTR;
		this.INPUTSTATUS_FOR_IPHONE = array;
		this.rankValue_ = -1;
		base..ctor();
	}

	private void Awake()
	{
		float[][] array = new float[5][];
		int num = 0;
		float[] array2 = new float[] { 12f, 0f, 76f, 0f, 2f, 2f, 66f, 68f };
		array2[0] = 12f * (float)Screen.width / 800f;
		array2[1] = 288f * (float)Screen.height / 480f;
		array2[2] = 76f * (float)Screen.width / 800f;
		array2[3] = 354f * (float)Screen.height / 480f;
		array[num] = array2;
		int num2 = 1;
		float[] array3 = new float[] { 12f, 0f, 76f, 0f, 2f, 2f, 66f, 68f };
		array3[0] = 12f * (float)Screen.width / 800f;
		array3[1] = 380f * (float)Screen.height / 480f;
		array3[2] = 76f * (float)Screen.width / 800f;
		array3[3] = 446f * (float)Screen.height / 480f;
		array[num2] = array3;
		int num3 = 2;
		float[] array4 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 66f, 68f };
		array4[0] = 620f * (float)Screen.width / 800f;
		array4[1] = 380f * (float)Screen.height / 480f;
		array4[2] = 684f * (float)Screen.width / 800f;
		array4[3] = 446f * (float)Screen.height / 480f;
		array[num3] = array4;
		int num4 = 3;
		float[] array5 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 66f, 68f };
		array5[0] = 724f * (float)Screen.width / 800f;
		array5[1] = 380f * (float)Screen.height / 480f;
		array5[2] = 788f * (float)Screen.width / 800f;
		array5[3] = 446f * (float)Screen.height / 480f;
		array[num4] = array5;
		int num5 = 4;
		float[] array6 = new float[] { 0f, 0f, 0f, 0f, 2f, 2f, 66f, 68f };
		array6[0] = 724f * (float)Screen.width / 800f;
		array6[1] = 288f * (float)Screen.height / 480f;
		array6[2] = 788f * (float)Screen.width / 800f;
		array6[3] = 354f * (float)Screen.height / 480f;
		array[num5] = array6;
		this.controlInfo_ = array;
		GUIPanelLayout[] array7 = new GUIPanelLayout[26];
		GUIPanelLayout[] array8 = array7;
		int num6 = 0;
		float[] array9 = new float[] { 0f, 0f, 2f, 70f, 164f, 278f };
		array9[0] = 630f + (float)Screen.width - 800f;
		array9[1] = 70f;
		array8[num6] = new GUIPanelLayout(array9, 2);
		GUIPanelLayout[] array10 = array7;
		int num7 = 1;
		float[] array11 = new float[] { 0f, 0f, 2f, 280f, 164f, 302f };
		array11[0] = 630f + (float)Screen.width - 800f;
		array11[1] = 222f;
		array10[num7] = new GUIPanelLayout(array11, 2);
		GUIPanelLayout[] array12 = array7;
		int num8 = 2;
		float[] array13 = new float[] { 0f, 0f, 166f, 132f, 190f, 164f };
		array13[0] = 727f + (float)Screen.width - 800f;
		array13[1] = 74f;
		array12[num8] = new GUIPanelLayout(array13, 1);
		GUIPanelLayout[] array14 = array7;
		int num9 = 3;
		float[] array15 = new float[] { 0f, 0f, 266f, 166f, 282f, 190f };
		array15[0] = 752f + (float)Screen.width - 800f;
		array15[1] = 82f;
		array14[num9] = new GUIPanelLayout(array15, 1);
		GUIPanelLayout[] array16 = array7;
		int num10 = 4;
		float[] array17 = new float[] { 0f, 0f, 166f, 166f, 184f, 190f };
		array17[0] = 770f + (float)Screen.width - 800f;
		array17[1] = 82f;
		array16[num10] = new GUIPanelLayout(array17, 1);
		GUIPanelLayout[] array18 = array7;
		int num11 = 5;
		float[] array19 = new float[] { 0f, 0f, 166f, 104f, 182f, 130f };
		array19[0] = 655f + (float)Screen.width - 800f;
		array19[1] = 248f;
		array18[num11] = new GUIPanelLayout(array19, 1);
		array7[6] = new GUIPanelLayout(new float[6], 1);
		GUIPanelLayout[] array20 = array7;
		int num12 = 7;
		float[] array21 = new float[] { 0f, 0f, 166f, 104f, 182f, 130f };
		array21[0] = 684f + (float)Screen.width - 800f;
		array21[1] = 248f;
		array20[num12] = new GUIPanelLayout(array21, 1);
		GUIPanelLayout[] array22 = array7;
		int num13 = 8;
		float[] array23 = new float[] { 0f, 0f, 166f, 104f, 182f, 130f };
		array23[0] = 703f + (float)Screen.width - 800f;
		array23[1] = 248f;
		array22[num13] = new GUIPanelLayout(array23, 1);
		array7[9] = new GUIPanelLayout(new float[6], 1);
		GUIPanelLayout[] array24 = array7;
		int num14 = 10;
		float[] array25 = new float[] { 0f, 0f, 166f, 104f, 182f, 130f };
		array25[0] = 732f + (float)Screen.width - 800f;
		array25[1] = 248f;
		array24[num14] = new GUIPanelLayout(array25, 1);
		GUIPanelLayout[] array26 = array7;
		int num15 = 11;
		float[] array27 = new float[] { 0f, 0f, 166f, 104f, 182f, 130f };
		array27[0] = 751f + (float)Screen.width - 800f;
		array27[1] = 248f;
		array26[num15] = new GUIPanelLayout(array27, 1);
		GUIPanelLayout[] array28 = array7;
		int num16 = 12;
		float[] array29 = new float[] { 0f, 0f, 364f, 104f, 374f, 120f };
		array29[0] = 673f + (float)Screen.width - 800f;
		array29[1] = 225f;
		array28[num16] = new GUIPanelLayout(array29, 1);
		array7[13] = new GUIPanelLayout(new float[6], 1);
		GUIPanelLayout[] array30 = array7;
		int num17 = 14;
		float[] array31 = new float[] { 0f, 0f, 364f, 104f, 374f, 120f };
		array31[0] = 694f + (float)Screen.width - 800f;
		array31[1] = 225f;
		array30[num17] = new GUIPanelLayout(array31, 1);
		GUIPanelLayout[] array32 = array7;
		int num18 = 15;
		float[] array33 = new float[] { 0f, 0f, 364f, 104f, 374f, 120f };
		array33[0] = 705f + (float)Screen.width - 800f;
		array33[1] = 225f;
		array32[num18] = new GUIPanelLayout(array33, 1);
		array7[16] = new GUIPanelLayout(new float[6], 1);
		GUIPanelLayout[] array34 = array7;
		int num19 = 17;
		float[] array35 = new float[] { 0f, 0f, 364f, 104f, 374f, 120f };
		array35[0] = 726f + (float)Screen.width - 800f;
		array35[1] = 225f;
		array34[num19] = new GUIPanelLayout(array35, 1);
		GUIPanelLayout[] array36 = array7;
		int num20 = 18;
		float[] array37 = new float[] { 0f, 0f, 364f, 104f, 374f, 120f };
		array37[0] = 737f + (float)Screen.width - 800f;
		array37[1] = 225f;
		array36[num20] = new GUIPanelLayout(array37, 1);
		GUIPanelLayout[] array38 = array7;
		int num21 = 19;
		float[] array39 = new float[] { 0f, 0f, 166f, 70f, 190f, 102f };
		array39[0] = 339f * (float)Screen.width / 800f;
		array39[1] = 402f * (float)Screen.height / 480f;
		array38[num21] = new GUIPanelLayout(array39, 1);
		GUIPanelLayout[] array40 = array7;
		int num22 = 20;
		float[] array41 = new float[] { 0f, 0f, 166f, 70f, 190f, 102f };
		array41[0] = 359f * (float)Screen.width / 800f;
		array41[1] = 402f * (float)Screen.height / 480f;
		array40[num22] = new GUIPanelLayout(array41, 1);
		GUIPanelLayout[] array42 = array7;
		int num23 = 21;
		float[] array43 = new float[] { 0f, 0f, 166f, 70f, 190f, 102f };
		array43[0] = 379f * (float)Screen.width / 800f;
		array43[1] = 402f * (float)Screen.height / 480f;
		array42[num23] = new GUIPanelLayout(array43, 1);
		GUIPanelLayout[] array44 = array7;
		int num24 = 22;
		float[] array45 = new float[] { 0f, 0f, 474f, 70f, 520f, 93f };
		array45[0] = 403f * (float)Screen.width / 800f;
		array45[1] = 413f * (float)Screen.height / 480f;
		array44[num24] = new GUIPanelLayout(array45, 2);
		GUIPanelLayout[] array46 = array7;
		int num25 = 23;
		float[] array47 = new float[] { 8f, 0f, 0f, 0f, 646f, 2f, 758f, 76f };
		array47[1] = 170f * (float)Screen.height / 480f;
		array47[2] = 120f * (float)Screen.width / 800f;
		array47[3] = 244f * (float)Screen.height / 480f;
		array46[num25] = new GUIPanelLayout(array47, 2);
		int num26 = 24;
		float[] array48 = new float[] { 0f, 0f, 530f, 2f, 586f, 56f };
		array48[0] = 736f * (float)Screen.width / 800f;
		array48[1] = 8f * (float)Screen.height / 480f;
		array7[num26] = new GUIPanelLayout(array48, 3);
		array7[25] = new GUIPanelLayout(new float[] { 166f, 2f, 296f, 160f, 350f, 204f }, 1, 3);
		this.PANELINFO_FOR_IPHONE = array7;
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? this.PANELINFO_FOR_IPAD : this.PANELINFO_FOR_IPHONE);
		this.RegistMonoBehaviour(6);
		GUIPanelFactory.Instance.RegistBuilder(1, new GUIPanelPlayerBuilderForIPad());
		GUIPanelFactory.Instance.RegistBuilder(2, new GUIPanelPlayerMarkBuilderForIPad());
		GUIPanelFactory.Instance.RegistBuilder(3, new GUIPanelPlayerRankBuilderForIPad());
		GUIPanelManager.ingame_ = new GUIPanelManager();
	}

	private void Start()
	{
		if (GUIPanelManager.ingame_ == null)
		{
			return;
		}
		GUIPanelManager.ingame_.SetCamera(CameraManager.Instance.guiCam_);
		CameraManager.Instance.guiCam_.ResetAspect();
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.meshRenderer_.material = this.materialForGuiType_[(int)GUIBase.GetGUIType()];
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
		this.panels_ = new GUIPanelEx[38];
		Vector3[] array = new Vector3[]
		{
			GUIFontCalculator.DEFAULT_GAP,
			GUIFontCalculator.X2_GAP,
			GUIFontCalculator.DEFAULT_GAP,
			GUIFontCalculator.X2_GAP,
			GUIFontCalculator.Y2_GAP
		};
		for (int i = 0; i < this.panelInfo_.Length; i++)
		{
			this.panels_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(this.panelInfo_[i], fiaTexture, array[this.panelInfo_[i].layer_]);
			GUIPanelManager.ingame_.RegistGUIInterface(this.panels_[i]);
		}
		this.panels_[24].SetTouchRegionByWindowPos(702f * (float)Screen.width / 800f, 0f, (float)Screen.width, 82f * (float)Screen.height / 480f);
		int guitype = (int)GUIBase.GetGUIType();
		float[][] array2 = new float[][]
		{
			new float[] { 0f, 10f, 146f, 30f, 166f, 206f, 378f, 226f },
			new float[] { -2f, 161f, 162f, 187f, 2f, 330f, 262f, 356f }
		};
		for (int j = 0; j < 6; j++)
		{
			int num = 26 + j;
			this.panels_[num] = GUIPanelFactory.Instance.CreateByWindowSpace(1, array2[guitype], fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
			GUIPanelManager.ingame_.RegistGUIInterface(this.panels_[num]);
		}
		float[][] array3 = new float[][]
		{
			new float[] { 146f, 7f, 995f, 2f, 1022f, 28f },
			new float[] { 161f, 159f, 189f, 189f, 450f, 2f, 478f, 32f }
		};
		Vector3[] array4 = new Vector3[]
		{
			GUIFontCalculator.Y2_GAP,
			new Vector3(2f, 2f, 12f)
		};
		for (int k = 0; k < 6; k++)
		{
			int num2 = 32 + k;
			this.panels_[num2] = GUIPanelFactory.Instance.CreateByWindowSpace(2, array3[guitype], fiaTexture, 2, array4[guitype]);
			this.panels_[num2].SetUV(k);
			GUIPanelManager.ingame_.RegistGUIInterface(this.panels_[num2]);
		}
		int num3 = this.controlInfo_.Length;
		this.controlPanels_ = new GUIPanelEx[num3];
		for (int l = 0; l < num3; l++)
		{
			this.controlPanels_[l] = GUIPanelFactory.Instance.CreateByWindowSpace(0, this.controlInfo_[l], fiaTexture, 2, new Vector3(66f, 259f, 18f));
			GUIPanelManager.ingame_.RegistGUIInterface(this.controlPanels_[l]);
		}
		this.guiRank_ = (GUIIPadPlayerRank)this.panels_[25];
		for (int m = 0; m < 6; m++)
		{
			this.guiPlayer_[m] = new GUIPlayerElem((GUIIPadPlayerPanel)this.panels_[m + 26], (GUIIPadPlayerMark)this.panels_[m + 32]);
		}
		Vector2[] array5 = new Vector2[]
		{
			new Vector2(2f, (float)(Screen.height - 12)),
			new Vector2(8f, (float)(Screen.height - 164))
		};
		base.guiText.font = this.fontForGuiType_[(int)GUIBase.GetGUIType()];
		base.guiText.pixelOffset = array5[(int)GUIBase.GetGUIType()];
		base.guiText.lineSpacing = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 2f : 1.9f);
		this.mouseManager_ = new MouseManager();
		if (KartManager.Instance.parameter_.Stage == StageType.GAME)
		{
			this.mouseManager_.Insert(24, this.panels_[24]);
		}
		else
		{
			this.panels_[24].Visible = false;
		}
		float num4 = 0f;
		Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
		if (facebook.LoggedIn)
		{
			MonthlyRanking monthlyRanking = new MonthlyRanking(facebook.FBID, KartManager.Instance.parameter_.gameMode_, (int)KartManager.Instance.parameter_.track_);
			if (monthlyRanking.Load())
			{
				Record record = monthlyRanking.FindRecordWithID(facebook.FBID);
				if (record != null && record.time_ < 600f)
				{
					num4 = record.time_;
				}
			}
		}
		else
		{
			GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(KartManager.Instance.parameter_.track_, (int)KartManager.Instance.parameter_.gameMode_);
			if (bestInfo != null)
			{
				num4 = bestInfo.finishTime_;
			}
		}
		this.UpdateUVOfTime(num4, 12);
		if (GUIBase.GetGUIType() == GUIType.IPHONE)
		{
			Vector2[] array6 = new Vector2[]
			{
				Vector2.zero,
				new Vector2(2f, 2f),
				new Vector2(134f, 2f),
				new Vector2(2f, 2f),
				new Vector2(266f, 2f),
				new Vector2(398f, 2f),
				new Vector2(2f, 304f),
				new Vector2(134f, 304f)
			};
			this.CONTROL_FONT_CALCULATOR_FOR_IPHONE = new GUIFontCalculator[array6.Length];
			Vector2 vector = new Vector2(64f, 66f);
			for (int n = 0; n < array6.Length; n++)
			{
				this.CONTROL_FONT_CALCULATOR_FOR_IPHONE[n] = GUIFontCalculator.CreateByWindowSpace(fiaTexture, vector, array6[n], GUIFontCalculator.X2_GAP);
			}
			this.UpdateControlsUV();
		}
	}

	private void UpdateControlsUV()
	{
		if (GUIBase.GetGUIType() == GUIType.IPAD)
		{
			return;
		}
		int controller = KartOptions.instance_.Controller;
		for (int i = 0; i < 5; i++)
		{
			this.controlPanels_[i].Visible = this.CONTROL_UVS_FOR_IPHONE[controller, i] != 0;
			this.controlPanels_[i].FontCalculator = this.CONTROL_FONT_CALCULATOR_FOR_IPHONE[this.CONTROL_UVS_FOR_IPHONE[controller, i]];
			this.controlPanels_[i].SetUV(0);
			this.controlPanels_[i].Dirty = 5;
		}
		if (GUIPanelManager.ingame_.Update())
		{
			GUIPanelManager.ingame_.UpdateMesh(ref this.mesh_);
		}
	}

	private void UpdateForInput()
	{
		if (GUIBase.GetGUIType() == GUIType.IPAD)
		{
			return;
		}
		int controller = KartOptions.instance_.Controller;
		for (int i = 0; i < 5; i++)
		{
			InputStatus inputStatus = iOSController.Instance.GetInputStatus(this.INPUTSTATUS_FOR_IPHONE[controller, i]);
			if (inputStatus != null)
			{
				this.controlPanels_[i].SetUV((!inputStatus.pressed_) ? 0 : 1);
			}
		}
	}

	private void Update()
	{
		if (GUIPanelManager.ingame_ == null)
		{
			return;
		}
		this.mouseManager_.Update();
		int selectedKey = this.mouseManager_.GetSelectedKey();
		if (selectedKey == 24)
		{
			this.panels_[24].SetUV(0);
			if (StageController.IsInstantiated())
			{
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
			if (GUIBase.GetGUIType() == GUIType.IPHONE)
			{
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.PAUSE));
				return;
			}
		}
		else if (Debug.isDebugBuild && selectedKey == 0)
		{
			string text = string.Empty;
			text += KartManager.Instance.goCourse_.GetDebugString();
			text += PassingLog.Instance.ToString();
			string text2 = string.Format("{0}_{1}.txt", TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_).Name, DateTime.Now.ToString("MMddHHmmss"));
			string text3 = FiaUtil.recordPath;
			text3 = Path.Combine(text3, "debugging");
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			text3 = Path.Combine(text3, text2);
			StreamWriter streamWriter = new StreamWriter(text3);
			if (streamWriter != null)
			{
				streamWriter.Write(text);
				streamWriter.Flush();
			}
		}
		this.panels_[24].SetUV((!this.mouseManager_.IsPushed()) ? 0 : 1);
		this.UpdateForInput();
		this.UpdateUVOfCharIdx(KartManager.Instance.goCourse_.MaxLap, 4);
		int num = Mathf.Clamp(KartManager.Instance.goCourse_.GetLap(KartManager.PLAYER_KART_IDX), 0, KartManager.Instance.goCourse_.MaxLap);
		this.UpdateUVOfCharIdx(num, 2);
		this.UpdateUVOfTime(KartManager.Instance.GetPlayTime(), 5);
		this.UpdateUVOfSpeed(KartManager.Instance.goPlayKart_.GetKartRealSpeed() / 1.609f, 19);
		int rankValue = KartManager.Instance.goCourse_.GetRankValue();
		if (this.rankValue_ != rankValue && rankValue >= 0)
		{
			string[] array = new string[6];
			for (int i = 0; i < 6; i++)
			{
				array[i] = "\n";
			}
			for (int j = 0; j < 6; j++)
			{
				if (KartManager.Instance.goKart_[j] != null)
				{
					int rank = KartManager.Instance.goCourse_.GetRank(j);
					this.guiPlayer_[j].Update(rank, KartManager.PLAYER_KART_IDX != j);
					array[rank] = KartManager.Instance.parameter_.kart_[j].name_ + "\n";
				}
				else
				{
					this.guiPlayer_[j].SetVisible(false);
				}
			}
			this.guiRank_.Update(KartManager.Instance.goCourse_.GetMyRank());
			base.guiText.text = string.Empty;
			for (int k = 0; k < 6; k++)
			{
				GUIText guiText = base.guiText;
				guiText.text += array[k];
			}
			this.rankValue_ = rankValue;
		}
		if (GUIPanelManager.ingame_.Update())
		{
			GUIPanelManager.ingame_.UpdateMesh(ref this.mesh_);
		}
	}

	private void Dump(Vector3[] v, Vector2[] uv, int cnt)
	{
		string text = string.Empty;
		for (int i = 0; i < cnt; i++)
		{
			if (i % 4 == 0)
			{
				text = text + (i / 4).ToString() + "\n";
			}
			string text2 = text;
			text = string.Concat(new string[]
			{
				text2,
				Vector3Helper.ToStringVector3(v[i]),
				" ",
				GUIFontCalculator.ToStringVector2(uv[i]),
				"\n"
			});
			if (i % 4 == 3)
			{
				text += "\n";
			}
		}
	}

	private void UpdateUVOfCharIdx(int charIdx, int panelIdx)
	{
		this.panels_[panelIdx].UV = charIdx;
	}

	private void UpdateUVOfTime(float t, int idx)
	{
		int num = (int)t;
		this.UpdateUVOfCharIdx((int)((float)num / 60f % 10f), idx);
		this.UpdateUVOfCharIdx(num % 60 / 10, idx + 2);
		this.UpdateUVOfCharIdx(num % 10, idx + 3);
		int num2 = (int)(t * 100f) % 100;
		this.UpdateUVOfCharIdx(num2 / 10, idx + 5);
		this.UpdateUVOfCharIdx(num2 % 10, idx + 6);
	}

	private void UpdateUVOfSpeed(float t, int idx)
	{
		this.UpdateUVOfCharIdx((int)(t / 100f), idx);
		this.UpdateUVOfCharIdx((int)(t / 10f) % 10, idx + 1);
		this.UpdateUVOfCharIdx((int)(t % 10f), idx + 2);
	}

	private void FixedUpdate()
	{
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_UI)
		{
			MonoBehaviourMessage1Param<bool> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<bool>)msg;
			if (monoBehaviourMessage1Param != null)
			{
				base.gameObject.SetActiveRecursively(monoBehaviourMessage1Param.param_);
				if (monoBehaviourMessage1Param.param_)
				{
					this.UpdateControlsUV();
				}
			}
		}
		else if (msg.type_ == MonoBehaviourMessageType.PAUSE || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL || msg.type_ == MonoBehaviourMessageType.SHOW_TUTORIAL2)
		{
			base.gameObject.SetActiveRecursively(false);
		}
		else if (msg.type_ == MonoBehaviourMessageType.RESUME)
		{
			this.UpdateControlsUV();
			base.gameObject.SetActiveRecursively(true);
		}
	}

	public override void OnUnloadStage()
	{
		GUIPanelManager.ingame_ = null;
		GUIAtlasManager.RemoveAtlas("ingame");
	}

	private const float x_factor = 1f;

	private const float y_factor = 1f;

	private const float tex_width = 64f;

	private const float tex_height = 66f;

	private const float tex_row1 = 2f;

	private const float tex_row2 = 304f;

	private const float btn_width = 64f;

	private const float btn_height = 66f;

	private const float x_pos1 = 12f;

	private const float x_pos2 = 620f;

	private const float x_pos3 = 724f;

	private const float y_pos1 = 288f;

	private const float y_pos2 = 380f;

	private MeshRenderer meshRenderer_;

	private GUIPanelLayout[] panelInfo_;

	private GUIPanelLayout[] PANELINFO_FOR_IPAD = new GUIPanelLayout[0];

	private GUIPanelLayout[] PANELINFO_FOR_IPHONE = new GUIPanelLayout[]
	{
		new GUIPanelLayout(new float[] { 630f, 70f, 2f, 70f, 164f, 278f }, 2),
		new GUIPanelLayout(new float[] { 630f, 222f, 2f, 280f, 164f, 302f }, 2),
		new GUIPanelLayout(new float[] { 727f, 74f, 166f, 132f, 190f, 164f }, 1),
		new GUIPanelLayout(new float[] { 752f, 82f, 266f, 166f, 282f, 190f }, 1),
		new GUIPanelLayout(new float[] { 770f, 82f, 166f, 166f, 184f, 190f }, 1),
		new GUIPanelLayout(new float[] { 655f, 248f, 166f, 104f, 182f, 130f }, 1),
		new GUIPanelLayout(new float[6], 1),
		new GUIPanelLayout(new float[] { 684f, 248f, 166f, 104f, 182f, 130f }, 1),
		new GUIPanelLayout(new float[] { 703f, 248f, 166f, 104f, 182f, 130f }, 1),
		new GUIPanelLayout(new float[6], 1),
		new GUIPanelLayout(new float[] { 732f, 248f, 166f, 104f, 182f, 130f }, 1),
		new GUIPanelLayout(new float[] { 751f, 248f, 166f, 104f, 182f, 130f }, 1),
		new GUIPanelLayout(new float[] { 673f, 225f, 364f, 104f, 374f, 120f }, 1),
		new GUIPanelLayout(new float[6], 1),
		new GUIPanelLayout(new float[] { 694f, 225f, 364f, 104f, 374f, 120f }, 1),
		new GUIPanelLayout(new float[] { 705f, 225f, 364f, 104f, 374f, 120f }, 1),
		new GUIPanelLayout(new float[6], 1),
		new GUIPanelLayout(new float[] { 726f, 225f, 364f, 104f, 374f, 120f }, 1),
		new GUIPanelLayout(new float[] { 737f, 225f, 364f, 104f, 374f, 120f }, 1),
		new GUIPanelLayout(new float[] { 339f, 402f, 166f, 70f, 190f, 102f }, 1),
		new GUIPanelLayout(new float[] { 359f, 402f, 166f, 70f, 190f, 102f }, 1),
		new GUIPanelLayout(new float[] { 379f, 402f, 166f, 70f, 190f, 102f }, 1),
		new GUIPanelLayout(new float[] { 403f, 413f, 474f, 70f, 520f, 93f }, 2),
		new GUIPanelLayout(new float[] { 8f, 170f, 646f, 2f, 758f, 76f }, 2),
		new GUIPanelLayout(new float[] { 736f, 8f, 530f, 2f, 586f, 56f }, 3),
		new GUIPanelLayout(new float[] { 166f, 2f, 296f, 160f, 350f, 204f }, 1, 3)
	};

	private float[][] controlInfo_;

	private GUIPanelEx[] controlPanels_;

	private GUIPanelEx[] panels_;

	private GUIPlayerElem[] guiPlayer_ = new GUIPlayerElem[6];

	private GUIIPadPlayerRank guiRank_;

	public Material[] materialForGuiType_;

	public Font[] fontForGuiType_;

	private MouseManager mouseManager_;

	private Mesh mesh_;

	private int[,] CONTROL_UVS_FOR_IPHONE = new int[,]
	{
		{ 0, 2, 0, 3, 0 },
		{ 2, 4, 3, 5, 2 },
		{ 6, 4, 3, 5, 7 }
	};

	private InputStatusEnum[,] INPUTSTATUS_FOR_IPHONE;

	private GUIFontCalculator[] CONTROL_FONT_CALCULATOR_FOR_IPHONE;

	private int rankValue_;

	public int[,] CONTROL_UVS_FOR_IPHONE;

	public InputStatusEnum[,] INPUTSTATUS_FOR_IPHONE;

	private enum PanelType
	{
		MINIMAP_PANEL,
		RECORD_PANEL,
		MYLAP,
		SLASH,
		TOTALLAP,
		TIMEINFO_1M,
		TIMEINFO_COLON_1,
		TIMEINFO_10S,
		TIMEINFO_1S,
		TIMEINFO_COLON_2,
		TIMEINFO_100MS,
		TIMEINFO_10MS,
		BESTINFO_1M,
		BESTINFO_COLON_1,
		BESTINFO_10S,
		BESTINFO_1S,
		BESTINFO_COLON_2,
		BESTINFO_100MS,
		BESTINFO_10MS,
		SPEED_100,
		SPEED_10,
		SPEED_1,
		SPEED_KM,
		ITEM_PANEL,
		PAUSE,
		RANK,
		PLAYER_PANEL_0,
		PLAYER_MARK_0 = 32,
		SIZE = 38
	}

	private enum TimeIndex
	{
		TIME_1M,
		TIME_10S = 2,
		TIME_1S,
		TIME_100MS = 5,
		TIME_10MS
	}
}
