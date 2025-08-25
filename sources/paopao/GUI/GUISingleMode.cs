using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GUISingleMode : FiaGUILayer
{
	protected override void FirstUpdate()
	{
		GUIAccomplishPopup.OpenStaticPopup();
		GUISingleMode.isCheckYearMonthReset_ = true;
		GUISingleMode.isCheckUpdateRanking_ = true;
	}

	protected override void AfterPanelUpdate()
	{
		if ((StageController.Instance.InputAutority & 3) != 0)
		{
			if (GUISingleMode.isCheckYearMonthReset_)
			{
				if (PlayerPrefs.HasKey("YEARMONTH_RESET"))
				{
					MonoBehaviourMessage1Param<int> monoBehaviourMessage1Param = (MonoBehaviourMessage1Param<int>)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE);
					MonoBehaviourExCenter.Instance.SendMessage(0, 267, monoBehaviourMessage1Param.Initialize(1));
					PlayerPrefs.DeleteKey("YEARMONTH_RESET");
				}
				GUISingleMode.isCheckYearMonthReset_ = false;
			}
			else if (GUISingleMode.isCheckUpdateRanking_)
			{
				if (PlayerPrefs.HasKey("UPDATE_RANKING"))
				{
					MonoBehaviourMessage message = MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.UPDATE_RANKING);
					MonoBehaviourExCenter.Instance.SendMessage(0, 1025, message);
					PlayerPrefs.DeleteKey("UPDATE_RANKING");
				}
				GUISingleMode.isCheckUpdateRanking_ = false;
			}
		}
	}

	public static bool isCheckYearMonthReset_;

	public static bool isCheckUpdateRanking_;
}
