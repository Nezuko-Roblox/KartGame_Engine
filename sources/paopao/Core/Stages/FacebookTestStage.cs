using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FacebookTestStage : MonoBehaviour
{
	private void Start()
	{
		Facebook inst = Facebook.Inst;
		Debug.Log("new facebook" + inst);
		if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
		{
			this.screenOrientation = "LandscapeLeft";
		}
		else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
		{
			this.screenOrientation = "LandscapeRight";
		}
	}

	private IEnumerator LoadImage()
	{
		Debug.Log("LoadImage");
		string fbid_ = "100001422398072";
		string filename_ = fbid_ + ".png";
		string url_ = "http://graph.facebook.com/" + fbid_ + "/picture";
		Debug.Log("URL: " + url_);
		WWW www_ = new WWW(url_);
		yield return www_;
		Debug.Log("www_.isDone = " + www_.isDone);
		Debug.Log("www_.error = " + www_.error);
		string path_ = FiaUtil.docPath;
		path_ = Path.Combine(path_, "fbImage");
		path_ = Path.Combine(path_, filename_);
		Debug.Log("PATH: " + path_);
		try
		{
			string dir_ = Path.GetDirectoryName(path_);
			if (!Directory.Exists(dir_))
			{
				Directory.CreateDirectory(dir_);
			}
			Debug.Log("www_.bytes: " + www_.bytes);
			using (FileStream fileStream_ = new FileStream(path_, FileMode.Create))
			{
				BinaryWriter binaryWriter_ = new BinaryWriter(fileStream_);
				binaryWriter_.Write(www_.bytes);
				binaryWriter_.Flush();
			}
			Debug.Log("ImageSaved");
		}
		catch (Exception ex2)
		{
			Exception ex = ex2;
			Debug.Log("Error: " + ex);
		}
		yield break;
	}

	private void LoginSuccess()
	{
		Debug.Log("Success!!");
	}

	private void LoginFailure(Exception ex)
	{
		Debug.Log("Failure!!");
		Debug.Log(ex);
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 220f, 220f), "Login"))
		{
			FacebookTestStage._Login();
		}
		else if (GUI.Button(new Rect(250f, 10f, 220f, 220f), "Logout"))
		{
			FacebookTestStage._Logout();
		}
		else if (GUI.Button(new Rect(10f, 250f, 220f, 220f), "Post"))
		{
			string text = "Mr. Tester";
			string text2 = "{0} set a new record of {1} in the {2} track!";
			string text3 = string.Format("{0}:{1:00}:{2:00}", 1, 23, 45);
			string text4 = "The test land";
			text2 = string.Format(text2, text, text3, text4);
			string empty = string.Empty;
			string text5 = "Race {0} on KartRider Rush, the free iPhone racing game from Nexon.";
			text5 = string.Format(text5, text);
			string text6 = "http://s.kartriderrush.com/resources/to_iphone_app.html";
			string text7 = "http://s.kartriderrush.com/resources/facebook_new_record.png";
			FacebookTestStage._Publish(text2, empty, text5, text6, text7);
		}
		else if (GUI.Button(new Rect(250f, 250f, 220f, 220f), "LoadImage"))
		{
			base.StartCoroutine(this.LoadImage());
		}
		else if (GUI.Button(new Rect(480f, 10f, 460f, 460f), this.androidVersion + "\n" + this.screenOrientation))
		{
		}
	}

	private void Update()
	{
		if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
		{
			this.screenOrientation = "LandscapeLeft";
			Screen.orientation = ScreenOrientation.LandscapeLeft;
		}
		else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
		{
			this.screenOrientation = "LandscapeRight";
			Screen.orientation = ScreenOrientation.LandscapeRight;
		}
		else if (Input.deviceOrientation == DeviceOrientation.Portrait)
		{
			this.screenOrientation = "Portrait";
			Screen.orientation = ScreenOrientation.Portrait;
		}
		else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
		{
			this.screenOrientation = "Portrait";
			Screen.orientation = ScreenOrientation.PortraitUpsideDown;
		}
	}

	private static void _New(string appID, string permissions)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.unity3d.Plugins.FBUnity"))
					{
						androidJavaClass2.CallStatic("_New", new object[] { @static, appID, permissions });
					}
				}
			}
		}
	}

	private static void _Login()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.Plugins.FBUnity"))
			{
				androidJavaClass.CallStatic("_Login", new object[0]);
			}
		}
	}

	private static void _Logout()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.Plugins.FBUnity"))
			{
				androidJavaClass.CallStatic("_Logout", new object[0]);
			}
		}
	}

	private static void _Publish(string name, string caption, string description, string href, string imageHref)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.Plugins.FBUnity"))
			{
				androidJavaClass.CallStatic("_Publish", new object[] { name, caption, description, href, imageHref });
			}
		}
	}

	private string screenOrientation = string.Empty;

	private string androidVersion = "None";
}
