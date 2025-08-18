using System;

public abstract class Response
{
	public Exception Error
	{
		get
		{
			return this.error_;
		}
		set
		{
			this.error_ = value;
		}
	}

	protected Exception error_;
}
