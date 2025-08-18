using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostRequest : Request
{
	public PostRequest(string url)
		: base(url)
	{
	}

	public override IEnumerator Run()
	{
		base.State = StartableState.RUNNING;
		string url = this.url_;
		base.Error = null;
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJNIHelper.debug = false;
			AndroidJavaObject httpPost = new AndroidJavaObject("org.apache.http.client.methods.HttpPost", new object[] { url });
			AndroidJavaObject nameValuePairs = new AndroidJavaObject("java.util.ArrayList", new object[0]);
			if (this.fields_ != null)
			{
				foreach (KeyValuePair<string, string> kvp in this.fields_)
				{
					nameValuePairs.Call<bool>("add", new object[]
					{
						new AndroidJavaObject("org.apache.http.message.BasicNameValuePair", new object[] { kvp.Key, kvp.Value })
					});
				}
			}
			if (this.data_ != null)
			{
				foreach (KeyValuePair<string, byte[]> kvp2 in this.data_)
				{
					nameValuePairs.Call<bool>("add", new object[]
					{
						new AndroidJavaObject("org.apache.http.message.BasicNameValuePair", new object[] { kvp2.Key, kvp2.Value })
					});
				}
			}
			AndroidJavaObject entityRequest = new AndroidJavaObject("org.apache.http.client.entity.UrlEncodedFormEntity", new object[] { nameValuePairs, "UTF-8" });
			httpPost.Call("setEntity", new object[] { entityRequest });
			AndroidJavaObject r = new AndroidJavaObject("com.nexon.kartriderrush.android.core.AsyncRunnable", new object[] { httpPost });
			AndroidJavaObject obj_Activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			obj_Activity.Call("runOnUiThread", new object[] { r });
			while (!r.Call<bool>("isStarted", new object[0]))
			{
				yield return new WaitForSeconds(0.1f);
			}
			this.wwwAsyncTask_ = r.Call<AndroidJavaObject>("getWWWAsyncTask", new object[0]);
			AndroidJNIHelper.debug = false;
		}
		else
		{
			WWWForm form = new WWWForm();
			if (this.fields_ != null)
			{
				foreach (KeyValuePair<string, string> kvp3 in this.fields_)
				{
					form.AddField(kvp3.Key, kvp3.Value);
				}
			}
			if (this.data_ != null)
			{
				foreach (KeyValuePair<string, byte[]> kvp4 in this.data_)
				{
					form.AddBinaryData(kvp4.Key, kvp4.Value);
				}
			}
			this.www_ = new WWW(url, form);
		}
		if (this.State != StartableState.RUNNING)
		{
			Debug.LogError("State != StartableState.RUNNING");
		}
		Debug.Log("yield start");
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

	public static PostRequest FromXML(XMLElement xml)
	{
		PostRequest postRequest = new PostRequest((string)xml.getAttribute("action"));
		foreach (object obj in xml.getChildren())
		{
			XMLElement xmlelement = (XMLElement)obj;
			postRequest.AddField(xmlelement.getName(), xmlelement.getContent());
		}
		return postRequest;
	}
}
