using System;
using System.Collections;
using UnityEngine;

public class RecordPoster : IStartable
{
	public RecordPoster(int _recordLap, float _recordLabTime, float _diff)
	{
		this.recordLap_ = _recordLap;
		this.recordLabTime_ = _recordLabTime;
		this.diff_ = _diff;
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

	private string escape(string orig)
	{
		return orig;
	}

	public virtual IEnumerator Update()
	{
		this.Error = null;
		this.State = StartableState.RUNNING;
		Request req_ = new PostRequest(this.escape("http://kart-recorder.appspot.com"));
		req_.AddField("uuid", string.Empty + iPhoneSettings.uniqueIdentifier);
		req_.AddField("track", string.Empty + KartManager.Instance.parameter_.track_);
		req_.AddField("mode", string.Empty + KartManager.Instance.parameter_.gameMode_);
		req_.AddField("kart", string.Empty + KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].body_);
		req_.AddField("lab", string.Empty + this.recordLap_);
		req_.AddField("time", string.Empty + this.recordLabTime_);
		req_.AddField("diff", string.Empty + this.diff_);
		IEnumerator r = req_.Run();
		while (r.MoveNext())
		{
			object obj = r.Current;
			yield return obj;
		}
		if (req_.Error != null)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log(req_.Error);
			}
			this.Error = new UnknownException();
			this.State = StartableState.FAILED;
		}
		else
		{
			this.State = StartableState.WAITING;
		}
		yield break;
	}

	public IEnumerator Update(RecordPosterDelegate del)
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

	public const string REQUEST_URL = "http://kart-recorder.appspot.com";

	protected StartableState state_;

	protected Exception error_;

	protected int recordLap_;

	protected float recordLabTime_;

	protected float diff_;
}
