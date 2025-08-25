using System;
using UnityEngine;

public class NativeHelper : MonoBehaviour
{
	public static NativeHelper Instance
	{
		get
		{
			return NativeHelper._instance;
		}
	}

	private void Start()
	{
		global::UnityEngine.Object.DontDestroyOnLoad(this);
		NativeHelper._instance = this;
		AndroidJNIHelper.debug = false;
	}

	private void Update()
	{
	}

	public void OnStart(string msg)
	{
		BackgroundNotifier.DidBecomeActive();
	}

	public void OnStop(string msg)
	{
		BackgroundNotifier.DidEnterBackground();
	}

	public void currentLocale(string msg)
	{
		NativeHelper.locale = msg;
	}

	public void hideInput(string msg)
	{
		if (iPhoneKeyboard.hideInput)
		{
			iPhoneKeyboard.hideInput = false;
		}
	}

	public static string buildType
	{
		get
		{
			string text;
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
			{
				if (androidJavaClass.CallStatic<string>("getBuildType", new object[0]) == null)
				{
					text = NativeHelper.buildType_;
				}
				else
				{
					text = androidJavaClass.CallStatic<string>("getBuildType", new object[0]);
				}
			}
			return text;
		}
	}

	public string getLocale()
	{
		return NativeHelper.locale;
	}

	public void OnKeyDown(string msg)
	{
		if (KartManager.Instance != null && KartManager.Instance.parameter_ != null)
		{
			GameObject gameObject = null;
			string[] array;
			switch (KartManager.Instance.parameter_.Stage)
			{
			case StageType.MAIN:
			{
				array = new string[] { "accomplish_popup", "gui_loading_popup", "gui_patch_summary_popup", "gui_tutorial_multi" };
				for (int i = 0; i < array.Length; i++)
				{
					gameObject = GameObject.Find("/bg01/" + array[i]);
					if (gameObject != null && gameObject.active)
					{
						break;
					}
				}
				if (gameObject == null)
				{
					iOSEvent.ShowExitPopup();
					return;
				}
				goto IL_0297;
			}
			case StageType.WAITROOM_HOST:
			case StageType.WAITROOM_CLIENT:
			{
				array = new string[] { "accomplish_popup", "gui_waiting_players_popup", "gui_mode" };
				for (int j = 0; j < array.Length; j++)
				{
					gameObject = GameObject.Find(((j != array.Length - 1) ? "/bg01/" : "/bg02/") + array[j]);
					if (gameObject != null && gameObject.active)
					{
						break;
					}
				}
				goto IL_0297;
			}
			case StageType.GAME:
			{
				array = new string[] { "pause", "quit_popup", "controls" };
				for (int k = 0; k < array.Length; k++)
				{
					gameObject = GameObject.Find("/gamestage/all_gui_ipad/" + array[k]);
					if (gameObject != null && gameObject.active)
					{
						break;
					}
				}
				goto IL_0297;
			}
			case StageType.INFO:
				gameObject = GameObject.Find("/bg01/gui_info");
				if (gameObject != null && gameObject.active)
				{
					goto IL_0297;
				}
				goto IL_0297;
			}
			array = new string[] { "accomplish_popup", "gui_loading_popup", "gui_fb_login_popup", "gui_ranking_reset_popup", "quest_popup", "purchase_confirm_popup", "restore_purchase_popup", "package_popup", "gui_mode" };
			for (int l = 0; l < array.Length; l++)
			{
				gameObject = GameObject.Find("/bg01/" + array[l]);
				if (gameObject != null && gameObject.active)
				{
					break;
				}
			}
			IL_0297:
			if (gameObject != null && gameObject.active)
			{
				gameObject.SendMessage("BackButtonAction", msg);
			}
		}
	}

	protected static NativeHelper _instance;

	private static string locale = "ko";

	private static string buildType_ = "SKT";
}
