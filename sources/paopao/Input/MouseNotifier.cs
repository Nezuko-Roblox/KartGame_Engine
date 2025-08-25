using System;
using UnityEngine;

public interface MouseNotifier
{
	bool Contains(Vector3 pos);

	int GetPriority();

	bool IsEnabled();
}
