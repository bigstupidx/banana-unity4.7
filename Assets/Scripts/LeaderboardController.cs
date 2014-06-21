using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class LeaderboardController : MonoBehaviour {

	public static LeaderboardController g_instance = null;
	
	void Awake()
	{
		g_instance = this;
		m_isWorking = false;
	}
	
	public static LeaderboardController Instance
	{
		get { return g_instance; }
	}
	
	private class Entry
	{
		public string id;
		public string name;
		public int score;
		public int rank;
		public bool isMine;
		
		public Entry(string nid, string nname, int nscore)
		{
			id = nid;
			name = nname;
			score = nscore;
			rank = 0;
			isMine = false;
		}
	}
	
	public bool HasInfo = false;
	public bool GotError = false;
	
	private bool m_isRequesting = false;
	private bool m_isWorking = false;
	
	private int m_myRank;
	private List<Entry> m_topEntries = new List<Entry>();
	private List<Entry> m_downEntries = new List<Entry>();
	private List<Entry> m_upEntries = new List<Entry>();
	
	public void Request()
	{
		if( m_isWorking )
		{
			return;
		}
	
		m_isRequesting = true;
	}
	
	void LateUpdate()
	{
		if( m_isRequesting )
		{
			if( !FacebookController.Instance.HasMyInfo )
			{
				return;
			}	
			
			if( PlayerStash.Instance.HighScore < 1 )
			{
				m_isRequesting = false;
				return;
			}
			
			m_isRequesting = false;
			Refresh();
		}
	}
	
	private void Refresh()
	{
		m_isWorking = true;
		
		HasInfo = false;
		GotError = false;
		
		m_myRank = 0;
		m_topEntries.Clear();
		m_downEntries.Clear();
		m_upEntries.Clear();		
		
		StartCoroutine(Connect());
	}
	
	private string EncryptURL(string blob, byte[] passwordKey)
	{
		byte[] blobData = System.Text.ASCIIEncoding.ASCII.GetBytes(blob);
		Rijndael alg = Rijndael.Create(); 		
		alg.Mode = CipherMode.CBC;
		alg.BlockSize = 256;		
		alg.GenerateIV();
		
		alg.Key = passwordKey;	
		
		ICryptoTransform cTransform = alg.CreateEncryptor();
		byte[] resultArray = cTransform.TransformFinalBlock(blobData, 0, blobData.Length);
		string cipherBlob = Convert.ToBase64String(resultArray);
		string base64iv = Convert.ToBase64String(alg.IV);
		string cipherKey = Convert.ToBase64String(alg.Key);
		
		return "a=" + WWW.EscapeURL(cipherBlob) + "&b=" + WWW.EscapeURL(base64iv) + "&c=" + WWW.EscapeURL(cipherKey);
	}
	
	public IEnumerator Connect()
	{	
		byte[] password = System.Text.ASCIIEncoding.ASCII.GetBytes(FacebookController.Instance.MyID);
		byte[] salt = new byte[] { 0x6, 0x4, 0x19, 0x89, 0x30, 0x04, 0x19, 0x79};		
		PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA1", 1000);
		byte[] passwordKey = pdb.GetBytes(32);		
		
		// Encrypt data blob
		string blob = WWW.EscapeURL(FacebookController.Instance.MyID) + " | " + WWW.EscapeURL(PlayerStash.Instance.HighScore.ToString()) + " | " + WWW.EscapeURL(FacebookController.Instance.MyFullName) + " | " + Convert.ToBase64String(passwordKey) + " | CHECK";	
		string post_url = "kinoastudios.com/CastleDefender/announce.php?" + EncryptURL(blob, passwordKey);
		
		Debug.Log("" + post_url);
		
		// Post the URL to the site and create a download object to get the result.
		WWW hs_post = new WWW(post_url);
		yield return hs_post; // Wait until the download is done
		
		if (hs_post.error != null)
		{
			GotError = true;
		}
		else
		{
			string[] lines = hs_post.text.Split('\n');
			int proc = -1;
			foreach( string line in lines )
			{
				if( line.Equals("TOP") )
				{
					proc = 0;
				}
				else if( line.Equals("DOWN") )
				{
					proc = 1;
				}
				else if( line.Equals("UP") )
				{
					proc = 2;
				}
				else if( line.Equals("RANK") )
				{
					proc = 3;
				}
				else
				{
					switch( proc )
					{
					case 0:
						AddToEntry(m_topEntries, line);
						break;
					case 1:
						AddToEntry(m_downEntries, line);
						break;
					case 2:
						AddToEntry(m_upEntries, line);
						break;
					case 3:
						int.TryParse(line, out m_myRank);
						proc = -1;
						break;
					}
				}
			}
		
			// Validate result
			if( m_myRank > 0 )
			{
				int i;
				
				for( i=0; i<m_upEntries.Count; ++i )		
				{
					int r = m_myRank - i - 1;
					m_upEntries[i].rank = r > 0 ? r : 1;
				}
				
				for( i=0; i<m_downEntries.Count; ++i )		
				{
					m_downEntries[i].rank = m_myRank + i + 1;
				}
				
				HasInfo = true;
			}
			else
			{
				GotError = true;
			}
		}
	
		m_isWorking = false;
		yield return null;
	}
	
	private bool AddToEntry(List<Entry> list, string line)
	{
		int score = 0;
		string[] args = line.Split('\t');
		if( args.Length >= 3 )
		{
			if( args[0].Length > 0 && args[1].Length > 0 && int.TryParse(args[2], out score) && score > 0 )
			{
				Entry e = new Entry(args[0], args[1], score);
				if( e.id.Equals(FacebookController.Instance.MyID) )
				{
					e.isMine = true;
				}
				
				list.Add(e);
				return true;
			}
		}
		
		return false;
	}
	
	public void PopulateTopTen(ScoreEntryPrefab[] holder)
	{
		int i;
		
		int count = Utils.Min(m_topEntries.Count, holder.Length);
		
		// Top ten
		for( i=0; i<count; ++i )		
		{		
			if( m_topEntries[i].isMine )
			{
				holder[i].Name.color = Color.yellow;
				holder[i].Rank.color = Color.yellow;
				holder[i].Score.color = Color.yellow;
			}
			else			
			{
				holder[i].Name.color = Color.white;
				holder[i].Rank.color = Color.white;
				holder[i].Score.color = Color.white;
			}
			
			holder[i].Set(i+1, m_topEntries[i].name, m_topEntries[i].score);			
		}
	}
	
	public void PopulateTopMe(ScoreEntryPrefab[] holder)
	{
		int i;
		
		// Surrounding me		
		int upLim = Utils.Min(m_upEntries.Count, 5);
		int downLim = Utils.Min(m_downEntries.Count, 4);
		
		if( downLim < 4 )
		{
			upLim += 4 - downLim;
			if( upLim > m_upEntries.Count )
			{
				upLim = m_upEntries.Count;
			}
		}
		else if( upLim < 5 )
		{
			downLim += 5 - upLim;
			if( downLim > m_downEntries.Count )
			{
				downLim = m_downEntries.Count;
			}
		}
		
		int n = 0;
		for( i=upLim - 1; i>=0; --i, ++n )		
		{	
			holder[n].Rank.color = Color.white;
			holder[n].Name.color = Color.white;
			holder[n].Score.color = Color.white;
			holder[n].Set(m_upEntries[i].rank, m_upEntries[i].name, m_upEntries[i].score);			
		}
		
		holder[n].Rank.color = Color.yellow;
		holder[n].Name.color = Color.yellow;
		holder[n].Score.color = Color.yellow;
		holder[n].Set(m_myRank, FacebookController.Instance.MyFullName, PlayerStash.Instance.HighScore);					
		++n;
		
		for( i=0; i<downLim; ++i, ++n )		
		{	
			holder[n].Rank.color = Color.white;
			holder[n].Name.color = Color.white;
			holder[n].Score.color = Color.white;
			holder[n].Set(m_downEntries[i].rank, m_downEntries[i].name, m_downEntries[i].score);			
		}
	}
	
	/*public void PopulateTexts(string[] topten, string[] topme)
	{
		int i;
		
		// Top ten
		for( i=0; i<m_topEntries.Count; ++i )		
		{		
			if( m_topEntries[i].isMine )
			{
				topten[0] += "[ffff00]";
				topten[1] += "[ffff00]";
				topten[2] += "[ffff00]";
			}
		
			topten[0] += (i+1).ToString() + "\n";
			topten[1] += m_topEntries[i].name + "\n";
			topten[2] += m_topEntries[i].score.ToString() + "\n";
			
			if( m_topEntries[i].isMine )
			{
				topten[0] += "[-]";
				topten[1] += "[-]";
				topten[2] += "[-]";
			}
		}		
		
		// Surrounding me
		for( i=0; i<m_upEntries.Count; ++i )		
		{
			int r = m_myRank - i - 1;
			m_upEntries[i].rank = r > 0 ? r : 1;
		}
		
		for( i=0; i<m_downEntries.Count; ++i )		
		{
			m_downEntries[i].rank = m_myRank + i + 1;
		}
		
		int upLim = Utils.Min(m_upEntries.Count, 5);
		int downLim = Utils.Min(m_downEntries.Count, 4);
		
		if( downLim < 4 )
		{
			upLim += 4 - downLim;
			if( upLim > m_upEntries.Count )
			{
				upLim = m_upEntries.Count;
			}
		}
		else if( upLim < 5 )
		{
			downLim += 5 - upLim;
			if( downLim < m_downEntries.Count )
			{
				downLim = m_downEntries.Count;
			}
		}
		
		for( i=upLim - 1; i>=0; --i )		
		{	
			topme[0] += m_upEntries[i].rank.ToString() + "\n";
			topme[1] += m_upEntries[i].name + "\n";
			topme[2] += m_upEntries[i].score.ToString() + "\n";			
		}
		
		topme[0] += "[ffff00]" + m_myRank.ToString() + "[-]\n";
		topme[1] += "[ffff00]" + FacebookController.Instance.MyFullName + "[-]\n";
		topme[2] += "[ffff00]" + PlayerStash.Instance.HighScore.ToString() + "[-]\n";			
		
		for( i=0; i<downLim; ++i )		
		{	
			topme[0] += m_downEntries[i].rank.ToString() + "\n";
			topme[1] += m_downEntries[i].name + "\n";
			topme[2] += m_downEntries[i].score.ToString() + "\n";			
		}
	}*/
}
