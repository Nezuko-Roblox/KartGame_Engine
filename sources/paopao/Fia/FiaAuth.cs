using System;
using System.Collections;
using UnityEngine;

public class FiaAuth
{
	public static IEnumerator FetchAuthToken(string fbid, string accessToken)
	{
		GetRequest req = new GetRequest("http://s.kartriderrush.com/server/auth.php");
		req.AddField("fbid", fbid);
		req.AddField("access_token", accessToken);
		IEnumerator i = req.Run();
		while (i.MoveNext())
		{
			object obj = i.Current;
			yield return obj;
		}
		if (req.XMLResponse.Error != null)
		{
			throw req.XMLResponse.Error;
		}
		string fiaToken = req.XMLResponse.XML.getContent();
		PlayerPrefs.SetString("FIA_TOKEN", fiaToken);
		yield break;
	}

	public static string AuthToken
	{
		get
		{
			string @string = PlayerPrefs.GetString("FIA_TOKEN");
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
				PlayerPrefs.DeleteKey("FIA_TOKEN");
			}
			else
			{
				PlayerPrefs.SetString("FIA_TOKEN", value);
			}
		}
	}
}
