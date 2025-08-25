using System;
using System.Globalization;

public class QuestRaceCompleteBuilder : QuestBuilder
{
	public QuestRaceCompleteBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		ulong num = ulong.Parse(tokens[1], NumberStyles.HexNumber);
		byte b = byte.Parse(tokens[2], NumberStyles.HexNumber);
		QuestRaceComplete.enOperator enOperator = (QuestRaceComplete.enOperator)((int)Enum.Parse(typeof(QuestRaceComplete.enOperator), tokens[3], true));
		return new QuestRaceComplete(num, b, enOperator);
	}
}
