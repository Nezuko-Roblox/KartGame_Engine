using System;
using UnityEngine;

public class RecordTest : MonoBehaviour
{
	private void Awake()
	{
		float num = 98.25f;
		int num2 = (int)num / 60;
		int num3 = (int)num % 60;
		int num4 = (int)((num - (float)((int)num)) * 100f);
		string text = string.Format("{0}:{1:00}:{2:00}", num2, num3, num4);
		Debug.Log(text);
	}
}
