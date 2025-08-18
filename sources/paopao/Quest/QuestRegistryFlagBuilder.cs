using System;
using System.Globalization;

public class QuestRegistryFlagBuilder : QuestBuilder
{
	public QuestRegistryFlagBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		uint num = uint.Parse(tokens[1], NumberStyles.HexNumber);
		return new QuestRegistryFlag(num);
	}
}
