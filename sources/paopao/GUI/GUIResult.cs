using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUIResult : MonoBehaviourEx
{
	private void Awake()
	{
		this.panelInfo_ = ((GUIBase.GetGUIType() != GUIType.IPAD) ? this.PANELINFO_FOR_IPHONE : this.PANELINFO_FOR_IPAD);
		this.layers_ = ((GUIBase.GetGUIType() != GUIType.IPAD) ? this.LAYERS_FOR_IPHONE : this.LAYERS_FOR_IPAD);
		this.buttonInfo_ = ((GUIBase.GetGUIType() != GUIType.IPAD) ? this.BUTTONINFO_FOR_IPHONE : this.BUTTONINFO_FOR_IPAD);
		this.RegistMonoBehaviour(7);
		this.guiManager_ = new GUIPanelManager();
		this.fb_ = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
	}

	private void Start()
	{
		this.numOfKarts = KartManager.Instance.GetGoKartCount();
		this.guiManager_.SetCamera(CameraManager.Instance.guiCam_);
		this.meshRenderer_ = base.GetComponent<MeshRenderer>();
		this.meshRenderer_.castShadows = false;
		this.meshRenderer_.receiveShadows = false;
		this.meshRenderer_.material = this.materialForGuiType_[(int)GUIBase.GetGUIType()];
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.material.mainTexture);
		int num = 0;
		float[] array = new float[] { 108f, 0f, 700f, 0f, 2f, 472f, 523f };
		array[1] = 58f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array[3] = 307f + ((this.numOfKarts <= 4) ? 0f : this.gapY);
		this.back_ = new GUIPanelEx3PartVert(num, array, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.guiManager_.RegistGUIInterface(this.back_);
		int num2 = 0;
		float[] array2 = new float[] { 111f, 0f, 697f, 0f, 506f, 206f, 597f };
		array2[1] = 61f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array2[3] = 61f + this.gapY + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		this.user_ = new GUIPanelEx3PartHorz(num2, array2, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP, 3f);
		this.guiManager_.RegistGUIInterface(this.user_);
		this.panels_ = new GUIPanelEx[63];
		Vector3[] array3 = new Vector3[]
		{
			GUIFontCalculator.DEFAULT_GAP,
			new Vector3(2f, 0f, 1f),
			GUIFontCalculator.DEFAULT_GAP,
			new Vector3(2f, 0f, 1f),
			new Vector3(0f, 2f, 2f),
			GUIFontCalculator.DEFAULT_GAP
		};
		for (int i = 0; i < this.panelInfo_.Length; i++)
		{
			if (this.numOfKarts <= 4 && i == 3)
			{
				this.panelInfo_[i][1] += this.gapY;
			}
			float[] array4 = new float[8];
			array4[0] = this.panelInfo_[i][0];
			array4[1] = this.panelInfo_[i][1];
			array4[2] = array4[0] + this.panelInfo_[i][4] - this.panelInfo_[i][2];
			array4[3] = array4[1] + this.panelInfo_[i][5] - this.panelInfo_[i][3];
			array4[4] = this.panelInfo_[i][2];
			array4[5] = this.panelInfo_[i][3];
			array4[6] = this.panelInfo_[i][4];
			array4[7] = this.panelInfo_[i][5];
			array4[0] = array4[0] * (float)Screen.width / 800f;
			array4[1] = array4[1] * (float)Screen.height / 480f;
			array4[2] = array4[2] * (float)Screen.width / 800f;
			array4[3] = array4[3] * (float)Screen.height / 480f;
			this.panels_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array4, fiaTexture, this.layers_[i], array3[this.layers_[i]]);
			this.guiManager_.RegistGUIInterface(this.panels_[i]);
		}
		int guitype = (int)GUIBase.GetGUIType();
		float[][] array5 = new float[2][];
		int num3 = 0;
		float[] array6 = new float[] { 474f, 0f, 995f, 170f, 1022f, 196f };
		array6[1] = 72f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array5[num3] = array6;
		array5[1] = new float[] { 599f, 218f, 359f, 2f, 389f, 32f };
		float[][] array7 = array5;
		Vector3[] array8 = new Vector3[]
		{
			GUIFontCalculator.Y2_GAP,
			GUIFontCalculator.X2_GAP
		};
		for (int j = 0; j < 6; j++)
		{
			int num4 = 4 + j;
			float[] array9 = new float[8];
			array9[0] = array7[guitype][0];
			array9[1] = array7[guitype][1];
			array9[2] = array9[0] + array7[guitype][4] - array7[guitype][2];
			array9[3] = array9[1] + array7[guitype][5] - array7[guitype][3];
			array9[4] = array7[guitype][2];
			array9[5] = array7[guitype][3];
			array9[6] = array7[guitype][4];
			array9[7] = array7[guitype][5];
			array9[0] = array9[0] * (float)Screen.width / 800f;
			array9[1] = array9[1] * (float)Screen.height / 480f;
			array9[2] = array9[2] * (float)Screen.width / 800f;
			array9[3] = array9[3] * (float)Screen.height / 480f;
			this.panels_[num4] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array9, fiaTexture, 2, array8[guitype]);
			array7[guitype][1] += this.gapY;
			this.panels_[num4].SetUV(j);
		}
		float[][] array10 = new float[2][];
		int num5 = 0;
		float[] array11 = new float[] { 123f, 0f, 352f, 160f, 406f, 204f };
		array11[1] = 109f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array10[num5] = array11;
		array10[1] = new float[] { 221f, 273f, 410f, 145f, 510f, 219f };
		float[][] array12 = array10;
		Vector3[] array13 = new Vector3[]
		{
			GUIFontCalculator.X2_GAP,
			GUIFontCalculator.Y2_GAP
		};
		for (int k = 0; k < 5; k++)
		{
			int num6 = 10 + k;
			float[] array14 = new float[8];
			array14[0] = array12[guitype][0];
			array14[1] = array12[guitype][1];
			array14[2] = array14[0] + array12[guitype][4] - array12[guitype][2];
			array14[3] = array14[1] + array12[guitype][5] - array12[guitype][3];
			array14[4] = array12[guitype][2];
			array14[5] = array12[guitype][3];
			array14[6] = array12[guitype][4];
			array14[7] = array12[guitype][5];
			array14[0] = array14[0] * (float)Screen.width / 800f;
			array14[1] = array14[1] * (float)Screen.height / 480f;
			array14[2] = array14[2] * (float)Screen.width / 800f;
			array14[3] = array14[3] * (float)Screen.height / 480f;
			this.panels_[num6] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array14, fiaTexture, 2, array13[guitype]);
			array12[guitype][1] += this.gapY;
			this.panels_[num6].SetUV(k);
		}
		float[][] array15 = new float[2][];
		int num7 = 0;
		float[] array16 = new float[] { 544f, 0f, 364f, 122f, 457f, 152f };
		array16[1] = 70f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array15[num7] = array16;
		array15[1] = new float[] { 645f, 219f, 305f, 58f, 397f, 87f };
		float[][] array17 = array15;
		for (int l = 0; l < 6; l++)
		{
			float[] array18 = new float[8];
			array18[0] = array17[guitype][0];
			array18[1] = array17[guitype][1];
			array18[2] = array18[0] + array17[guitype][4] - array17[guitype][2];
			array18[3] = array18[1] + array17[guitype][5] - array17[guitype][3];
			array18[4] = array17[guitype][2];
			array18[5] = array17[guitype][3];
			array18[6] = array17[guitype][4];
			array18[7] = array17[guitype][5];
			array18[0] = array18[0] * (float)Screen.width / 800f;
			array18[1] = array18[1] * (float)Screen.height / 480f;
			array18[2] = array18[2] * (float)Screen.width / 800f;
			array18[3] = array18[3] * (float)Screen.height / 480f;
			this.panels_[15 + l] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array18, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
			array17[guitype][1] += this.gapY;
		}
		float[][] array19 = new float[2][];
		int num8 = 0;
		float[] array20 = new float[] { 531f, 0f, 166f, 104f, 182f, 130f };
		array20[1] = 72f + ((this.numOfKarts <= 4) ? this.gapY : 0f);
		array19[num8] = array20;
		array19[1] = new float[] { 645f, 219f, 41f, 166f, 61f, 196f };
		float[][] array21 = array19;
		float num9 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 20f : 18f);
		float num10 = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 20f : 12f);
		float num11 = array21[guitype][0];
		for (int m = 0; m < 6; m++)
		{
			array21[guitype][0] = num11;
			for (int n = 0; n < 7; n++)
			{
				int num12 = 21 + m * 7 + n;
				float[] array22 = new float[8];
				array22[0] = array21[guitype][0];
				array22[1] = array21[guitype][1];
				array22[2] = array22[0] + array21[guitype][4] - array21[guitype][2];
				array22[3] = array22[1] + array21[guitype][5] - array21[guitype][3];
				array22[4] = array21[guitype][2];
				array22[5] = array21[guitype][3];
				array22[6] = array21[guitype][4];
				array22[7] = array21[guitype][5];
				array22[0] = array22[0] * (float)Screen.width / 800f;
				array22[1] = array22[1] * (float)Screen.height / 480f;
				array22[2] = array22[2] * (float)Screen.width / 800f;
				array22[3] = array22[3] * (float)Screen.height / 480f;
				this.panels_[num12] = GUIPanelFactory.Instance.CreateByWindowSpace(0, array22, fiaTexture, 1, array3[1]);
				array21[guitype][0] += ((n != 1 && n != 4) ? num9 : num10);
			}
			array21[guitype][1] += this.gapY;
		}
		for (int num13 = 4; num13 <= 62; num13++)
		{
			this.guiManager_.RegistGUIInterface(this.panels_[num13]);
		}
		if (KartManager.Instance.parameter_.Stage == StageType.GAME)
		{
			this.buttons_ = new GUIButton[this.buttonInfo_.Length];
			for (int num14 = 0; num14 < this.buttonInfo_.Length; num14++)
			{
				this.buttonInfo_[num14][1] += ((this.numOfKarts <= 4) ? (-this.gapY) : 0f);
				this.buttonInfo_[num14][3] += ((this.numOfKarts <= 4) ? (-this.gapY) : 0f);
				this.buttons_[num14] = new GUIButton(0, this.buttonInfo_[num14], fiaTexture, 3, new Vector3(129f, 0f, 1f));
				this.guiManager_.RegistGUIInterface(this.buttons_[num14]);
				this.mouseManager_.Insert(10 + num14, this.buttons_[num14]);
			}
		}
		Vector2[] array23 = new Vector2[]
		{
			new Vector2(204f * (float)Screen.width / 800f, (float)Screen.height - (71f + ((this.numOfKarts <= 4) ? this.gapY : 0f) + 5f) * (float)Screen.height / 480f),
			new Vector2(456f, 548f)
		};
		base.guiText.font = this.fontForGuiType_[1];
		base.guiText.pixelOffset = array23[(int)GUIBase.GetGUIType()];
		base.guiText.lineSpacing = ((GUIBase.GetGUIType() != GUIType.IPHONE) ? 2.05f : 1.85f) * (float)Screen.height / 480f;
		base.guiText.alignment = TextAlignment.Left;
		base.guiText.anchor = TextAnchor.UpperLeft;
		base.gameObject.layer = LayerMask.NameToLayer("Gui");
		for (int num15 = 4; num15 <= 62; num15++)
		{
			this.panels_[num15].Visible = false;
		}
		this.panels_[0].Visible = false;
		this.panels_[1].Visible = false;
		this.panels_[2].Visible = false;
		base.gameObject.SetActiveRecursively(false);
		this.mesh_ = base.GetComponent<MeshFilter>().mesh;
	}

	private void FixedUpdate()
	{
	}

	private void Update()
	{
		if (this.updating_ != 0)
		{
			this.mouseManager_.InitSelectionData();
		}
		else
		{
			this.mouseManager_.Update();
		}
		if (this.mouseManager_.IsSelected())
		{
			int selectedKey = this.mouseManager_.GetSelectedKey();
			if (StageController.IsInstantiated())
			{
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
			if (selectedKey == 10)
			{
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.RESTART));
			}
			else if (selectedKey == 11)
			{
				base.SendMessage(1, ((MonoBehaviourMessage1Param<GameStageCommand>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.GAMESTAGE_COMMAND)).Initialize(GameStageCommand.QUIT));
			}
			else if (selectedKey == 0)
			{
				if (this.fb_.LoggedIn)
				{
					this.Publish();
				}
				else
				{
					this.updating_ |= 1;
					FiaCoroutine fiaCoroutine = new FiaCoroutine(this.fb_.Login(), new OnSuccess(this.LoginSuccess), new OnFailure(this.LoginFailure));
					base.StartCoroutine(fiaCoroutine);
				}
			}
		}
		int pushedKey = this.mouseManager_.GetPushedKey();
		bool flag = false;
		if (this.buttons_ != null)
		{
			for (int i = 0; i < this.buttons_.Length; i++)
			{
				flag |= this.buttons_[i].SetPushed(pushedKey == 10 + i);
			}
		}
		int num = 0;
		if (this.panels_[num].Visible)
		{
			flag |= this.panels_[num].SetUV((pushedKey != num) ? 0 : 1);
		}
		if (flag)
		{
			this.guiManager_.Update();
			this.mesh_.Clear();
			this.guiManager_.UpdateMesh(ref this.mesh_);
		}
	}

	public override void ReceiveMessage(int sender, MonoBehaviourMessage msg)
	{
		base.ReceiveMessage(sender, msg);
		if (msg.type_ == MonoBehaviourMessageType.SHOW_RESULT)
		{
			MonoBehaviourMessage2Param<bool, bool> monoBehaviourMessage2Param = (MonoBehaviourMessage2Param<bool, bool>)msg;
			base.gameObject.SetActiveRecursively(monoBehaviourMessage2Param.lparam_);
			if (monoBehaviourMessage2Param.lparam_)
			{
				this.RankMarkSetting();
				this.PlayerNameSetting();
				this.RankOrderSetting();
				this.PlayerBarSetting();
				this.TimeSetting();
				this.FacebookSetting();
				this.UpdateForNewRecord();
				this.guiManager_.Update();
				Mesh mesh = base.GetComponent<MeshFilter>().mesh;
				mesh.Clear();
				this.guiManager_.UpdateMesh(ref mesh);
			}
		}
	}

	private void RecordPostSuccess()
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Record Post Success");
		}
	}

	private void RecordPostFailure(Exception ex)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Record Post Failure");
			Debug.Log(ex);
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
		this.UpdateUVOfCharIdx(10, idx + 1);
		this.UpdateUVOfCharIdx(num % 60 / 10, idx + 2);
		this.UpdateUVOfCharIdx(num % 10, idx + 3);
		this.UpdateUVOfCharIdx(10, idx + 4);
		int num2 = (int)(t * 100f) % 100;
		this.UpdateUVOfCharIdx(num2 / 10, idx + 5);
		this.UpdateUVOfCharIdx(num2 % 10, idx + 6);
	}

	private void TimeSetting()
	{
		RaceResult result_ = KartManager.Instance.result_;
		int num = 21;
		for (int i = 0; i < result_.elems_.Length; i++)
		{
			if (result_.elems_[i].IsRetire())
			{
				this.panels_[15 + i].Visible = true;
			}
			else
			{
				for (int j = 0; j < 7; j++)
				{
					this.panels_[num + j].Visible = true;
				}
				this.UpdateUVOfTime(result_.elems_[i].raceTime_, num);
			}
			num += 7;
		}
	}

	private void PlayerBarSetting()
	{
		float[][] array = new float[][]
		{
			new float[]
			{
				111f,
				61f + ((this.numOfKarts <= 4) ? this.gapY : 0f),
				this.gapY
			},
			new float[] { 301f, 209f, 76f }
		};
		int guitype = (int)GUIBase.GetGUIType();
		RaceResult result_ = KartManager.Instance.result_;
		for (int i = 0; i < result_.elems_.Length; i++)
		{
			if (result_.elems_[i].kartIndex_ == KartManager.PLAYER_KART_IDX)
			{
				this.user_.SetRectByWindowSpace(array[guitype][0] * (float)Screen.width / 800f, (array[guitype][1] + (float)i * array[guitype][2]) * (float)Screen.height / 480f);
				return;
			}
		}
	}

	private void RankOrderSetting()
	{
		RaceResult result_ = KartManager.Instance.result_;
		for (int i = 0; i < result_.elems_.Length - 1; i++)
		{
			this.panels_[10 + i].Visible = true;
		}
	}

	private void PlayerNameSetting()
	{
		RaceResult result_ = KartManager.Instance.result_;
		string text = string.Empty;
		for (int i = 0; i < result_.elems_.Length; i++)
		{
			text += result_.elems_[i].name_;
			text += "\n";
		}
		base.guiText.text = text;
	}

	private void RankMarkSetting()
	{
		float[][] array = new float[][]
		{
			new float[]
			{
				474f,
				72f + ((this.numOfKarts <= 4) ? this.gapY : 0f),
				this.gapY
			},
			new float[] { 599f, 218f, 76f }
		};
		int guitype = (int)GUIBase.GetGUIType();
		RaceResult result_ = KartManager.Instance.result_;
		for (int i = 0; i < result_.elems_.Length; i++)
		{
			int num = 4 + result_.elems_[i].kartIndex_;
			this.panels_[num].Visible = true;
			this.panels_[num].SetRectByWindowSpace(array[guitype][0] * (float)Screen.width / 800f, (array[guitype][1] + (float)i * array[guitype][2]) * (float)Screen.height / 480f);
		}
	}

	private void FacebookSetting()
	{
		if (KartManager.Instance.parameter_.Stage != StageType.GAME)
		{
			return;
		}
		RaceResult result_ = KartManager.Instance.result_;
		if (result_ == null || (!result_.IsMonthlyBestRecord && !result_.IsBestRecord))
		{
			return;
		}
		int num = -1;
		RaceResult result_2 = KartManager.Instance.result_;
		for (int i = 0; i < result_2.elems_.Length; i++)
		{
			if (result_2.elems_[i].kartIndex_ == KartManager.PLAYER_KART_IDX)
			{
				num = i;
				break;
			}
		}
		int guitype = (int)GUIBase.GetGUIType();
		int num2 = 0;
		float[][] array = new float[][]
		{
			new float[]
			{
				708f,
				53f + ((this.numOfKarts <= 4) ? this.gapY : 0f),
				this.gapY
			},
			new float[] { 414f, 61f, 40f }
		};
		this.panels_[num2].Visible = true;
		float num3 = array[guitype][0];
		float num4 = array[guitype][1] + (float)num * array[guitype][2];
		float num5 = array[guitype][0];
		float num6 = array[guitype][1];
		float num7 = array[guitype][2];
		this.panels_[num2].SetRectByWindowSpace(num3 * ((float)Screen.width / 800f), num4 * (float)Screen.height / 480f);
		this.mouseManager_.Insert(num2, this.panels_[num2]);
	}

	private void UpdateForNewRecord()
	{
		if (KartManager.Instance.result_.IsBestRecord || KartManager.Instance.result_.IsMonthlyBestRecord)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("This is a new record.");
			}
			int guitype = (int)GUIBase.GetGUIType();
			int num = -1;
			RaceResult raceResult = KartManager.Instance.result_;
			for (int i = 0; i < raceResult.elems_.Length; i++)
			{
				if (raceResult.elems_[i].kartIndex_ == KartManager.PLAYER_KART_IDX)
				{
					num = i;
					break;
				}
			}
			float[][] array = new float[][]
			{
				new float[]
				{
					14f,
					51f + ((this.numOfKarts <= 4) ? this.gapY : 0f),
					this.gapY
				},
				new float[] { 328f, 60f, 40f }
			};
			float[][] array2 = new float[][]
			{
				new float[]
				{
					14f,
					51f + ((this.numOfKarts <= 4) ? this.gapY : 0f),
					this.gapY
				},
				new float[] { 328f, 60f, 40f }
			};
			int num2 = 1;
			int num3 = 2;
			this.panels_[num2].Visible = KartManager.Instance.result_.IsBestRecord;
			this.panels_[num3].Visible = KartManager.Instance.result_.IsMonthlyBestRecord;
			this.panels_[num2].SetRectByWindowSpace(array[guitype][0] * (float)Screen.width / 800f, (array[guitype][1] + (float)num * array[guitype][2]) * (float)Screen.height / 480f);
			this.panels_[num3].SetRectByWindowSpace(array2[guitype][0] * (float)Screen.width / 800f, (array2[guitype][1] + (float)num * array2[guitype][2]) * (float)Screen.height / 480f);
			if (NativeHelper.buildType == "SKT")
			{
				raceResult = KartManager.Instance.result_;
				float num4 = 0f;
				foreach (RaceResultElem raceResultElem in raceResult.elems_)
				{
					if (raceResultElem.kartIndex_ == KartManager.PLAYER_KART_IDX)
					{
						num4 = raceResultElem.raceTime_;
						break;
					}
				}
				string text = string.Format("{0:0.00}", num4);
				if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_SPEED)
				{
					string name = AssetDefinitionManager.GetAssetDefinition(AssetType.TRACK, (int)KartManager.Instance.parameter_.track_).Name;
					string text2 = string.Format("{0}/{1}", name, text);
					using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
					{
						androidJavaClass.CallStatic<int>("setGamecenterPoint", new object[] { text2 });
					}
				}
			}
			return;
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log("Not a new record.");
		}
	}

	private void PublishCallback(Facebook fb)
	{
		if (fb.State == StartableState.WAITING)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("PublishCallback / publish success");
			}
			KartOptions.Instance.SetQuestFlag(KartOptions.QuestFlag.FACEBOOK_PUBLISHING, true);
			KartOptions.Instance.SaveRegistry();
			this.panels_[0].Visible = false;
			this.guiManager_.Update();
			this.guiManager_.UpdateMesh(ref this.mesh_);
		}
		else if (Debug.isDebugBuild)
		{
			Debug.Log("PublishCallback / publish failed, canceled");
		}
		this.updating_ &= -3;
	}

	private void LoginSuccess()
	{
		this.updating_ &= -2;
		this.Publish();
		PlayerPrefs.SetInt("UPDATE_RANKING", 1);
	}

	private void LoginFailure(Exception ex)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log(ex);
		}
		this.updating_ &= -2;
		Type type = ex.GetType();
		if (type == typeof(FacebookRequestException))
		{
			iOSEvent.Alert("페이스북에 로그인 할 수 없습니다.");
		}
		else if (type == typeof(RequestException))
		{
			iOSEvent.Alert("서버에 연결할 수 없습니다. 다시 시도해주세요.");
		}
		else if (type == typeof(ServerException))
		{
			iOSEvent.Alert(ex.Message);
		}
		else if (type == typeof(FacebookFQLException))
		{
			iOSEvent.Alert("페이스북에 로그인 할 수 없습니다.");
		}
		else if (type != typeof(FacebookCanceledException))
		{
			if (type == typeof(FiaAuthException))
			{
				iOSEvent.Alert("페이스북에 로그인 할 수 없습니다.");
			}
			else if (Debug.isDebugBuild)
			{
				Debug.Log("UNEXPECTED ERROR! " + ex);
			}
		}
	}

	private void Publish()
	{
		if (!this.fb_.LoggedIn)
		{
			iOSEvent.Alert("기록을 올리려면 페이스북에 로그인해야 합니다.");
			return;
		}
		this.updating_ |= 2;
		string text = "{0}이/가 {2} 트랙에서 {1}의 신기록을 세웠습니다!";
		if (KartManager.Instance.result_.IsMonthlyBestRecord)
		{
			text = "{0}이/가 {2} 트랙에서 {1}의 이번 달 신기록을 세웠습니다!";
		}
		RaceResult result_ = KartManager.Instance.result_;
		float num = 0f;
		foreach (RaceResultElem raceResultElem in result_.elems_)
		{
			if (raceResultElem.kartIndex_ == KartManager.PLAYER_KART_IDX)
			{
				num = raceResultElem.raceTime_;
				break;
			}
		}
		int num2 = (int)num / 60;
		int num3 = (int)num % 60;
		int num4 = (int)((num - (float)((int)num)) * 100f);
		string text2 = string.Format("{0}:{1:00}:{2:00}", num2, num3, num4);
		string name = AssetDefinitionManager.GetAssetDefinition(AssetType.TRACK, (int)KartManager.Instance.parameter_.track_).Name;
		text = string.Format(text, this.fb_.UserName, text2, name);
		string empty = string.Empty;
		string text3 = "{0}와/과 함께 안드로이드에서 넥슨의 레이싱 게임 카트라이더 러쉬를 즐겨보세요.";
		text3 = string.Format(text3, this.fb_.UserName);
		string text4 = "http://www.facebook.com/KartRiderRush";
		string text5 = "http://s.kartriderrush.com/resources/facebook_new_record.png";
		base.StartCoroutine(this.fb_.Publish(text, empty, text3, text4, text5, new FacebookDelegate(this.PublishCallback)));
	}

	private MeshRenderer meshRenderer_;

	private float[][] panelInfo_;

	private float[][] PANELINFO_FOR_IPAD = new float[][]
	{
		new float[] { 440f, 136f, 385f, 162f, 423f, 202f },
		new float[] { 14f, 64f, 333f, 162f, 383f, 202f },
		new float[] { 56f, 71f, 321f, 283f, 409f, 331f },
		new float[] { 221f, 162f, 410f, 34f, 510f, 143f }
	};

	private float[][] PANELINFO_FOR_IPHONE = new float[][]
	{
		new float[] { 708f, 53f, 375f, 295f, 459f, 360f },
		new float[] { 14f, 51f, 586f, 78f, 673f, 136f },
		new float[] { 14f, 51f, 675f, 78f, 763f, 136f },
		new float[] { 119f, 39f, 522f, 88f, 584f, 158f }
	};

	private int[] LAYERS_FOR_IPHONE = new int[] { 3, 1, 1, 2 };

	private int[] LAYERS_FOR_IPAD = new int[] { 3, 1, 1, 2 };

	private int[] layers_;

	private GUIPanelEx3PartVert back_;

	private GUIPanelEx3PartHorz user_;

	private float[][] BUTTONINFO_FOR_IPAD = new float[][]
	{
		new float[]
		{
			110f, 242f, 240f, 282f, 2f, 470f, 87f, 2f, 346f, 132f,
			364f
		},
		new float[]
		{
			250f, 242f, 380f, 282f, 2f, 470f, 87f, 2f, 444f, 154f,
			468f
		}
	};

	private float[][] BUTTONINFO_FOR_IPHONE = new float[][]
	{
		new float[]
		{
			148f, 358f, 396f, 432f, 766f, 854f, 893f, 2f, 826f, 250f,
			854f
		},
		new float[]
		{
			432f, 358f, 680f, 432f, 766f, 854f, 893f, 2f, 796f, 250f,
			824f
		}
	};

	private float[][] buttonInfo_;

	private GUIPanelManager guiManager_;

	private GUIPanelEx[] panels_;

	private GUIButton[] buttons_;

	public Material[] materialForGuiType_;

	public Font[] fontForGuiType_;

	private MouseManager mouseManager_ = new MouseManager();

	private Mesh mesh_;

	private Facebook fb_;

	private int numOfKarts;

	private int updating_;

	private float gapY = (float)((GUIBase.GetGUIType() != GUIType.IPHONE) ? 76 : 48);

	private enum TimeInfo
	{
		_1M,
		_COLON_1,
		_10S,
		_1S,
		_COLON_2,
		_100MS,
		_10MS,
		SIZE
	}

	private enum PanelType
	{
		FACEBOOK,
		NEW_RECORD_MARK,
		NEW_MONTHLY_RECORD_MARK,
		PLAYER_RANK_0,
		PLAYER_MARK_0,
		PLAYER_RANK_1 = 10,
		RETIRE_0 = 15,
		TIMEINFO_0 = 21,
		SIZE = 63,
		INVISIBLE_BEGIN = 4,
		INVISIBLE_END = 62,
		MANUAL_REGIST_BEGIN = 4,
		MANUAL_REGIST_END = 62
	}

	private enum UpdatingFlag
	{
		LOG_IN = 1,
		PUBLISH
	}
}
