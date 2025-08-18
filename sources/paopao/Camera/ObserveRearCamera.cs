using System;
using UnityEngine;

public class ObserveRearCamera : MonoBehaviour
{
	private void Start()
	{
		base.camera.pixelRect = new Rect((float)(Screen.width / 2 - 150), (float)(Screen.height - 75 - 20), 300f, 75f);
	}

	private void Update()
	{
	}
}
