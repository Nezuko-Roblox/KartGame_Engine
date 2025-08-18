using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockFacebook : Facebook
{
	protected MockFacebook()
	{
		this.friendDict_ = new Dictionary<string, string>();
		this.friendDict_.Add("1572380590", "Yongha Kim");
		this.friendDict_.Add("100000060151270", "Ben Park");
		this.friendDict_.Add("100000098751065", "SoonGu Hwang");
		this.friendDict_.Add("100000401048181", "JongHyuk An");
		this.friendDict_.Add("100000526091775", "Chan Lee");
		this.friendDict_.Add("100000549697161", "Taehyung Lim");
		this.friendDict_.Add("100000558108547", "Sehoon Jung");
		this.friendDict_.Add("100000630337621", "Dongil Oh");
		this.friendDict_.Add("100000741339683", "Kyung Rip Min");
		this.friendDict_.Add("100000887492453", "Sookjung Kim");
		this.friendDict_.Add("100000917702368", "Sangmi Sun");
		this.friendDict_.Add("100001019105228", "Moonhyoung Park");
		this.friendDict_.Add("100001068730600", "Jongmoon Lee");
		this.friendDict_.Add("100001422398072", "Dae Hoon Han");
	}

	public override string AccessToken
	{
		get
		{
			return this.accessToken_;
		}
		set
		{
			this.accessToken_ = value;
		}
	}

	public override string FBID
	{
		get
		{
			return this.fbid_;
		}
		set
		{
			this.fbid_ = value;
		}
	}

	public override string UserName
	{
		get
		{
			return this.userName_;
		}
		set
		{
			this.userName_ = value;
		}
	}

	public new static Facebook Inst
	{
		get
		{
			if (Facebook.fb_ == null)
			{
				Facebook.fb_ = new MockFacebook();
			}
			return Facebook.fb_;
		}
	}

	public override IEnumerator Logout()
	{
		base.Start();
		this.AccessToken = null;
		this.FBID = null;
		this.UserName = null;
		this.friendDict_ = null;
		FiaAuth.AuthToken = null;
		RankingParameter.Reset();
		yield return null;
		yield break;
	}

	public override IEnumerator Login()
	{
		base.Start();
		if (this.accessToken_ == string.Empty)
		{
			yield return false;
		}
		this.friendDict_ = new Dictionary<string, string>();
		this.friendDict_.Add("100001422398072", "Dae Hoon Han");
		this.friendDict_.Add("100000558108547", "Sehoon Jung");
		this.fbMsg_ = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.FACEBOOK_MESSAGE);
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.fbMsg_.Initialize(0));
		yield return new WaitForSeconds(0.5f);
		IEnumerator fetch = FiaAuth.FetchAuthToken(this.FBID, this.AccessToken);
		while (fetch.MoveNext())
		{
			object obj = fetch.Current;
			yield return obj;
		}
		fetch = base.FetchUserInfo();
		while (fetch.MoveNext())
		{
			object obj2 = fetch.Current;
			yield return obj2;
		}
		fetch = base.FetchFriends();
		while (fetch.MoveNext())
		{
			object obj3 = fetch.Current;
			yield return obj3;
		}
		fetch = FiaAuth.FetchAuthToken(this.FBID, this.AccessToken);
		while (fetch.MoveNext())
		{
			object obj4 = fetch.Current;
			yield return obj4;
		}
		MonoBehaviourExCenter.Instance.SendMessage(0, 22, this.fbMsg_.Initialize(1));
		yield break;
	}

	public override Dictionary<string, string> FriendDict
	{
		get
		{
			return this.friendDict_;
		}
	}

	private string accessToken_ = string.Empty;

	private string fbid_ = string.Empty;

	private string userName_ = string.Empty;

	private MonoBehaviourMessage1Param<int> fbMsg_;
}
