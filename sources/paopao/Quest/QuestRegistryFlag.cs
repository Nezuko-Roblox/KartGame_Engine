using System;

public class QuestRegistryFlag : QuestBase
{
	public QuestRegistryFlag(uint flag)
		: base(0)
	{
		this.flag_ = flag;
	}

	public override void Refresh()
	{
		uint quest = (uint)KartOptions.Instance.Quest;
		this.max_ = 0;
		this.current_ = 0;
		for (int i = 0; i < 32; i++)
		{
			uint num = 1U << i;
			if ((num & this.flag_) != 0U)
			{
				this.max_++;
				if ((num & quest) != 0U)
				{
					this.current_++;
				}
			}
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		for (int i = 0; i < 32; i++)
		{
			int num = 1 << i;
			if (((long)num & (long)((ulong)this.flag_)) != 0L)
			{
				text += ((KartOptions.QuestFlag)num).ToString();
				text += "\n";
			}
		}
		return text;
	}

	protected uint flag_;
}
