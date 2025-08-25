using System;

public class FQLRequest : GetRequest
{
	public FQLRequest(string query, string accessToken)
		: base("https://api.facebook.com/method/fql.query")
	{
		base.AddField("format", "xml");
		base.AddField("query", query);
		base.AddField("access_token", accessToken);
	}

	public const string FQL_BASE_URL = "https://api.facebook.com/method/fql.query";
}
