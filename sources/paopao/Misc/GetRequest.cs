using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRequest : Request
{
	public GetRequest(string url)
		: base(url)
	{
	}

	public GetRequest(string url, Dictionary<string, string> fields)
		: base(url, fields)
	{
	}

	public override IEnumerator Run()
	{
		base.State = StartableState.RUNNING;
		string url = this.url_;
		base.Error = null;
		if (this.fields_ != null)
		{
			if (url.IndexOf('?') < 0)
			{
				url += "?";
			}
			foreach (KeyValuePair<string, string> kvp in this.fields_)
			{
				string escval = WWW.EscapeURL(kvp.Value);
				escval = escval.Replace("|", "%7C");
				url += string.Format("&{0}={1}", kvp.Key, escval);
			}
		}
		if (Debug.isDebugBuild)
		{
			Debug.Log(url);
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject httpGet = new AndroidJavaObject("org.apache.http.client.methods.HttpGet", new object[] { url });
			AndroidJavaObject r = new AndroidJavaObject("com.nexon.kartriderrush.android.core.AsyncRunnable", new object[] { httpGet });
			AndroidJavaObject obj_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			obj_Activity.Call("runOnUiThread", new object[] { r });
			while (!r.Call<bool>("isStarted", new object[0]))
			{
				yield return new WaitForSeconds(0.1f);
			}
			this.wwwAsyncTask_ = r.Call<AndroidJavaObject>("getWWWAsyncTask", new object[0]);
		}
		else
		{
			this.www_ = new WWW(url);
		}
		if (this.State != StartableState.RUNNING)
		{
			Debug.LogError("State != StartableState.RUNNING");
		}
		if (Application.platform == RuntimePlatform.Android)
		{
			float timeout = Time.time + 30f;
			while (timeout > Time.time && this.wwwAsyncTask_.Call<int>("getStatusOrdinal", new object[0]) != 2)
			{
				yield return new WaitForSeconds(0.1f);
			}
			if (this.wwwAsyncTask_.Call<int>("getStatusOrdinal", new object[0]) != 2)
			{
				this.wwwAsyncTask_.Call("cancel", new object[0]);
				base.Error = new RequestException("Timed out");
				base.XMLResponse.Error = this.Error;
				base.State = StartableState.FAILED;
			}
			else if (this.wwwAsyncTask_.Call<string>("getError", new object[0]) != null)
			{
				base.Error = new RequestException(this.wwwAsyncTask_.Call<string>("getError", new object[0]));
				base.State = StartableState.FAILED;
			}
			else
			{
				this.result_ = this.wwwAsyncTask_.Call<string>("getResult", new object[0]);
				this.byteResult_ = Convert.FromBase64String(this.wwwAsyncTask_.Call<string>("getBase64Encoded", new object[0]));
				base.State = StartableState.WAITING;
			}
		}
		else
		{
			float timeout2 = Time.time + 30f;
			while (timeout2 > Time.time && !this.www_.isDone)
			{
				yield return new WaitForSeconds(0.1f);
			}
			base.SetState(this.www_);
		}
		yield break;
	}
}
