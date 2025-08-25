using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class MockFiaStore : FiaStore
{
	public string Locale
	{
		get
		{
			return this.locale_;
		}
		set
		{
			this.locale_ = value;
		}
	}

	public new static FiaStore Inst
	{
		get
		{
			if (FiaStore.store_ == null)
			{
				FiaStore.store_ = new MockFiaStore();
			}
			return FiaStore.store_;
		}
	}

	public override IEnumerator RequestProductInfo()
	{
		base.State = StartableState.RUNNING;
		this.locale_ = this.locales_[9];
		string path = FiaUtil.docPath;
		path = Path.Combine(path, "mockstore");
		path = Path.Combine(path, this.locale_ + ".txt");
		using (StreamReader reader = new StreamReader(path))
		{
			string tmp = reader.ReadToEnd();
			FiaStore.RequestProductInfoSuccess(tmp);
		}
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.Error != null)
		{
			throw this.Error;
		}
		yield break;
	}

	public override IEnumerator Purchase(string productID)
	{
		base.State = StartableState.RUNNING;
		FiaStore.PurchaseSuccess(productID);
		while (this.State == StartableState.RUNNING)
		{
			yield return new WaitForSeconds(0.5f);
		}
		if (this.Error != null)
		{
			throw this.Error;
		}
		yield break;
	}

	private string[] locales_ = new string[]
	{
		"en_AU", "en_CA", "en_CH", "en_DK", "en_GB", "en_HK", "en_JP", "en_NO", "en_NZ", "en_US",
		"es_MX", "fr_FR", "sv_SE"
	};

	private string locale_;
}
