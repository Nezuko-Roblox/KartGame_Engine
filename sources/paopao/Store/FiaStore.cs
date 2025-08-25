using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FiaStore : IStartable
{
	protected FiaStore()
	{
	}

	protected static void _Purchase(string productID)
	{
	}

	protected static void _RestorePurchases()
	{
	}

	protected static void _RequestProductInfo(string[] productIDs)
	{
	}

	public static FiaStore Inst
	{
		get
		{
			if (FiaStore.store_ == null)
			{
				FiaStore.store_ = new FiaStore();
			}
			return FiaStore.store_;
		}
	}

	public void LoadDefaultProductInfo()
	{
		this.ProductInfoList = Product.DefaultProductList();
	}

	public virtual IEnumerator RequestProductInfo()
	{
		this.Start();
		List<string> ids = new List<string>();
		foreach (Product p in Product.DefaultProductList())
		{
			ids.Add(p.ID);
		}
		FiaStore._RequestProductInfo(ids.ToArray());
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

	public static void RequestProductInfoSuccess(string productInfoJSON)
	{
		JSONObject jsonobject = new JSONObject(productInfoJSON);
		FiaStore.Inst.ProductInfoList = Product.FromJSON(jsonobject);
		FiaStore.Inst.SaveProductInfoList();
		FiaStore.Inst.State = StartableState.WAITING;
	}

	public static void RequestProductInfoFailure(int errorCode)
	{
		switch (errorCode + 1)
		{
		case 0:
			FiaStore.Inst.Error = new UnknownException();
			break;
		case 1:
			FiaStore.Inst.Error = new CanceledException();
			break;
		case 2:
			FiaStore.Inst.Error = new ITunesStoreConnectionException();
			break;
		default:
			FiaStore.Inst.Error = new UnknownException();
			break;
		}
		FiaStore.Inst.State = StartableState.FAILED;
	}

	public Product FindProductWithID(string productID)
	{
		List<Product> list = this.ProductInfoList.FindAll((Product p) => string.Compare(p.ID, productID) == 0);
		return (list == null) ? null : list[0];
	}

	public List<Product> FindProductsWithType(ProductType type)
	{
		return this.ProductInfoList.FindAll((Product p) => p.Type == type);
	}

	public List<Product> FindUnpurchasedProductsWithType(ProductType type)
	{
		return this.ProductInfoList.FindAll((Product p) => p.Type == type && !this.PurchasedProductList.Contains(p.ID));
	}

	public virtual IEnumerator Purchase(string productID)
	{
		this.Start();
		FiaStore._Purchase(productID);
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

	public static void PurchaseSuccess(string productID)
	{
		if (!FiaStore.Inst.PurchasedProductList.Contains(productID))
		{
			FiaStore.Inst.PurchasedProductList.Add(productID);
		}
		FiaStore.Inst.SavePurchasedProductList();
		FiaStore.Inst.State = StartableState.WAITING;
	}

	public static void PurchaseFailure(int errorCode)
	{
		switch (errorCode + 1)
		{
		case 0:
			FiaStore.Inst.Error = new UnknownException();
			break;
		case 1:
			FiaStore.Inst.Error = new CanceledException();
			break;
		case 2:
			FiaStore.Inst.Error = new ITunesStoreConnectionException();
			break;
		default:
			FiaStore.Inst.Error = new UnknownException();
			break;
		}
		FiaStore.Inst.State = StartableState.FAILED;
	}

	public IEnumerator RestorePurchases()
	{
		this.Start();
		FiaStore._RestorePurchases();
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

	public static void RestorePurchasesSuccess(string productIDs)
	{
		foreach (string text in productIDs.Split(new char[] { ',' }))
		{
			string text2 = text.Trim();
			if (!FiaStore.Inst.PurchasedProductList.Contains(text2))
			{
				FiaStore.Inst.PurchasedProductList.Add(text2);
			}
		}
		FiaStore.Inst.SavePurchasedProductList();
		FiaStore.Inst.State = StartableState.WAITING;
	}

	public static void RestorePurchasesFailure(int errorCode)
	{
		switch (errorCode + 1)
		{
		case 0:
			FiaStore.Inst.Error = new UnknownException();
			break;
		case 1:
			FiaStore.Inst.Error = new CanceledException();
			break;
		case 2:
			FiaStore.Inst.Error = new ITunesStoreConnectionException();
			break;
		default:
			FiaStore.Inst.Error = new UnknownException();
			break;
		}
		FiaStore.Inst.State = StartableState.FAILED;
	}

	public List<Product> ProductInfo()
	{
		return null;
	}

	public bool SavePurchasedProductList()
	{
		using (FileStream fileStream = new FileStream(this.PurchasedProductListLocalPath, FileMode.Create))
		{
			string text = string.Join(",", this.PurchasedProductList.ToArray());
			text = iPhoneSettings.uniqueIdentifier + "," + text;
			byte[] array = Encryption.Encrypt(text);
			fileStream.Write(array, 0, array.Length);
		}
		return true;
	}

	public bool LoadPurchasedProductList()
	{
		bool flag;
		try
		{
			using (FileStream fileStream = new FileStream(this.PurchasedProductListLocalPath, FileMode.Open))
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				string text = Encryption.Decrypt(array);
				string text2 = text.Substring(0, text.IndexOf(','));
				string uniqueIdentifier = iPhoneSettings.uniqueIdentifier;
				if (string.Compare(text2, uniqueIdentifier) != 0)
				{
					throw new Exception("udid not matching");
				}
				text = text.Substring(text.IndexOf(',') + 1, text.Length - text.IndexOf(',') - 1);
				this.PurchasedProductList = new List<string>(text.Split(new char[] { ',' }));
			}
			flag = true;
		}
		catch (Exception ex)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(this.PurchasedProductListLocalPath);
				fileInfo.Delete();
			}
			catch (Exception ex2)
			{
			}
			flag = false;
		}
		return flag;
	}

	public bool SaveProductInfoList()
	{
		bool flag;
		try
		{
			using (FileStream fileStream = new FileStream(this.ProductInfoListLocalPath, FileMode.Create))
			{
				byte[] array = Encryption.Encrypt(Product.ToJSON(this.ProductInfoList).print());
				fileStream.Write(array, 0, array.Length);
			}
			flag = true;
		}
		catch (Exception ex)
		{
			flag = false;
		}
		return flag;
	}

	public bool LoadProductInfoList()
	{
		bool flag;
		try
		{
			using (FileStream fileStream = new FileStream(this.ProductInfoListLocalPath, FileMode.Open))
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, (int)fileStream.Length);
				JSONObject jsonobject = new JSONObject(Encryption.Decrypt(array));
				if (jsonobject == null)
				{
					this.RemoveFile(this.ProductInfoListLocalPath);
					return false;
				}
				this.ProductInfoList = Product.FromJSON(jsonobject);
			}
			flag = true;
		}
		catch (Exception ex)
		{
			this.RemoveFile(this.ProductInfoListLocalPath);
			flag = false;
		}
		return flag;
	}

	private void RemoveFile(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			fileInfo.Delete();
		}
		catch (Exception ex)
		{
		}
	}

	private string PurchasedProductListLocalPath
	{
		get
		{
			string text = FiaUtil.recordPath;
			text = Path.Combine(text, "ranking");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return Path.Combine(text, "plist.db");
		}
	}

	private string ProductInfoListLocalPath
	{
		get
		{
			string text = FiaUtil.recordPath;
			text = Path.Combine(text, "ranking");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return Path.Combine(text, "pcache.db");
		}
	}

	protected void Start()
	{
		this.State = StartableState.RUNNING;
		this.Error = null;
	}

	public StartableState State
	{
		get
		{
			return this.state_;
		}
		set
		{
			this.state_ = value;
		}
	}

	public Exception Error
	{
		get
		{
			return this.error_;
		}
		set
		{
			this.error_ = value;
		}
	}

	public List<string> PurchasedProductList
	{
		get
		{
			if (this.purchasedProductList_ == null)
			{
				this.purchasedProductList_ = new List<string>();
			}
			return this.purchasedProductList_;
		}
		set
		{
			this.purchasedProductList_ = value;
		}
	}

	public List<string> UnlockedProductList
	{
		get
		{
			List<string> list = new List<string>();
			foreach (Product product in this.ProductInfoList)
			{
				if (this.PurchasedProductList.Contains(product.ID))
				{
					foreach (string text in product.Unlocks)
					{
						if (!list.Contains(text))
						{
							list.Add(text);
						}
					}
				}
			}
			return list;
		}
		set
		{
			this.purchasedProductList_ = value;
		}
	}

	public List<Product> ProductInfoList
	{
		get
		{
			return this.productInfoList_;
		}
		set
		{
			this.productInfoList_ = value;
		}
	}

	private List<string> purchasedProductList_;

	private List<Product> productInfoList_;

	protected static FiaStore store_;

	private StartableState state_;

	private Exception error_;
}
