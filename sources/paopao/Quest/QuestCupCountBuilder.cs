using System;

public class QuestCupCountBuilder : QuestBuilder
{
	public QuestCupCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		return new QuestCupCount(num);
	}
}
