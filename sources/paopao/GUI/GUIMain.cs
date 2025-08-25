using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GUIMain : FiaGUILayer
{
	public override void DoInit()
	{
		StageController.Instance.BeginStage();
		FiaTexture fiaTexture = new FiaTexture(this.meshRenderer_.materials[0].mainTexture);
		FiaTexture fiaTexture2 = new FiaTexture(this.meshRenderer_.materials[1].mainTexture);
		FiaTexture fiaTexture3 = new FiaTexture(this.meshRenderer_.materials[2].mainTexture);
		this.mouseManager_ = new MouseManager();
		GUIPanelFactory instance = GUIPanelFactory.Instance;
		int num = 0;
		float[] array = new float[] { 0f, 0f, 0f, 0f, 2f, 612f, 802f, 1092f };
		array[2] = (float)Screen.width;
		array[3] = (float)Screen.height;
		this.back_ = instance.CreateByWindowSpace(num, array, fiaTexture, 5, GUIFontCalculator.DEFAULT_GAP);
		this.back_.RegistPanelManager(this.panelManager_);
		for (int i = 0; i < 5; i++)
		{
			this.buttons_[i] = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[]
			{
				(float)(12 + i * 156) * (float)Screen.width / 800f,
				198f * (float)Screen.height / 480f,
				(float)(164 + i * 156) * (float)Screen.width / 800f,
				472f * (float)Screen.height / 480f,
				(float)(2 + this.DEFAULT_BUTTON_IDX[i] * 154),
				2f,
				(float)((1 + this.DEFAULT_BUTTON_IDX[i]) * 154),
				276f
			}, fiaTexture, 3, GUIFontCalculator.Y2_GAP);
			this.buttons_[i].SetUV(0);
			this.buttons_[i].RegistPanelManager(this.panelManager_);
		}
		if (NativeHelper.buildType == "SKT")
		{
			GUIPanelEx[] array2 = this.buttons_;
			int num2 = 5;
			GUIPanelFactory instance2 = GUIPanelFactory.Instance;
			int num3 = 0;
			float[] array3 = new float[] { 0f, 0f, 0f, 0f, 0f, 552f, 156f, 610f };
			array3[0] = 70f * (float)Screen.width / 800f;
			array3[1] = 7f * (float)Screen.height / 480f;
			array3[2] = 226f * (float)Screen.width / 800f;
			array3[3] = 65f * (float)Screen.height / 480f;
			array2[num2] = instance2.CreateByWindowSpace(num3, array3, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		}
		else
		{
			GUIPanelEx[] array4 = this.buttons_;
			int num4 = 5;
			GUIPanelFactory instance3 = GUIPanelFactory.Instance;
			int num5 = 0;
			float[] array5 = new float[] { 0f, 0f, 0f, 0f, 0f, 552f, 156f, 610f };
			array5[0] = 9f * (float)Screen.width / 800f;
			array5[1] = 7f * (float)Screen.height / 480f;
			array5[2] = 165f * (float)Screen.width / 800f;
			array5[3] = 65f * (float)Screen.height / 480f;
			array4[num4] = instance3.CreateByWindowSpace(num5, array5, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		}
		this.buttons_[5].RegistPanelManager(this.panelManager_);
		this.buttons_[5].SetUV(0);
		GUIPanelEx[] array6 = this.buttons_;
		int num6 = 6;
		GUIPanelFactory instance4 = GUIPanelFactory.Instance;
		int num7 = 0;
		float[] array7 = new float[] { 0f, 0f, 0f, 0f, 632f, 552f, 684f, 610f };
		array7[0] = 740f * (float)Screen.width / 800f;
		array7[1] = 7f * (float)Screen.height / 480f;
		array7[2] = 792f * (float)Screen.width / 800f;
		array7[3] = 65f * (float)Screen.height / 480f;
		array6[num6] = instance4.CreateByWindowSpace(num7, array7, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		this.buttons_[6].RegistPanelManager(this.panelManager_);
		this.buttons_[6].SetUV(0);
		this.buttons_[7] = GUIPanelFactory.Instance.CreateByWindowSpace(0, new float[] { 10f, 8f, 385f, 1222f, 443f, 1279f }, fiaTexture, 3, GUIFontCalculator.X2_GAP);
		if (NativeHelper.buildType == "SKT")
		{
			this.buttons_[7].RegistPanelManager(this.panelManager_);
			this.buttons_[7].SetUV(0);
		}
		for (int j = 0; j < 8; j++)
		{
			this.mouseManager_.Insert(j, this.buttons_[j]);
		}
		GUIKartViewer.Instance.Hide();
		this.guiName_ = new GUIString(new Vector2((float)(Screen.width / 2), 18f), 1, 10, GUIString.Alignment.CENTER, 1, fiaTexture2);
		this.guiName_.SubMeshIndex = 1;
		this.panelManager_.RegistGUIInterface(this.guiName_);
		this.mouseManager_.Insert(this.MOUSE_NOTIFIER_ID_USER_NAME, new DefaultNameMouseNotifier(new Rect((float)((Screen.width - 192) / 2), 0f, 192f, 58f)));
		GUIPanelFactory instance5 = GUIPanelFactory.Instance;
		int num8 = 0;
		float[] array8 = new float[] { 0f, 0f, 0f, 0f, 2f, 1094f, 802f, 1216f };
		array8[0] = 0f * (float)Screen.width / 800f;
		array8[1] = 68f * (float)Screen.height / 480f;
		array8[2] = 800f * (float)Screen.width / 800f;
		array8[3] = 122f * (float)Screen.height / 480f;
		this.enterNameDesc_ = instance5.CreateByWindowSpace(num8, array8, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.enterNameDesc_);
		this.enterNameDesc_.Visible = false;
		GUIPanelFactory instance6 = GUIPanelFactory.Instance;
		int num9 = 0;
		float[] array9 = new float[] { 0f, 0f, 0f, 0f, 2f, 1218f, 358f, 1286f };
		array9[0] = 223f * (float)Screen.width / 800f;
		array9[1] = 0f * (float)Screen.height / 480f;
		array9[2] = 579f * (float)Screen.width / 800f;
		array9[3] = 68f * (float)Screen.height / 480f;
		this.enterNameBack_ = instance6.CreateByWindowSpace(num9, array9, fiaTexture, 2, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.enterNameBack_);
		this.enterNameBack_.Visible = false;
		GUIPanelFactory instance7 = GUIPanelFactory.Instance;
		int num10 = 0;
		float[] array10 = new float[] { 0f, 0f, 0f, 0f, 360f, 1218f, 363f, 1247f };
		array10[0] = 232f * (float)Screen.width / 800f;
		array10[1] = 20f * (float)Screen.height / 480f;
		array10[2] = 235f * (float)Screen.width / 800f;
		array10[3] = 49f * (float)Screen.height / 480f;
		this.enterNameCursor_ = instance7.CreateByWindowSpace(num10, array10, fiaTexture, 1, GUIFontCalculator.DEFAULT_GAP);
		this.panelManager_.RegistGUIInterface(this.enterNameCursor_);
		this.enterNameCursor_.Visible = false;
		if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
		{
			this.fb_ = Facebook.Inst;
		}
		else
		{
			this.fb_ = MockFacebook.Inst;
		}
		if (!KartOptions.Instance.IsDefaultUserNameSet())
		{
			if (Env.IsDesktop)
			{
				KartOptions.Instance.DefaultUserName = this.DEFAULT_USER_NAME;
				KartOptions.Instance.SaveRegistry();
			}
			else
			{
				this.EditUserName();
			}
		}
		else
		{
			this.guiName_.SetString(KartOptions.Instance.LastUserName);
			this.guiName_.SetColor(Color.black);
			this.nextPopupState_ = GUIMain.NextPopupState.PATCH_SUMMARY;
		}
		this.buttons_[5].SetUV((!this.fb_.LoggedIn) ? 0 : 2);
		for (int k = 0; k < this.alerts_.Length; k++)
		{
			GUIPanelEx[] array11 = this.alerts_;
			int num11 = k;
			GUIPanelFactory instance8 = GUIPanelFactory.Instance;
			int num12 = 0;
			float[] array12 = new float[] { 0f, 195f, 0f, 0f, 2f, 2f, 34f, 34f };
			array12[0] = (float)this.ALERT_X_POS[k] * (float)Screen.width / 800f;
			array12[1] = array12[1] * (float)Screen.height / 480f;
			array12[2] = array12[0] + 32f * (float)Screen.width / 800f;
			array12[3] = array12[1] + 32f * (float)Screen.height / 480f;
			array11[num11] = instance8.CreateByWindowSpace(num12, array12, fiaTexture3, 1, new Vector3(2f, 2f, 17f));
			this.alerts_[k].SubMeshIndex = 2;
			this.panelManager_.RegistGUIInterface(this.alerts_[k]);
			this.alerts_[k].Visible = false;
		}
		this.RefreshAlerts();
	}

	protected void RefreshAlerts()
	{
		AlertState alertState = AlertStateFactory.Instance.GetAlertState(AlertStateType.TRACK);
		AlertState alertState2 = AlertStateFactory.Instance.GetAlertState(AlertStateType.KART);
		AlertState alertState3 = AlertStateFactory.Instance.GetAlertState(AlertStateType.CHARACTER);
		AlertState alertState4 = AlertStateFactory.Instance.GetAlertState(AlertStateType.BUNDLE);
		alertState.Refresh();
		alertState2.Refresh();
		alertState3.Refresh();
		alertState4.Refresh();
		int num = 0;
		foreach (AssetDefinition assetDefinition in TrackAssetDefinitionManager.Instance.GetAssetDefinitionList())
		{
			if ((!assetDefinition.Lock || assetDefinition.LockType != AssetDefinition.enLockType.CASH) && alertState.DisplayAlert(assetDefinition.Id))
			{
				num++;
			}
		}
		if (num > 0)
		{
			this.alerts_[0].SetUV(num - 1);
			this.alerts_[0].Visible = true;
			this.alerts_[1].SetUV(num - 1);
			this.alerts_[1].Visible = true;
		}
		else
		{
			this.alerts_[0].Visible = false;
			this.alerts_[1].Visible = false;
		}
		this.alerts_[2].Visible = false;
		int num2 = 0;
		foreach (AssetDefinition assetDefinition2 in KartAssetDefinitionManager.Instance.GetAssetDefinitionList())
		{
			if ((!assetDefinition2.Lock || assetDefinition2.LockType != AssetDefinition.enLockType.CASH) && alertState2.DisplayAlert(assetDefinition2.Id))
			{
				num2++;
			}
		}
		foreach (AssetDefinition assetDefinition3 in CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList())
		{
			if ((!assetDefinition3.Lock || assetDefinition3.LockType != AssetDefinition.enLockType.CASH) && alertState3.DisplayAlert(assetDefinition3.Id))
			{
				num2++;
			}
		}
		if (num2 > 0)
		{
			this.alerts_[3].SetUV(num2 - 1);
			this.alerts_[3].Visible = true;
		}
		else
		{
			this.alerts_[3].Visible = false;
		}
		int num3 = alertState4.AlertCount();
		if (num3 > 0)
		{
			this.alerts_[4].Visible = true;
			this.alerts_[4].SetUV(num3 - 1);
		}
		else
		{
			this.alerts_[4].Visible = false;
		}
	}

	protected override void FirstUpdate()
	{
		base.FirstUpdate();
		GUIAccomplishPopup.OpenStaticPopup();
	}

	protected override void BeforePanelUpdate()
	{
		this.mouseManager_.Update();
		if (this.updating_ == 0)
		{
			int pushedKey = this.mouseManager_.GetPushedKey();
			for (int i = 0; i < 8; i++)
			{
				if (i == 5)
				{
					int num = ((!this.fb_.LoggedIn) ? 0 : 2);
					this.buttons_[i].SetUV((pushedKey != i) ? num : (num + 1));
				}
				else
				{
					this.buttons_[i].SetUV((pushedKey != i) ? 0 : 1);
				}
			}
		}
		else if ((this.updating_ & 2) != 0)
		{
			string text = this.keyboard_.text;
			if (Application.platform == RuntimePlatform.Android)
			{
				if (this.prevKeyboardText_ != null)
				{
					if (this.prevKeyboardText_.Length == text.Length)
					{
						text = this.defaultName_;
					}
					else if (this.prevKeyboardText_.Length < this.keyboard_.text.Length)
					{
						string text2 = null;
						for (int j = 0; j < this.prevKeyboardText_.Length; j++)
						{
							if (this.prevKeyboardText_[j] != this.keyboard_.text[j])
							{
								text2 = string.Empty + this.keyboard_.text[j];
								break;
							}
						}
						if (text2 == null)
						{
							text2 = string.Empty + this.keyboard_.text[this.keyboard_.text.Length - 1];
						}
						text = this.defaultName_ + text2;
					}
					else if (this.prevKeyboardText_.Length > this.keyboard_.text.Length)
					{
						if (this.defaultName_.Length > 0)
						{
							text = this.defaultName_.Substring(0, this.defaultName_.Length - 1);
						}
						else
						{
							text = this.defaultName_;
						}
					}
				}
				this.prevKeyboardText_ = this.keyboard_.text;
			}
			this.defaultName_ = FiaUtil.GenerateUserNameEx(text, this.defaultName_);
			text = this.defaultName_;
			if (Application.platform != RuntimePlatform.Android)
			{
				this.keyboard_.text = text;
			}
			if (this.defaultName_ != string.Empty)
			{
				this.guiName_.SetString(this.defaultName_);
				this.guiName_.SetColor(Color.black);
				this.enterNameCursor_.SetRectByWindowSpace(this.guiName_.Rect.xMax, (float)Screen.height - this.guiName_.Rect.yMin);
			}
			else
			{
				this.guiName_.SetString(this.DEFAULT_USER_NAME);
				this.guiName_.SetColor(Color.grey);
				this.enterNameCursor_.SetRectByWindowSpace(this.guiName_.Rect.xMin - 1f, (float)Screen.height - this.guiName_.Rect.yMin);
			}
			if (this.keyboard_.done)
			{
				this.updating_ &= -3;
				KartOptions.Instance.DefaultUserName = ((!(this.defaultName_ == string.Empty)) ? this.defaultName_ : this.DEFAULT_USER_NAME);
				KartOptions.Instance.SaveRegistry();
				this.guiName_.SetString(KartOptions.Instance.DefaultUserName);
				this.guiName_.SetColor(Color.black);
				base.CancelInvoke();
				this.enterNameDesc_.Visible = false;
				this.enterNameBack_.Visible = false;
				this.enterNameCursor_.Visible = false;
				this.nextPopupState_ = GUIMain.NextPopupState.PATCH_SUMMARY;
			}
		}
	}

	protected override void AfterPanelUpdate()
	{
		if (this.updating_ == 0)
		{
			int selectedKey = this.mouseManager_.GetSelectedKey();
			if (MathHelper.IsBetweenII(selectedKey, 0, 6) && this.NEXT_STAGE[selectedKey] != StageType.NONE)
			{
				GUIMain.ButtonType buttonType = (GUIMain.ButtonType)selectedKey;
				if (buttonType != GUIMain.ButtonType.Item)
				{
					if (buttonType == GUIMain.ButtonType.Speed)
					{
						KartManager.Instance.parameter_.gameMode_ = GameMode.SINGLE_SPEED;
					}
				}
				else
				{
					KartManager.Instance.parameter_.gameMode_ = GameMode.SINGLE_ITEM;
				}
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
				if (NativeHelper.buildType == "admob")
				{
					string text = string.Empty;
					if (selectedKey == 2)
					{
						text = "admob_multiplay";
						using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
						{
							androidJavaClass.CallStatic<int>("unityAlert", new object[] { text });
						}
						return;
					}
					if (selectedKey == 4)
					{
						text = "admob_store";
						using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
						{
							androidJavaClass2.CallStatic<int>("unityAlert", new object[] { text });
						}
						return;
					}
				}
				if (selectedKey == 2 && !KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.TUTORIAL_MULTI))
				{
					MonoBehaviourMessageType monoBehaviourMessageType = MonoBehaviourMessageType.SHOW_TUTORIAL_MULTI;
					base.BroadcastMessage(new MonoBehaviourMessage(monoBehaviourMessageType));
					MonoBehaviourExCenter.Instance.SendMessage(0, 1074, new MonoBehaviourMessage(monoBehaviourMessageType));
				}
				else
				{
					StageController.Instance.ChangeStage(this.NEXT_STAGE[selectedKey]);
				}
			}
			else if (selectedKey == 5)
			{
				bool loggedIn = this.fb_.LoggedIn;
				if (loggedIn)
				{
					bool flag;
					if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
					{
						if (iPhoneSettings.internetReachability == iPhoneNetworkReachability.NotReachable)
						{
							iOSEvent.Alert("네트워크에 연결할 수 없습니다.");
							flag = false;
						}
						else
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						this.updating_ |= 8;
						FiaCoroutine fiaCoroutine = new FiaCoroutine(this.fb_.Logout(), new OnSuccess(this.LogoutSuccess), new OnFailure(this.LogoutFailure));
						base.StartCoroutine(fiaCoroutine);
					}
				}
				else
				{
					bool flag2;
					if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
					{
						if (iPhoneSettings.internetReachability == iPhoneNetworkReachability.NotReachable)
						{
							iOSEvent.Alert("네트워크에 연결할 수 없습니다.");
							flag2 = false;
						}
						else
						{
							flag2 = true;
						}
					}
					else
					{
						flag2 = true;
					}
					if (flag2)
					{
						this.updating_ |= 4;
						FiaCoroutine fiaCoroutine2 = new FiaCoroutine(this.fb_.Login(), new OnSuccess(this.LoginSuccess), new OnFailure(this.LoginFailure));
						base.StartCoroutine(fiaCoroutine2);
					}
				}
				StageController.Instance.PlaySound(StageController.FxType.CLICK);
			}
			else if (selectedKey == 7)
			{
				using (AndroidJavaClass androidJavaClass3 = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
				{
					androidJavaClass3.CallStatic<int>("requestGamecenter", new object[] { string.Empty });
				}
			}
			else if (selectedKey == this.MOUSE_NOTIFIER_ID_USER_NAME && !this.fb_.LoggedIn)
			{
				StageController.Instance.PlaySound(StageController.FxType.SELECT);
				this.EditUserName();
			}
		}
		if (this.nextPopupState_ == GUIMain.NextPopupState.ACCOMPLISHED && StageController.Instance.InputAutority != 4)
		{
			this.nextPopupState_ = GUIMain.NextPopupState.NONE;
			GUIAccomplishPopup.OpenStaticPopup();
		}
		if (this.nextPopupState_ == GUIMain.NextPopupState.PATCH_SUMMARY)
		{
			if (PlayerPrefs.GetString("LAST_PATCH_SUMMARY") == KartOptions.Instance.ProgramVersion)
			{
				this.nextPopupState_ = GUIMain.NextPopupState.NONE;
				GUIAccomplishPopup.OpenStaticPopup();
			}
			else
			{
				MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.PATCH_SUMMARY_POPUP_MESSAGE);
				MonoBehaviourExCenter.Instance.SendMessage(0, 270, monoBehaviourMessage1Param.Initialize(1));
				PlayerPrefs.SetString("LAST_PATCH_SUMMARY", KartOptions.Instance.ProgramVersion);
				this.nextPopupState_ = GUIMain.NextPopupState.ACCOMPLISHED;
			}
		}
	}

	private void EditUserName()
	{
		this.updating_ |= 2;
		this.enterNameDesc_.Visible = true;
		this.enterNameBack_.Visible = true;
		this.defaultName_ = KartOptions.Instance.DefaultUserName;
		iPhoneKeyboard.hideInput = true;
		this.prevKeyboardText_ = null;
		this.keyboard_ = iPhoneKeyboard.Open(this.defaultName_, iPhoneKeyboardType.ASCIICapable, false, false, false, false, string.Empty);
		base.Invoke("NameEditCursorVisible", 0.5f);
	}

	private void NameEditCursor(bool visible_)
	{
		this.enterNameCursor_.Visible = visible_;
	}

	private void NameEditCursorVisible()
	{
		this.NameEditCursor(true);
		base.Invoke("NameEditCursorInvisible", 0.5f);
	}

	private void NameEditCursorInvisible()
	{
		this.NameEditCursor(false);
		base.Invoke("NameEditCursorVisible", 0.5f);
	}

	private void UpdateRanking()
	{
		this.updating_ |= 1;
		KartOptions.Instance.UpdateQuestFlag();
		KartOptions.Instance.SaveRegistry();
		GUIAccomplishPopup.OpenStaticPopup();
		RankingUpdater rankingUpdater = new RankingUpdater(this.fb_.FBID, this.fb_.FriendDict, FiaAuth.AuthToken);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(rankingUpdater.Update(), new OnSuccess(this.RankingUpdateSuccess), new OnFailure(this.RankingUpdateFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	private void RankingUpdateSuccess()
	{
		this.updating_ &= -2;
	}

	private void RankingUpdateFailure(Exception ex)
	{
		this.updating_ &= -2;
	}

	private void LogoutSuccess()
	{
		this.updating_ &= -9;
		this.ChangeUser();
	}

	private void LogoutFailure(Exception ex)
	{
		this.updating_ &= -9;
	}

	private void LoginSuccess()
	{
		this.updating_ &= -5;
		this.ChangeUser();
		this.UpdateRanking();
	}

	private void LoginFailure(Exception ex)
	{
		this.updating_ &= -5;
		MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FACEBOOK_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, monoBehaviourMessage1Param.Initialize(1));
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
		else if (type == typeof(FacebookCanceledException))
		{
		}
	}

	private void ChangeUser()
	{
		KartOptions.Instance.LoadRegistry();
		this.guiName_.SetString(KartOptions.Instance.LastUserName);
	}

	private const int BUTTON_NO = 5;

	private const int ALERT_Y_POS = 195;

	private string prevKeyboardText_;

	private MouseManager mouseManager_;

	private GUIPanelEx back_;

	private GUIPanelEx[] buttons_ = new GUIPanelEx[8];

	private GUIPanelEx[] alerts_ = new GUIPanelEx[5];

	private string name_;

	private GUIString guiName_;

	private Facebook fb_;

	private int updating_;

	private iPhoneKeyboard keyboard_;

	private string defaultName_ = string.Empty;

	private string DEFAULT_USER_NAME = "Rush";

	private GUIPanelEx enterNameDesc_;

	private GUIPanelEx enterNameBack_;

	private GUIPanelEx enterNameCursor_;

	private GUIMain.NextPopupState nextPopupState_;

	private int[] ALERT_X_POS = new int[] { 135, 291, 447, 603, 759 };

	public static bool isCheckGUIAccomplishPopup_ = true;

	private int MOUSE_NOTIFIER_ID_USER_NAME = 8;

	private int[] DEFAULT_BUTTON_IDX = new int[] { 0, 1, 2, 3, 4 };

	private StageType[] NEXT_STAGE = new StageType[]
	{
		StageType.SINGLE_ITEM,
		StageType.SINGLE_SPEED,
		StageType.WIFI,
		StageType.GARAGE,
		StageType.STORE,
		StageType.NONE,
		StageType.INFO
	};

	private enum NextPopupState
	{
		KEYBOARD,
		PATCH_SUMMARY,
		ACCOMPLISHED,
		NONE
	}

	private enum UpdatingFlag
	{
		RANKING = 1,
		DEFAULT_NAME_SETTING,
		LOG_IN = 4,
		LOG_OUT = 8
	}

	private enum ButtonType
	{
		Item,
		Speed,
		Wifi,
		Garage,
		Shop,
		Facebook,
		Info,
		Gamecenter
	}
}
