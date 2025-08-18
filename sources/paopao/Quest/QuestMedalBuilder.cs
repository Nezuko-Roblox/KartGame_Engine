using System;
using System.Globalization;

public class QuestMedalBuilder : QuestBuilder
{
	public QuestMedalBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		ulong num = ulong.Parse(tokens[1], NumberStyles.HexNumber);
		byte b = byte.Parse(tokens[2], NumberStyles.HexNumber);
		MedalType medalType = (MedalType)int.Parse(tokens[3]);
		return new QuestMedal(num, b, medalType);
	}
}
