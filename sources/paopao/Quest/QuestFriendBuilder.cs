using System;

public class QuestFriendBuilder : QuestBuilder
{
	public QuestFriendBuilder(string key, int paramCnt)
		: base(key, paramCnt)
	{
	}

	public override QuestBase Build(string[] tokens)
	{
		int num = int.Parse(tokens[1]);
		return new QuestFriend(num);
	}
}
