using UnityEngine;
using System;

public struct ObsecuredInt {

	public static string KEY = "";
	public static byte[] CRYPT = new byte[16];

	public ObsecuredInt(int value)
	{
		Value = value;
	}

	public static implicit operator ObsecuredInt(int value)
	{
		return new ObsecuredInt (value);
	}

	public static ObsecuredInt operator+(ObsecuredInt lhs, int add)
	{
		return new ObsecuredInt (lhs.Value + add);
	}

	public static ObsecuredInt operator++(ObsecuredInt orig)
	{
		return new ObsecuredInt (orig.Value + 1);
	}

	private static void GenerateKey()
	{
		int i;
		for (i=0; i<10; ++i) {
			KEY += (UnityEngine.Random.Range(0, 26) + 'A');
		}

		for (i=0; i<CRYPT.Length; ++i) {
			CRYPT[i] = (byte)UnityEngine.Random.Range (0, 255);
		}
	}

	public int Value
	{
		get
		{
			byte[] data = Convert.FromBase64String(m_gut);
			for( int i=0; i<data.Length; ++i )
			{
				data[i] ^= CRYPT[i & 15];
			}
			string origin = System.Text.ASCIIEncoding.ASCII.GetString(data);
			if( origin.StartsWith(KEY) )
			{
				string val = origin.Remove(0, KEY.Length);
				int res;
				if( int.TryParse(val, out res) )
				{
					return res;
				}
			}

			return 0;
		}

		set
		{
			if (KEY.Length < 1) {
				GenerateKey();
			}

			byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(KEY + value.ToString ());
			for( int i=0; i<data.Length; ++i )
			{
				data[i] ^= CRYPT[i & 15];
			}
			m_gut = Convert.ToBase64String(data);
		}
	}

	private string m_gut;
}
