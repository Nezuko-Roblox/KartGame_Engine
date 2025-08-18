using System;
using UnityEngine;

public class FiaStoreTest : MonoBehaviour
{
	private void Awake()
	{
	}

	private void SetupProductInfo()
	{
		if (!this.store.LoadProductInfoList())
		{
			this.store.LoadDefaultProductInfo();
		}
		base.StartCoroutine(new FiaCoroutine(this.store.RequestProductInfo(), new OnSuccess(this.RequestProductInfoSuccess), new OnFailure(this.RequestProductInfoFailure)));
	}

	private void OnGUI()
	{
		if (this.store == null)
		{
			if (GUI.Button(new Rect(60f, 0f, 200f, 50f), "Use Fia Store"))
			{
				this.store = FiaStore.Inst;
				this.SetupProductInfo();
			}
			if (GUI.Button(new Rect(60f, 100f, 200f, 50f), "Use Mock Fia Store"))
			{
				this.store = MockFiaStore.Inst;
				this.SetupProductInfo();
			}
		}
		else
		{
			if (GUI.Button(new Rect(60f, 0f, 200f, 50f), "Purchase bundle 0"))
			{
				FiaCoroutine fiaCoroutine = new FiaCoroutine(this.store.Purchase("bundle.0"), new OnSuccess(this.PurchaseSuccess), new OnFailure(this.PurchaseFailure));
				base.StartCoroutine(fiaCoroutine);
			}
			if (GUI.Button(new Rect(60f, 50f, 200f, 50f), "Print curr products"))
			{
				foreach (Product product in this.store.ProductInfoList)
				{
					Debug.Log(product);
				}
			}
			if (GUI.Button(new Rect(60f, 100f, 200f, 50f), "Print Bundles"))
			{
				foreach (Product product2 in this.store.FindProductsWithType(ProductType.BUNDLE))
				{
					Debug.Log(product2);
				}
			}
			if (GUI.Button(new Rect(60f, 150f, 200f, 50f), "Print Purchased Products"))
			{
				this.store.LoadPurchasedProductList();
				foreach (string text in this.store.UnlockedProductList)
				{
					Debug.Log(text);
				}
			}
		}
	}

	private void PurchaseSuccess()
	{
		Debug.Log("Purchase success!");
		foreach (string text in this.store.UnlockedProductList)
		{
			Debug.Log(text);
		}
	}

	private void PurchaseFailure(Exception ex)
	{
		Debug.Log(ex);
	}

	private void RestorePurchasesSuccess()
	{
		Debug.Log("successfully repurchased item");
		foreach (string text in this.store.UnlockedProductList)
		{
			Debug.Log(text);
		}
	}

	private void RestorePurchasesFailure(Exception ex)
	{
		Debug.Log(ex);
	}

	private void RequestProductInfoSuccess()
	{
		Debug.Log("successfully requested product info");
		foreach (Product product in this.store.ProductInfoList)
		{
			Debug.Log(product);
		}
	}

	private void RequestProductInfoFailure(Exception ex)
	{
		Debug.Log(ex);
	}

	private FiaStore store;
}
