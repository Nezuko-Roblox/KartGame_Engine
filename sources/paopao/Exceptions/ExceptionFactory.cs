using System;
using System.Collections.Generic;

public class ExceptionFactory
{
	public static Dictionary<string, Type> NameToType
	{
		get
		{
			if (ExceptionFactory.nameToType_ == null)
			{
				ExceptionFactory.nameToType_ = new Dictionary<string, Type>();
				foreach (Type type in ExceptionFactory.exceptionTypes_)
				{
					ExceptionFactory.nameToType_.Add(type.ToString(), type);
				}
			}
			return ExceptionFactory.nameToType_;
		}
	}

	public static bool IsFiaException(Type type)
	{
		return Array.IndexOf<Type>(ExceptionFactory.exceptionTypes_, type) >= 0;
	}

	public static Type FiaExceptionFromTypeName(string name)
	{
		if (ExceptionFactory.NameToType.ContainsKey(name))
		{
			return ExceptionFactory.NameToType[name];
		}
		return null;
	}

	private static ExceptionFactory inst_;

	private static Type[] exceptionTypes_ = new Type[]
	{
		typeof(RequestException),
		typeof(ServerException),
		typeof(FacebookAuthException),
		typeof(FacebookCanceledException),
		typeof(FiaAuthException),
		typeof(XMLParsingException),
		typeof(InvalidRecordException),
		typeof(UnknownException)
	};

	private static Dictionary<string, Type> nameToType_;
}
