using System;
using UnityEngine;

public class HUDFPS : MonoBehaviour
{
	private void Start()
	{
		if (!base.guiText)
		{
			base.enabled = false;
			return;
		}
		this.timeleft = this.updateInterval;
	}

	private void Update()
	{
		this.timeleft -= Time.deltaTime;
		this.accum += Time.timeScale / Time.deltaTime;
		this.frames++;
		if ((double)this.timeleft <= 0.0)
		{
			float num = this.accum / (float)this.frames;
			string text = string.Format("{0:F2} FPS", num);
			string text2 = string.Format("sysram: {0:F2}", SystemInfo.systemMemorySize);
			string text3 = string.Format("vram: {0:F2}", SystemInfo.graphicsMemorySize);
			string text4 = string.Format("x: {0:F6}", Input.acceleration.x);
			string text5 = string.Format("y: {0:F6}", Input.acceleration.y);
			string text6 = string.Format("z: {0:F6}", Input.acceleration.z);
			base.guiText.text = string.Concat(new string[]
			{
				text, "\n", text2, "\n", text3, "\n", text4, "\n", text5, "\n",
				text6
			});
			base.guiText.material.color = Color.red;
			this.timeleft = this.updateInterval;
			this.accum = 0f;
			this.frames = 0;
		}
	}

	public float updateInterval = 0.5f;

	private float accum;

	private int frames;

	private float timeleft;
}
