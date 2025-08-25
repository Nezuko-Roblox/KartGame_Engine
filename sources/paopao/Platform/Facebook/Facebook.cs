using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Facebook : IStartable
{
	protected Facebook()
	{
		Facebook._New("108846335834433", "offline_access,email");
	}

	public StartableState State
	{
		get
		{
			return this.state_;
		}
		set
		{
			this.state_ = value;
		}
	}

	public Exception Error
	{
		get
		{
			return this.error_;
		}
		set
		{
			this.error_ = value;
		}
	}

	public virtual Dictionary<string, string> FriendDict
	{
		get
		{
			return this.friendDict_;
		}
		set
		{
			this.friendDict_ = value;
		}
	}

	public virtual string AccessToken
	{
		get
		{
			string @string = PlayerPrefs.GetString("FACEBOOK_ACCESS_TOKEN");
			if (@string == string.Empty)
			{
				return null;
			}
			return @string;
		}
		set
		{
			if (value == string.Empty || value == null)
			{
				PlayerPrefs.DeleteKey("FACEBOOK_ACCESS_TOKEN");
			}
			else
			{
				PlayerPrefs.SetString("FACEBOOK_ACCESS_TOKEN", value);
			}
		}
	}

	public virtual string FBID
	{
		get
		{
			string @string = PlayerPrefs.GetString("FACEBOOK_ID");
			if (@string == string.Empty)
			{
				return null;
			}
			return @string;
		}
		set
		{
			if (value == string.Empty || value == null)
			{
				PlayerPrefs.DeleteKey("FACEBOOK_ID");
			}
			else
			{
				PlayerPrefs.SetString("FACEBOOK_ID", value);
			}
		}
	}

	public virtual string UserName
	{
		get
		{
			string @string = PlayerPrefs.GetString("FACEBOOK_USER_NAME");
			if (@string == string.Empty)
			{
				return null;
			}
			return @string;
		}
		set
		{
			if (value == string.Empty || value == null)
			{
				PlayerPrefs.DeleteKey("FACEBOOK_USER_NAME");
			}
			else
			{
				PlayerPrefs.SetString("FACEBOOK_USER_NAME", value);
			}
		}
	}

	private static void _New(string appID, string permissions)
	{
		Debug.Log("_New");
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.unity3d.Plugins.FBUnity"))
					{
						Debug.Log("call _New");
						androidJavaClass2.CallStatic("_New", new object[] { @static, appID, permissions });
					}
				}
			}
		}
	}

	private static void _Login()
	{
		Debug.Log("_Login Android");
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

	public static Facebook Inst
	{
		get
		{
			if (Facebook.fb_ == null)
			{
				Facebook.fb_ = new Facebook();
			}
			return Facebook.fb_;
		}
	}

	public virtual IEnumerator Login()
	{
		this.Start();
		this.fbMsg_ = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FACEBOOK_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.fbMsg_.Initialize(0));
		Facebook._Login();
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.Error != null)
		{
			throw this.Error;
		}
		this.Start();
		yield return new WaitForSeconds(0.5f);
		IEnumerator fetch = this.FetchUserInfo();
		while (fetch.MoveNext())
		{
			object obj = fetch.Current;
			yield return obj;
		}
		fetch = this.FetchFriends();
		while (fetch.MoveNext())
		{
			object obj2 = fetch.Current;
			yield return obj2;
		}
		fetch = FiaAuth.FetchAuthToken(this.FBID, this.AccessToken);
		while (fetch.MoveNext())
		{
			object obj3 = fetch.Current;
			yield return obj3;
		}
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.fbMsg_.Initialize(1));
		yield break;
	}

	public virtual IEnumerator Login(FacebookDelegate del)
	{
		IEnumerator i = this.Login();
		while (i.MoveNext())
		{
			object obj = i.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	public static void LoginSuccess(string accessToken)
	{
		Facebook.Inst.State = StartableState.WAITING;
		Facebook.Inst.AccessToken = accessToken;
	}

	public static void LoginFailure(bool canceled)
	{
		Facebook.Inst.State = StartableState.FAILED;
		if (canceled)
		{
			Facebook.Inst.Error = new FacebookCanceledException();
		}
		else
		{
			Facebook.Inst.Error = new FacebookRequestException();
		}
	}

	public virtual IEnumerator Logout()
	{
		this.Start();
		this.AccessToken = null;
		this.FBID = null;
		this.UserName = null;
		FiaAuth.AuthToken = null;
		RankingParameter.Reset();
		Facebook._Logout();
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		yield break;
	}

	public virtual IEnumerator Logout(FacebookDelegate del)
	{
		IEnumerator i = this.Logout();
		while (i.MoveNext())
		{
			object obj = i.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	public static void LogoutSuccess()
	{
		Facebook.Inst.State = StartableState.WAITING;
	}

	public IEnumerator Publish(string name, string caption, string description, string href, string imageHref)
	{
		this.Start();
		Facebook._Publish(name, caption, description, href, imageHref);
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		yield break;
	}

	public IEnumerator Publish(string name, string caption, string description, string href, string imageHref, FacebookDelegate del)
	{
		IEnumerator i = this.Publish(name, caption, description, href, imageHref);
		while (i.MoveNext())
		{
			object obj = i.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	public static void PublishSuccess()
	{
		Facebook.Inst.State = StartableState.WAITING;
	}

	public static void PublishFailure()
	{
		Facebook.Inst.State = StartableState.FAILED;
		Facebook.Inst.Error = new FacebookCanceledException();
	}

	public IEnumerator Request()
	{
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		yield break;
	}

	public static void RequestSuccess(string json)
	{
		Facebook.Inst.State = StartableState.WAITING;
	}

	public static void RequestFailure()
	{
		Facebook.Inst.State = StartableState.FAILED;
		Facebook.Inst.Error = new FacebookCanceledException();
	}

	protected void Start()
	{
		this.State = StartableState.RUNNING;
		this.Error = null;
	}

	public bool LoggedIn
	{
		get
		{
			return this.AccessToken != null && this.UserName != null && this.FBID != null && this.FriendDict != null && FiaAuth.AuthToken != null;
		}
	}

	public string PictureURLForID(string id)
	{
		return "http://graph.facebook.com/" + id + "/picture";
	}

	public IEnumerator FetchUserInfo()
	{
		Request req = new FQLRequest("SELECT uid, name FROM user WHERE uid = me()", this.AccessToken);
		IEnumerator run = req.Run();
		while (run.MoveNext())
		{
			object obj = run.Current;
			yield return obj;
		}
		if (req.XMLResponse.Error != null)
		{
			throw req.XMLResponse.Error;
		}
		this.ParseUserXML(req.XMLResponse.XML);
		yield return new WaitForSeconds(0.1f);
		yield break;
	}

	public IEnumerator FetchFriends()
	{
		Request req = new FQLRequest("SELECT uid, name FROM user WHERE is_app_user = 1 AND uid IN (SELECT uid1 FROM friend WHERE uid2 = me())", this.AccessToken);
		IEnumerator run = req.Run();
		while (run.MoveNext())
		{
			object obj = run.Current;
			yield return obj;
		}
		if (req.XMLResponse.Error != null)
		{
			throw req.XMLResponse.Error;
		}
		this.ParseFriendXML(req.XMLResponse.XML);
		this.SaveFriendXML();
		this.friendDict_.Add(Facebook.Inst.FBID, Facebook.Inst.UserName);
		yield return new WaitForSeconds(0.1f);
		yield break;
	}

	public void FetchFriendsNatively()
	{
		string text = "https://api.facebook.com/method/fql.query?";
		text = text + "&api_version=" + KartOptions.Instance.ApiVersion;
		text = text + "&program_version=" + KartOptions.Instance.ProgramVersion;
		text += "&format=xml";
		text += "&query=SELECT+uid,+name+FROM+user+WHERE+is_app_user+%3d+1+AND+uid+IN+(SELECT+uid1+FROM+friend+WHERE+uid2+%3d+me())";
		text = text + "&access_token=" + WWW.EscapeURL(this.AccessToken).Replace("|", "%7C");
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("org.apache.http.impl.client.DefaultHttpClient", new object[0]);
		AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("org.apache.http.client.methods.HttpGet", new object[] { text });
		AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("org.apache.http.impl.client.BasicResponseHandler", new object[0]);
		string text2 = androidJavaObject.Call<string>("execute", new object[] { androidJavaObject2, androidJavaObject3 });
		XMLElement xmlelement = new XMLElement();
		try
		{
			xmlelement.parseString(text2);
		}
		catch (Exception ex)
		{
			this.Error = new XMLParsingException(ex);
		}
		if (xmlelement != null)
		{
			if (!(xmlelement.getName() == "error"))
			{
				if (!(xmlelement.getName() == "error_response"))
				{
					goto IL_01A3;
				}
			}
			try
			{
				if (xmlelement.getName() == "error")
				{
					this.Error = new UnknownException("Fia Error");
				}
				else if (xmlelement.getName() == "error_response")
				{
					this.Error = new UnknownException("Facebook Error");
				}
			}
			catch (Exception ex2)
			{
				this.Error = new UnknownException(ex2);
			}
			if (this.Error == null)
			{
				this.Error = new UnknownException();
			}
		}
		IL_01A3:
		this.ParseFriendXML(xmlelement);
		this.SaveFriendXML();
		this.friendDict_.Add(Facebook.Inst.FBID, Facebook.Inst.UserName);
	}

	private void FetchAuthTokenNatively(string fbid, string accessToken)
	{
		string text = "http://s.kartriderrush.com/server/auth.php?";
		text = text + "&api_version=" + KartOptions.Instance.ApiVersion;
		text = text + "&program_version=" + KartOptions.Instance.ProgramVersion;
		text = text + "&fbid=" + fbid;
		text = text + "&access_token=" + WWW.EscapeURL(accessToken).Replace("|", "%7C");
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("org.apache.http.impl.client.DefaultHttpClient", new object[0]);
		AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("org.apache.http.client.methods.HttpGet", new object[] { text });
		AndroidJavaObject androidJavaObject3 = new AndroidJavaObject("org.apache.http.impl.client.BasicResponseHandler", new object[0]);
		string text2 = androidJavaObject.Call<string>("execute", new object[] { androidJavaObject2, androidJavaObject3 });
		XMLElement xmlelement = new XMLElement();
		try
		{
			xmlelement.parseString(text2);
		}
		catch (Exception ex)
		{
			this.Error = new XMLParsingException(ex);
		}
		if (xmlelement != null)
		{
			if (!(xmlelement.getName() == "error"))
			{
				if (!(xmlelement.getName() == "error_response"))
				{
					goto IL_0193;
				}
			}
			try
			{
				if (xmlelement.getName() == "error")
				{
					this.Error = new UnknownException("Fia Error");
				}
				else if (xmlelement.getName() == "error_response")
				{
					this.Error = new UnknownException("Facebook Error");
				}
			}
			catch (Exception ex2)
			{
				this.Error = new UnknownException(ex2);
			}
			if (this.Error == null)
			{
				this.Error = new UnknownException();
			}
		}
		IL_0193:
		string content = xmlelement.getContent();
		PlayerPrefs.SetString("FIA_TOKEN", content);
	}

	public void ParseUserXML(XMLElement xml)
	{
		foreach (object obj in ((XMLElement)xml.getChildren()[0]).getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			if (xmlelement.getName() == "uid")
			{
				Facebook.Inst.FBID = xmlelement.getContent();
			}
			else if (xmlelement.getName() == "name")
			{
				Facebook.Inst.UserName = xmlelement.getContent();
			}
		}
	}

	public void ParseFriendXML(XMLElement xml)
	{
		this.friendDict_ = new Dictionary<string, string>();
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			string text = null;
			string text2 = null;
			foreach (object obj2 in xmlelement.getChildren())
			{
				XMLElement xmlelement2 = (XMLElement)obj2;
				string name = xmlelement2.getName();
				if (name != null)
				{
					if (Facebook.<>f__switch$map1 == null)
					{
						Facebook.<>f__switch$map1 = new Dictionary<string, int>(2)
						{
							{ "uid", 0 },
							{ "name", 1 }
						};
					}
					int num;
					if (Facebook.<>f__switch$map1.TryGetValue(name, out num))
					{
						if (num != 0)
						{
							if (num == 1)
							{
								text2 = xmlelement2.getContent();
							}
						}
						else
						{
							text = xmlelement2.getContent();
						}
					}
				}
			}
			this.friendDict_[text] = text2;
		}
	}

	public bool SaveFriendXML()
	{
		bool flag;
		try
		{
			using (StreamWriter streamWriter = new StreamWriter(this.FriendListLocalPath))
			{
				streamWriter.Write("<friends>");
				foreach (string text in this.friendDict_.Keys)
				{
					streamWriter.Write("<user><uid>");
					streamWriter.Write(text);
					streamWriter.Write("</uid><name>");
					streamWriter.Write(this.friendDict_[text]);
					streamWriter.Write("</name></user>");
				}
				streamWriter.Write("</friends>");
			}
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
		}
		return flag;
	}

	public bool LoadFriendXML()
	{
		bool flag;
		try
		{
			XMLElement xmlelement = new XMLElement();
			using (StreamReader streamReader = new StreamReader(this.FriendListLocalPath))
			{
				xmlelement.parseString(streamReader.ReadToEnd());
			}
			this.ParseFriendXML(xmlelement);
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
		}
		return flag;
	}

	private string FriendListLocalPath
	{
		get
		{
			string text = FiaUtil.docPath;
			text = Path.Combine(text, "players");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return Path.Combine(text, string.Format("{0}_{1}.xml", this.FBID, "friends"));
		}
	}

	private const string _GRAPH_BASE_URL = "graph.facebook.com/";

	public const string GRAPH_BASE_URL = "https://graph.facebook.com/";

	public const string GRAPH_BASE_URL_HTTP = "http://graph.facebook.com/";

	private const string APP_ID = "108846335834433";

	private const string PERMISSIONS = "offline_access,email";

	private StartableState state_;

	private Exception error_;

	protected static Facebook fb_;

	protected Dictionary<string, string> friendDict_;

	private MonoBehaviourMessage1Param<int> fbMsg_;
}
