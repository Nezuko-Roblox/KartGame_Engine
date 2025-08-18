using System;

public class QuestBuilderManager
{
	public static QuestBuilderManager Instance
	{
		get
		{
			if (QuestBuilderManager.instance_ == null)
			{
				QuestBuilderManager.instance_ = new QuestBuilderManager();
			}
			return QuestBuilderManager.instance_;
		}
	}

	public QuestBase Build(string str)
	{
		char[] array = new char[] { '_' };
		string[] array2 = str.Split(array, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < this.builder_.Length; i++)
		{
			if (this.builder_[i].IsRightFormat(array2))
			{
				return this.builder_[i].Build(array2);
			}
		}
		return null;
	}

	public static QuestBuilderManager instance_;

	private QuestBuilder[] builder_ = new QuestBuilder[]
	{
		new QuestRaceCountBuilder("racecount", 4),
		new QuestMedalBuilder("medal", 4),
		new QuestRegistryFlagBuilder("registry", 2),
		new QuestWinCountBuilder("wincount", 4),
		new QuestRaceCompleteBuilder("racecomplete", 4),
		new QuestDifficultyBuilder("difficulty", 5),
		new QuestFriendBuilder("friend", 2),
		new QuestTrackCountBuilder("trackcount", 2),
		new QuestCupCountBuilder("cupcount", 2),
		new QuestMapCupCountBuilder("mapcupcount", 4),
		new QuestMultiMapCupCountBuilder("multimapcupcount", 3)
	};
}
