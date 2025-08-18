using System;
using System.Globalization;

public class QuestRaceCountBuilder : QuestBuilder
{
	public QuestRaceCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		ulong num2 = ulong.Parse(tokens[2], NumberStyles.HexNumber);
		byte b = byte.Parse(tokens[3], NumberStyles.HexNumber);
		return new QuestRaceCount(num, num2, b);
	}
}
