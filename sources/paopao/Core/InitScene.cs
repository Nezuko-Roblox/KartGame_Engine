using System;
using UnityEngine;

public class InitScene : MonoBehaviour
{
	private void Start()
	{
		Debug.Log(">>>>>> jaeduk > Start()" + NativeHelper.buildType);
		this.armFlag = 0;
		if (NativeHelper.buildType == "SKT" || NativeHelper.buildType == "LGT")
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
			{
				int num = androidJavaClass.CallStatic<int>("checkArmService", new object[0]);
			}
		}
		else
		{
			this.armServiceVerified();
		}
	}

	private void armServiceVerified()
	{
		this.lastInterval = (double)Time.realtimeSinceStartup;
		this.armFlag = 1;
	}

	private void Update()
	{
		if (this.armFlag == 0)
		{
			return;
		}
		if (Array.IndexOf<iPhoneGeneration>(InitScene.IGNORED_GENERATIONS, iPhoneSettings.generation) >= 0)
		{
			GUIUtil.Localize(this.notsupported_.GetComponent<GUITexture>());
			this.notsupported_.SetActiveRecursively(true);
		}
		else
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if ((double)realtimeSinceStartup > this.lastInterval + 2.5)
			{
				KartManager.Instance.parameter_.Stage = StageType.NONE;
				Application.LoadLevel("track_loading");
			}
			else
			{
				this.replayLogo_.SetActiveRecursively(true);
			}
		}
	}

	public GameObject notsupported_;

	public GameObject replayLogo_;

	private double lastInterval;

	private int armFlag;

	private static iPhoneGeneration[] IGNORED_GENERATIONS = new iPhoneGeneration[]
	{
		iPhoneGeneration.iPhone,
		iPhoneGeneration.iPhone3G,
		iPhoneGeneration.iPodTouch1Gen,
		iPhoneGeneration.iPodTouch2Gen
	};
}
