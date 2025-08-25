using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Product
{
	public string ID { get; set; }

	public string LocalizedTitle { get; set; }

	public string LocalizedDescription { get; set; }

	public string PriceString { get; set; }

	public string PriceLocale { get; set; }

	public float Price { get; set; }

	public int Index { get; set; }

	public int IconIndex { get; set; }

	public ProductType Type { get; set; }

	public string[] Unlocks { get; set; }

	public string Info { get; set; }

	public static List<Product> FromJSON(JSONObject productList)
	{
		Dictionary<string, Product> dictionary = Product.DefaultProductDict();
		List<Product> list = new List<Product>();
		try
		{
			for (int i = 0; i < productList.list.Count; i++)
			{
				JSONObject jsonobject = (JSONObject)productList.list[i];
				if (jsonobject.type != JSONObject.Type.NULL)
				{
					try
					{
						Product product = new Product();
						int j = 0;
						while (j < jsonobject.list.Count)
						{
							JSONObject jsonobject2 = (JSONObject)jsonobject.list[j];
							string text = (string)jsonobject.keys[j];
							switch (text)
							{
							case "productID":
								product.ID = jsonobject2.str;
								break;
							case "localizedTitle":
								product.LocalizedTitle = jsonobject2.str;
								break;
							case "localizedDescription":
								product.LocalizedDescription = jsonobject2.str;
								break;
							case "priceString":
								product.PriceString = jsonobject2.str;
								break;
							case "priceLocale":
								product.PriceLocale = jsonobject2.str;
								break;
							case "price":
								product.Price = (float)jsonobject2.n;
								break;
							}
							IL_0185:
							j++;
							continue;
							goto IL_0185;
						}
						Product product2 = dictionary[product.ID];
						product.Index = product2.Index;
						product.Type = product2.Type;
						product.Unlocks = product2.Unlocks;
						product.IconIndex = product2.IconIndex;
						product.Info = product2.Info;
						list.Add(product);
					}
					catch (Exception ex)
					{
					}
				}
			}
			foreach (Product product3 in dictionary.Values)
			{
				bool flag = true;
				foreach (Product product4 in list)
				{
					if (product4.ID == product3.ID)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					list.Add(product3);
				}
			}
		}
		catch (Exception ex2)
		{
		}
		return list;
	}

	public static JSONObject ToJSON(List<Product> productList)
	{
		JSONObject jsonobject = new JSONObject();
		jsonobject.type = JSONObject.Type.ARRAY;
		jsonobject.list = new ArrayList();
		foreach (Product product in productList)
		{
			JSONObject jsonobject2 = new JSONObject();
			jsonobject2.type = JSONObject.Type.OBJECT;
			jsonobject2.keys = new ArrayList();
			jsonobject2.list = new ArrayList();
			jsonobject2.keys.Add("productID");
			jsonobject2.list.Add(JSONObject.StringType(product.ID));
			jsonobject2.keys.Add("localizedDescription");
			jsonobject2.list.Add(JSONObject.StringType(product.LocalizedDescription));
			jsonobject2.keys.Add("priceString");
			jsonobject2.list.Add(JSONObject.StringType(product.PriceString));
			jsonobject2.keys.Add("priceLocale");
			jsonobject2.list.Add(JSONObject.StringType(product.PriceLocale));
			jsonobject2.keys.Add("price");
			jsonobject2.list.Add(new JSONObject(product.Price));
			jsonobject.list.Add(jsonobject2);
		}
		return jsonobject;
	}

	public static Dictionary<string, Product> DefaultProductDict()
	{
		Dictionary<string, Product> dictionary = new Dictionary<string, Product>();
		foreach (Product product in Product.DefaultProductList())
		{
			dictionary.Add(product.ID, product);
		}
		return dictionary;
	}

	public static List<Product> DefaultProductList()
	{
		List<Product> list = new List<Product>();
		TextAsset textAsset;
		if (NativeHelper.buildType == "LGT")
		{
			textAsset = (TextAsset)Resources.Load("productdefinitionlgt");
		}
		else
		{
			textAsset = (TextAsset)Resources.Load("productdefinition");
		}
		XMLElement xmlelement = new XMLElement();
		xmlelement.parseString(textAsset.text);
		foreach (object obj in xmlelement.getChildren())
		{
			XMLElement xmlelement2 = (XMLElement)obj;
			Product product = new Product();
			product.ID = (string)xmlelement2.getAttribute("id");
			product.Index = int.Parse((string)xmlelement2.getAttribute("index"));
			product.Type = (ProductType)int.Parse((string)xmlelement2.getAttribute("type"));
			product.LocalizedTitle = (string)xmlelement2.getAttribute("title");
			product.Price = float.Parse((string)xmlelement2.getAttribute("price"));
			product.PriceString = (string)xmlelement2.getAttribute("price");
			product.Info = (string)xmlelement2.getAttribute("info");
			if (product.Type == ProductType.BUNDLE_SET)
			{
				product.Unlocks = ((string)xmlelement2.getAttribute("unlocks")).Split(new char[] { ' ' });
			}
			else
			{
				product.Unlocks = new string[] { product.ID };
			}
			product.IconIndex = int.Parse((string)xmlelement2.getAttribute("iconIndex"));
			list.Add(product);
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("<Product ID={0} LocalizedTitle={1} LocalizedDescription={2} PriceString={3} PriceLocale={4} Price={5} Index = {6}", new object[] { this.ID, this.LocalizedTitle, this.LocalizedDescription, this.PriceString, this.PriceLocale, this.Price, this.Index });
	}
}
