using UnityEngine;
using System.Collections;

public class ScoreEntryPrefab : MonoBehaviour {

	public UILabel Rank;
	public UILabel Name;
	public UILabel Score;
	
	public void Set(int rank, string name, int score)
	{
		Rank.text = rank.ToString();
		Name.text = name.ToString();
		Score.text = score.ToString();
	}
}
