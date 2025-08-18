using System;

public class QuestBuilder
{
	public QuestBuilder(string key, int paramCnt)
	{
		this.key_ = key;
		this.paramCount_ = paramCnt;
	}

	public bool IsRightFormat(string[] param)
	{
		return param.Length == this.paramCount_ && string.Compare(param[0], this.key_, true) == 0;
	}

	public virtual QuestBase Build(string[] param)
	{
		return null;
	}

	private string key_;

	private int paramCount_;
}
