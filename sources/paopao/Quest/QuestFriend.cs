using System;
using UnityEngine;

public class QuestFriend : QuestBase
{
	public QuestFriend(int friendNo)
		: base(friendNo)
	{
	}

	public override void Refresh()
	{
		if (KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.FACEBOOK_FRIEND))
		{
			this.current_ = this.max_;
		}
		else
		{
			Facebook facebook;
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				facebook = Facebook.Inst;
			}
			else
			{
				facebook = MockFacebook.Inst;
			}
			this.current_ = ((!facebook.LoggedIn) ? 0 : Mathf.Clamp(facebook.FriendDict.Count - 1, 0, this.max_));
		}
	}
}
