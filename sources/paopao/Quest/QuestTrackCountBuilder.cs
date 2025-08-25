using System;

public class QuestTrackCountBuilder : QuestBuilder
{
	public QuestTrackCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		return new QuestTrackCount(num);
	}
}
