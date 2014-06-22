using UnityEngine;
using System.Collections;

public class StateLeaderboard : GameState {	

	public UIButton FacebookButton;
	public UIButton ShareButton;
	public UIToggle Top10Toggle;
	public UIToggle TopMeToggle;
		
	public UIWidget ScorePanel;
	public UILabel PleaseWaitLabel;
	public UILabel ErrorLabel;
	public GameObject ScoreEntryTemplate;
	
	private int m_lastToggle = -1;		
	
	private ScoreEntryPrefab[] m_scoreEntries = new ScoreEntryPrefab[10];
	
	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonBack")).onClick += (obj) =>
		{
			StartCoroutine (OnBackButtonClick ());
		};
		
		UIEventListener.Get(FacebookButton.gameObject).onClick += (obj) =>
		{
			FacebookController.Instance.Operate(FacebookController.EOperation.LOG_IN);		
		};
		FacebookButton.gameObject.SetActive(false);
		
		UIEventListener.Get(ShareButton.gameObject).onClick += (obj) =>
		{
			FacebookController.Instance.Operate(FacebookController.EOperation.POST_TO_WALL);		
		};
		ShareButton.gameObject.SetActive(false);	
		
		float scorePanelHeight = ScorePanel.parent.GetComponent<UIWidget>().height - 70;
		float lineHeight = scorePanelHeight / 9.0f;
		Transform ScorePanelRoot = ScorePanel.transform;
		Vector3 pos = Vector3.zero;
		pos.y = scorePanelHeight * 0.5f;
		
		for( int i=0; i<10; ++i )
		{
			GameObject obj = GameObject.Instantiate(ScoreEntryTemplate) as GameObject;
			obj.transform.parent = ScorePanelRoot;
			obj.transform.localPosition = pos;
			pos.y -= lineHeight;
			
			m_scoreEntries[i] = obj.GetComponent<ScoreEntryPrefab>();
		}
		
		ScorePanel.alpha = 0.0f;
	}
	
	private IEnumerator OnBackButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PopState();
	}

	public override void OnEnter()
	{		
		LeaderboardController.Instance.Request();
	
		PlayerStash.Instance.HighScore = Utils.Max(PlayerStash.Instance.HighScore, PlayerStash.Instance.CurrentScore);
		PlayerStash.Instance.CurrentScore = PlayerStash.Instance.HighScore;		
		
		m_lastToggle = -1;
	}
	
	public override void OnUpdate()
	{
		FacebookButton.gameObject.SetActive(FacebookController.Instance.CanOperateLogIn());
		ShareButton.gameObject.SetActive(FacebookController.Instance.CanOperatePost());
		
		if( LeaderboardController.Instance.HasInfo )
		{
			PleaseWaitLabel.gameObject.SetActive(false);
			ErrorLabel.gameObject.SetActive(false);
			
			ScorePanel.alpha = 1.0f;
			
			int newToggle = -1;
			if( Top10Toggle.value )
			{
				newToggle = 0;
			}
			else if( TopMeToggle.value )
			{
				newToggle = 1;
			}
			
			if( m_lastToggle != newToggle )
			{
				m_lastToggle = newToggle;
				
				if( m_lastToggle == 0 )
				{
					LeaderboardController.Instance.PopulateTopTen(m_scoreEntries);
				}
				else
				{
					LeaderboardController.Instance.PopulateTopMe(m_scoreEntries);
				}
			}
		}
		else if( LeaderboardController.Instance.GotError )
		{
			ErrorLabel.text = "Could not get the leaderboard. Please check your internet connection and try again.";
		
			PleaseWaitLabel.gameObject.SetActive(false);
			ErrorLabel.gameObject.SetActive(true);
			
			ScorePanel.alpha = 0.0f;
		}
		else
		{
			if( !FB.IsLoggedIn )
			{
				ErrorLabel.text = "Please log in your facebook account to access leaderboard.";
			
				PleaseWaitLabel.gameObject.SetActive(false);
				ErrorLabel.gameObject.SetActive(true);
			}
			else
			{
				PleaseWaitLabel.gameObject.SetActive(true);
				ErrorLabel.gameObject.SetActive(false);
			}
			
			ScorePanel.alpha = 0.0f;
			
			
		}
	}
	
	public override void OnExit()
	{		
	}
}
