using System;
using System.Collections;
using System.IO;

public class FBProfileImageUpdater : IStartable
{
	public FBProfileImageUpdater(string _filename, string _url)
	{
		this.filename_ = _filename;
		this.url_ = _url;
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

	public virtual IEnumerator Update()
	{
		this.Error = null;
		this.State = StartableState.RUNNING;
		Request req_ = new GetRequest(this.url_, null);
		IEnumerator r = req_.Run();
		while (r.MoveNext())
		{
			object obj = r.Current;
			yield return obj;
		}
		if (req_.Error == null && req_.ByteResult != null)
		{
			try
			{
				string path_ = FiaUtil.docPath;
				path_ = Path.Combine(path_, "fbImage");
				path_ = Path.Combine(path_, this.filename_);
				string dir_ = Path.GetDirectoryName(path_);
				if (!Directory.Exists(dir_))
				{
					Directory.CreateDirectory(dir_);
				}
				using (FileStream fileStream_ = new FileStream(path_, FileMode.Create))
				{
					BinaryWriter binaryWriter_ = new BinaryWriter(fileStream_);
					binaryWriter_.Write(req_.ByteArrayResponse.Bytes);
					binaryWriter_.Flush();
				}
			}
			catch (Exception ex2)
			{
				Exception ex = ex2;
				this.Error = ex;
			}
		}
		else
		{
			this.Error = req_.Error;
		}
		if (this.Error != null)
		{
			this.State = StartableState.FAILED;
			throw this.Error;
		}
		this.State = StartableState.WAITING;
		yield break;
	}

	public IEnumerator Update(FBProfileImageUpdaterDelegate del)
	{
		IEnumerator e = this.Update();
		while (e.MoveNext())
		{
			object obj = e.Current;
			yield return obj;
		}
		del(this);
		yield break;
	}

	protected Exception error_;

	protected StartableState state_;

	protected string filename_;

	protected string url_;
}
