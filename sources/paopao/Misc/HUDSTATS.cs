using System;
using UnityEngine;

public class HUDSTATS : MonoBehaviour
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
			InGameStatistics instance = InGameStatistics.Instance;
			string text = "\tTotal\tEffective";
			string text2 = string.Empty;
			for (int i = 0; i < 10; i++)
			{
				string text3 = text2;
				text2 = string.Concat(new object[]
				{
					text3,
					(GameItem)i,
					": ",
					instance.TotalItemUsage.Stat((GameItem)i),
					"\t",
					instance.EffectiveItemUsage.Stat((GameItem)i),
					"\n"
				});
			}
			string text4 = "ShakeBooster: " + instance.Others.ShakeBooster;
			string text5 = "DriftBooster: " + instance.Others.DriftBooster;
			string text6 = "Collision: " + instance.Others.Collision;
			base.guiText.text = string.Concat(new string[]
			{
				iPhoneSettings.model,
				"\n",
				text,
				"\n",
				text2,
				"\n\nOthers\n",
				text4,
				"\n",
				text5,
				"\n",
				text6,
				"\n"
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
