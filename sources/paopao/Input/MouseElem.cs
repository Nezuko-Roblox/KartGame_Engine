using System;

public class MouseElem
{
	public MouseElem(int key, MouseNotifier notifier)
	{
		this.key_ = key;
		this.notifier_ = notifier;
	}

	public int key_;

	public MouseNotifier notifier_;
}
