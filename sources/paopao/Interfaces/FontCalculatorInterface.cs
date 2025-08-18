using System;
using UnityEngine;

public interface FontCalculatorInterface
{
	void SetUV(Rect uv);

	bool GetUV(int charIdx, ref Vector2[] uvs, int idx);
}
