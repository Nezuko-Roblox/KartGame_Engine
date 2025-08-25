using System;
using UnityEngine;

public class FBCallback : MonoBehaviour
{
	private void Start()
	{
		global::UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void Update()
	{
	}

	private void LoginSuccess(string accessToken)
	{
		Facebook.LoginSuccess(accessToken);
	}

	private void LoginFailure(string msg)
	{
		bool flag = false;
		if (msg == "true")
		{
			flag = true;
		}
		else if (msg == "false")
		{
			flag = false;
		}
		Facebook.LoginFailure(flag);
	}

	private void LogoutSuccess(string msg)
	{
		Facebook.LogoutSuccess();
	}

	private void PublishSuccess(string msg)
	{
		Facebook.PublishSuccess();
	}

	private void PublishFailure(string msg)
	{
		Facebook.PublishFailure();
	}
}
