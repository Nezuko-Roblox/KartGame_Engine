using System;
using UnityEngine;

public interface GUIInterface
{
	void MoveRectByWindowPos(float x, float y);

	void SetRectByWindowSpace(float x, float y);

	void RegistPanelManager(GUIPanelManager manager);

	Vector2 GetLeftTopByWindowPos();
}
