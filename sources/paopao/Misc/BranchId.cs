using System;

public class BranchId
{
	public BranchId()
	{
	}

	public BranchId(BranchIdElem[] history, int historySize)
	{
		if (historySize <= 0)
		{
			this.history_ = null;
		}
		else
		{
			this.history_ = new BranchIdElem[historySize];
			Array.Copy(history, this.history_, historySize);
		}
	}

	public static bool IsExclusive(BranchId lparam, BranchId rparam)
	{
		if (lparam.history_ == null || rparam.history_ == null)
		{
			return false;
		}
		int num = Math.Min(lparam.history_.Length, rparam.history_.Length);
		for (int i = 0; i < num; i++)
		{
			if (lparam.history_[i].node_ != rparam.history_[i].node_)
			{
				return false;
			}
			if (lparam.history_[i].branch_ != rparam.history_[i].branch_)
			{
				return true;
			}
		}
		return false;
	}

	public override string ToString()
	{
		if (this.history_ == null)
		{
			return "No Branch";
		}
		string text = string.Empty;
		for (int i = 0; i < this.history_.Length; i++)
		{
			text += string.Format("[{0}/{1}]", this.history_[i].node_, this.history_[i].branch_);
		}
		return text;
	}

	private BranchIdElem[] history_;
}
