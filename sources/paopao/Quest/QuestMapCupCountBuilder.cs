using System;

public class QuestMapCupCountBuilder : QuestBuilder
{
	public QuestMapCupCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		int num2 = int.Parse(tokens[2]);
		int num3 = int.Parse(tokens[3]);
		return new QuestMapCupCount(num2 + num3, num, num2, num3);
	}
}
