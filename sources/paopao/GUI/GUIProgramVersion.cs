using System;
using UnityEngine;

public class GUIProgramVersion : MonoBehaviour
{
	private void Start()
	{
		this.style_.normal.textColor = new Color(0f, 0f, 0f);
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
	}

	private GUIStyle style_ = new GUIStyle();
}
