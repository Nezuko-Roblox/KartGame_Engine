using System;
using UnityEngine;

public class FiaColor
{
	public static Color clear
	{
		get
		{
			return FiaColor.clear_;
		}
	}

	public static Color white
	{
		get
		{
			return FiaColor.white_;
		}
	}

	public static Color lightGrey
	{
		get
		{
			return FiaColor.lightGrey_;
		}
	}

	public static Color lightGray
	{
		get
		{
			return FiaColor.lightGrey_;
		}
	}

	public static Color darkGrey
	{
		get
		{
			return FiaColor.darkGrey_;
		}
	}

	public static Color darkGray
	{
		get
		{
			return FiaColor.darkGrey_;
		}
	}

	public static Color grey
	{
		get
		{
			return FiaColor.grey_;
		}
	}

	public static Color gray
	{
		get
		{
			return FiaColor.grey_;
		}
	}

	public static Color black
	{
		get
		{
			return FiaColor.black_;
		}
	}

	protected static Color clear_ = Color.clear;

	protected static Color white_ = Color.white;

	protected static Color lightGrey_ = new Color(0.733333349f, 0.733333349f, 0.733333349f);

	protected static Color darkGrey_ = new Color(0.274509817f, 0.274509817f, 0.274509817f);

	protected static Color grey_ = Color.grey;

	protected static Color black_ = Color.black;
}
