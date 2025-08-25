using System;
using System.IO;
using System.Security.Cryptography;

public class Encryption
{
	public static byte[] Encrypt(string original)
	{
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		byte[] array = Encryption.EncryptString(original, rijndaelManaged.Key, rijndaelManaged.IV);
		byte[] array2 = new byte[rijndaelManaged.IV.Length + array.Length + rijndaelManaged.Key.Length];
		Buffer.BlockCopy(rijndaelManaged.IV, 0, array2, 0, rijndaelManaged.IV.Length);
		Buffer.BlockCopy(array, 0, array2, 16, array.Length);
		Buffer.BlockCopy(rijndaelManaged.Key, 0, array2, 16 + array.Length, rijndaelManaged.Key.Length);
		byte[] array3 = new HMACMD5(rijndaelManaged.Key).ComputeHash(array2);
		byte[] array4 = new byte[array3.Length + array2.Length];
		Buffer.BlockCopy(array3, 0, array4, 0, array3.Length);
		Buffer.BlockCopy(array2, 0, array4, array3.Length, array2.Length);
		return array4;
	}

	public static string Decrypt(byte[] ciek)
	{
		byte[] array = new byte[16];
		byte[] array2 = new byte[16];
		byte[] array3 = new byte[ciek.Length - 64];
		byte[] array4 = new byte[32];
		byte[] array5 = new byte[ciek.Length - 16];
		Buffer.BlockCopy(ciek, 0, array, 0, 16);
		Buffer.BlockCopy(ciek, 16, array2, 0, 16);
		Buffer.BlockCopy(ciek, 32, array3, 0, ciek.Length - 64);
		Buffer.BlockCopy(ciek, ciek.Length - 32, array4, 0, 32);
		Buffer.BlockCopy(ciek, 16, array5, 0, ciek.Length - 16);
		byte[] array6 = new HMACMD5(array4).ComputeHash(array5);
		for (int i = 0; i < 16; i++)
		{
			if (array[i] != array6[i])
			{
				throw new ArgumentException("Checksum does not match");
			}
		}
		RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.IV = array2;
		rijndaelManaged.Key = array4;
		return Encryption.DecryptString(array3, rijndaelManaged.Key, rijndaelManaged.IV);
	}

	private static byte[] EncryptString(string plainText, byte[] Key, byte[] IV)
	{
		if (plainText == null || plainText.Length <= 0)
		{
			throw new ArgumentNullException("plainText");
		}
		if (Key == null || Key.Length <= 0)
		{
			throw new ArgumentNullException("Key");
		}
		if (IV == null || IV.Length <= 0)
		{
			throw new ArgumentNullException("IV");
		}
		MemoryStream memoryStream = null;
		RijndaelManaged rijndaelManaged = null;
		try
		{
			rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = Key;
			rijndaelManaged.IV = IV;
			ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(rijndaelManaged.Key, rijndaelManaged.IV);
			memoryStream = new MemoryStream();
			using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
				{
					streamWriter.Write(plainText);
				}
			}
		}
		finally
		{
			if (rijndaelManaged != null)
			{
				rijndaelManaged.Clear();
			}
		}
		return memoryStream.ToArray();
	}

	private static string DecryptString(byte[] cipherText, byte[] Key, byte[] IV)
	{
		if (cipherText == null || cipherText.Length <= 0)
		{
			throw new ArgumentNullException("cipherText");
		}
		if (Key == null || Key.Length <= 0)
		{
			throw new ArgumentNullException("Key");
		}
		if (IV == null || IV.Length <= 0)
		{
			throw new ArgumentNullException("IV");
		}
		RijndaelManaged rijndaelManaged = null;
		string text = null;
		try
		{
			rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Key = Key;
			rijndaelManaged.IV = IV;
			ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(rijndaelManaged.Key, rijndaelManaged.IV);
			using (MemoryStream memoryStream = new MemoryStream(cipherText))
			{
				using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
				{
					using (StreamReader streamReader = new StreamReader(cryptoStream))
					{
						text = streamReader.ReadToEnd();
					}
				}
			}
		}
		finally
		{
			if (rijndaelManaged != null)
			{
				rijndaelManaged.Clear();
			}
		}
		return text;
	}
}
