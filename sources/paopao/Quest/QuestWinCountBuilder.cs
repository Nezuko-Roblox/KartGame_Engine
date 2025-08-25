using System;
using System.Globalization;

public class QuestWinCountBuilder : QuestBuilder
{
	public QuestWinCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		ulong num2 = ulong.Parse(tokens[2], NumberStyles.HexNumber);
		byte b = byte.Parse(tokens[3], NumberStyles.HexNumber);
		return new QuestWinCount(num, num2, b);
	}
}
