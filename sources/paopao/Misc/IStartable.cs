using System;

public interface IStartable
{
	StartableState State { get; }

	Exception Error { get; }
}
