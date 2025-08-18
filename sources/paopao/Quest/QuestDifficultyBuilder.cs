using System;
using System.Globalization;

public class QuestDifficultyBuilder : QuestBuilder
{
	public QuestDifficultyBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		ulong num = ulong.Parse(tokens[1], NumberStyles.HexNumber);
		byte b = byte.Parse(tokens[2], NumberStyles.HexNumber);
		QuestDifficulty.enOperator enOperator = (QuestDifficulty.enOperator)((int)Enum.Parse(typeof(QuestDifficulty.enOperator), tokens[3], true));
		byte b2 = byte.Parse(tokens[4], NumberStyles.HexNumber);
		return new QuestDifficulty(num, b, enOperator, b2);
	}
}
