using System;

public class QuestMultiMapCupCountBuilder : QuestBuilder
{
	public QuestMultiMapCupCountBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		string[] array = tokens[1].Split(new char[] { ',' });
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = int.Parse(array[i]);
		}
		int num = int.Parse(tokens[2]);
		return new QuestMultiMapCupCount(num, array2);
	}
}
