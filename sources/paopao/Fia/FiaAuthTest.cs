using System;
using UnityEngine;

public class FiaAuthTest : MonoBehaviour
{
	public void Awake()
	{
		Facebook facebook = ((!Env.IsDesktop) ? Facebook.Inst : MockFacebook.Inst);
		FiaCoroutine fiaCoroutine = new FiaCoroutine(FiaAuth.FetchAuthToken(facebook.FBID, facebook.AccessToken), new OnSuccess(this.FetchSuccess), new OnFailure(this.FetchFailure));
		base.StartCoroutine(fiaCoroutine);
	}

	public void FetchSuccess()
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("success " + FiaAuth.AuthToken);
		}
	}

	public void FetchFailure(Exception ex)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log(ex);
		}
	}
}
