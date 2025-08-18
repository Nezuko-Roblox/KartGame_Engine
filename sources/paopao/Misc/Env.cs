using System;
using System.IO;
using UnityEngine;

public class Env
{
	public static bool IsDesktop
	{
		get
		{
			foreach (RuntimePlatform runtimePlatform in Env.DesktopPlatforms)
			{
				if (Application.platform == runtimePlatform)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsIPhoneHighRes
	{
		get
		{
			return !Env.IsDesktop && (Screen.width == 640 || Screen.height == 640);
		}
	}

	public static bool IsIPhoneLowRes
	{
		get
		{
			return !Env.IsDesktop && (Screen.width == 320 || Screen.height == 320);
		}
	}

	public static bool IsAndroid
	{
		get
		{
			return RuntimePlatform.Android == Application.platform;
		}
	}

	public static bool IsIPhone
	{
		get
		{
			return Env.IsIPhoneHighRes || Env.IsIPhoneLowRes;
		}
	}

	public static bool IsIPad
	{
		get
		{
			return !Env.IsDesktop && (Screen.width == 768 || Screen.height == 768);
		}
	}

	public static bool IsIPad2
	{
		get
		{
			return !Env.IsDesktop && iPhoneSettings.generation == iPhoneGeneration.Unknown;
		}
	}

	public static bool IsIPhoneRes
	{
		get
		{
			return Screen.width == 320 || Screen.height == 320;
		}
	}

	public static bool IsIPadRes
	{
		get
		{
			return Screen.width == 768 || Screen.height == 768;
		}
	}

	public static bool SupportsIOSRuntime
	{
		get
		{
			return false;
		}
	}

	public static bool IsConnectedToInternet
	{
		get
		{
			return true;
		}
	}

	public static string DocPath
	{
		get
		{
			return Path.Combine(Path.GetDirectoryName(Application.dataPath), "Documents");
		}
	}

	public static string RecordPath
	{
		get
		{
			return Path.Combine(Env.DocPath, "record");
		}
	}

	private static RuntimePlatform[] DesktopPlatforms = new RuntimePlatform[]
	{
		RuntimePlatform.OSXEditor,
		RuntimePlatform.OSXPlayer,
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.WindowsPlayer
	};
}
