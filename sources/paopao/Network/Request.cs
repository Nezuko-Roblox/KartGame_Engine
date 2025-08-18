using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Request : IStartable
{
	public Request(string url)
	{
		this.url_ = url;
		this.fields_ = new Dictionary<string, string>();
		this.AddField("api_version", KartOptions.Instance.ApiVersion);
		this.AddField("program_version", KartOptions.Instance.ProgramVersion);
	}

	public Request(string url, Dictionary<string, string> fields)
	{
		this.url_ = url;
		this.fields_ = fields;
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

	public string Result
	{
		get
		{
			return this.result_;
		}
	}

	public byte[] ByteResult
	{
		get
		{
			return this.byteResult_;
		}
	}

	public abstract IEnumerator Run();

	public IEnumerator Run(RequestDelegate del)
	{
		IEnumerator i = this.Run();
		while (i.MoveNext())
		{
			object obj = i.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	public void AddField(string k, string v)
	{
		this.fields_.Add(k, v);
	}

	public void AddField(string k, int v)
	{
		this.fields_.Add(k, v + string.Empty);
	}

	public void AddField(string k, float v)
	{
		this.fields_.Add(k, v + string.Empty);
	}

	public void AddField(string k, ICollection<string> values)
	{
		string[] array = new string[values.Count];
		values.CopyTo(array, 0);
		string text = string.Join(",", array);
		this.fields_.Add(k, text);
	}

	public void AddBinaryData(string k, byte[] v)
	{
		this.data_.Add(k, v);
	}

	public void SignRequest()
	{
	}

	public XMLResponse XMLResponse
	{
		get
		{
			if (this.xmlResponse_ == null)
			{
				this.xmlResponse_ = new XMLResponse(this.result_);
			}
			return this.xmlResponse_;
		}
	}

	public ByteArrayResponse ByteArrayResponse
	{
		get
		{
			if (this.byteArrayResponse_ == null)
			{
				this.byteArrayResponse_ = new ByteArrayResponse(this.byteResult_);
			}
			return this.byteArrayResponse_;
		}
	}

	protected void SetState(WWW www)
	{
		if (!www.isDone)
		{
			this.State = StartableState.FAILED;
			this.Error = new RequestException("Timed out");
			this.XMLResponse.Error = this.Error;
		}
		else if (www.error != null)
		{
			this.State = StartableState.FAILED;
			this.Error = new RequestException(www.error);
		}
		else
		{
			this.result_ = this.www_.text;
			this.byteResult_ = this.www_.bytes;
			this.State = StartableState.WAITING;
		}
	}

	public const float MAX_REQUEST_DURATION = 30f;

	public string url_;

	public Dictionary<string, string> fields_;

	public Dictionary<string, byte[]> data_;

	protected WWW www_;

	protected AndroidJavaObject wwwAsyncTask_;

	protected XMLResponse xmlResponse_;

	protected ByteArrayResponse byteArrayResponse_;

	protected StartableState state_;

	protected Exception error_;

	protected string result_;

	protected byte[] byteResult_;
}
