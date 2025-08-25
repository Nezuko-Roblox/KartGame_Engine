using System;
using UnityEngine;

public class ObserveSubCamera : MonoBehaviour
{
	private void Start()
	{
		base.camera.pixelRect = new Rect((float)(Screen.width - 100 - 40), (float)(Screen.height - 100 - 40), 100f, 100f);
	}

	private void Update()
	{
	}
}
