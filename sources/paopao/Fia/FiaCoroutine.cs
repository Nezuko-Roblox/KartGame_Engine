using System;
using System.Collections;
using UnityEngine;

public class FiaCoroutine : IEnumerator
{
	public FiaCoroutine(IEnumerator coroutine)
	{
		this.coroutine_ = coroutine;
		this.onSuccess_ = null;
		this.onSuccessWith_ = null;
		this.str_ = null;
		this.onFailure_ = null;
		this.type_ = FiaCoroutine.CoroutineType.NULL;
	}

	public FiaCoroutine(IEnumerator coroutine, OnSuccess onSuccess, OnFailure onFailure)
	{
		this.coroutine_ = coroutine;
		this.onSuccess_ = onSuccess;
		this.onSuccessWith_ = null;
		this.str_ = null;
		this.onFailure_ = onFailure;
		this.type_ = FiaCoroutine.CoroutineType.ON_SUCCESS;
	}

	public FiaCoroutine(IEnumerator coroutine, OnSuccessWith onSuccessWith, string str, OnFailure onFailure)
	{
		this.coroutine_ = coroutine;
		this.onSuccess_ = null;
		this.onSuccessWith_ = onSuccessWith;
		this.str_ = str;
		this.onFailure_ = onFailure;
		this.type_ = FiaCoroutine.CoroutineType.ON_SUCCESS_WITH;
	}

	public bool MoveNext()
	{
		bool flag2;
		try
		{
			bool flag = this.coroutine_.MoveNext();
			if (!flag)
			{
				if (this.type_ == FiaCoroutine.CoroutineType.ON_SUCCESS)
				{
					this.onSuccess_();
				}
				else if (this.type_ == FiaCoroutine.CoroutineType.ON_SUCCESS_WITH)
				{
					this.onSuccessWith_(this.str_);
				}
			}
			flag2 = flag;
		}
		catch (Exception ex)
		{
			if (this.type_ == FiaCoroutine.CoroutineType.NULL)
			{
				Debug.Log("FiaCoroutine Exception: " + ex);
			}
			else
			{
				this.onFailure_(ex);
			}
			flag2 = false;
		}
		return flag2;
	}

	public object Current
	{
		get
		{
			return this.coroutine_.Current;
		}
	}

	public void Reset()
	{
		this.coroutine_.Reset();
	}

	private IEnumerator coroutine_;

	private OnSuccess onSuccess_;

	private OnSuccessWith onSuccessWith_;

	private string str_;

	private OnFailure onFailure_;

	private FiaCoroutine.CoroutineType type_;

	private enum CoroutineType
	{
		NULL,
		ON_SUCCESS,
		ON_SUCCESS_WITH
	}
}
