using System;
using UnityEngine;

public class RankingParameter
{
	public static bool UpdateRanking
	{
		get
		{
			return PlayerPrefs.HasKey("UPDATE_RANKING");
		}
		set
		{
			if (!value)
			{
				PlayerPrefs.DeleteKey("UPDATE_RANKING");
			}
			else
			{
				PlayerPrefs.SetInt("UPDATE_RANKING", 1);
			}
		}
	}

	public static bool ResetYearMonth
	{
		get
		{
			return PlayerPrefs.HasKey("YEARMONTH_RESET");
		}
		set
		{
			if (!value)
			{
				PlayerPrefs.DeleteKey("YEARMONTH_RESET");
			}
			else
			{
				PlayerPrefs.SetInt("YEARMONTH_RESET", 1);
			}
		}
	}

	public static string LastYearMonth
	{
		get
		{
			string @string = PlayerPrefs.GetString("YEARMONTH_LAST");
			if (@string == string.Empty)
			{
				return null;
			}
			return @string;
		}
		set
		{
			if (value == string.Empty || value == null)
			{
				PlayerPrefs.DeleteKey("YEARMONTH_LAST");
			}
			else
			{
				PlayerPrefs.SetString("YEARMONTH_LAST", value);
			}
		}
	}

	public static Product SelectedProduct
	{
		get
		{
			string @string = PlayerPrefs.GetString("SELECTED_PRODUCT");
			if (@string == string.Empty)
			{
				return null;
			}
			return FiaStore.Inst.FindProductWithID(@string);
		}
		set
		{
			if (value == null)
			{
				PlayerPrefs.DeleteKey("SELECTED_PRODUCT");
			}
			else
			{
				PlayerPrefs.SetString("SELECTED_PRODUCT", value.ID);
			}
		}
	}

	public static void Reset()
	{
		RankingParameter.UpdateRanking = false;
		RankingParameter.ResetYearMonth = false;
		RankingParameter.LastYearMonth = null;
	}
}
