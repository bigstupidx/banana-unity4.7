using UnityEngine;
using System.Collections;

public class StateMainMenu : GameState {

	public UIToggle ToggleSound;
	public UIButton FacebookButton;
	public UIButton InviteButton;	
	public UIWidget[] WidgetTutorials;
	
	private const float TIME_PER_TUTORIAL = 7.5f;
	
	private int m_currentTutorial = 0;
	private float m_tutorialTimeout = 0.0f;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonPlay")).onClick += (obj) =>
		{
			StartCoroutine (OnPlayButtonClick ());
		};
		
		UIEventListener.Get(FacebookButton.gameObject).onClick += (obj) =>
		{
			FacebookController.Instance.Operate(FacebookController.EOperation.LOG_IN);		
		};
		FacebookButton.gameObject.SetActive(false);
		
		UIEventListener.Get(InviteButton.gameObject).onClick += (obj) =>
		{
			FacebookController.Instance.Operate(FacebookController.EOperation.INVITE_FRIENDS);		
		};
		InviteButton.gameObject.SetActive(false);		
	}
	
	private IEnumerator OnPlayButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		Player.Instance.Reset();
		EnemiesManager.Instance.Reset();
		StateManager.Instance.PushState (StateManager.Instance.InGameMenu);		
	}

	public override void OnEnter()
	{
		ToggleSound.value = PlayerStash.Instance.IsSound;
		
		m_currentTutorial = 0;
		m_tutorialTimeout = TIME_PER_TUTORIAL;
	}

	public override void OnUpdate()
	{
		FacebookButton.gameObject.SetActive(FacebookController.Instance.IsFunctional && !FacebookController.Instance.IsLoggedIn);
		InviteButton.gameObject.SetActive(FacebookController.Instance.IsLoggedIn);
		PlayerStash.Instance.IsSound = ToggleSound.value;
		
		Vector3 pos = Camera.main.transform.localPosition;
		pos.y += (11.0f - pos.y) * Time.deltaTime * 2;
		Camera.main.transform.localPosition = pos;
		
		// Fade in out tutorials
		m_tutorialTimeout -= Time.deltaTime;
		if( m_tutorialTimeout < 0.0f )
		{
			m_tutorialTimeout = TIME_PER_TUTORIAL;			
			++m_currentTutorial;
			if( m_currentTutorial >= WidgetTutorials.Length )
			{
				m_currentTutorial = 0;
			}
		}
		
		for( int i=0; i<WidgetTutorials.Length; ++i )
		{
			if( i == m_currentTutorial )
			{
				WidgetTutorials[i].alpha += (1.0f - WidgetTutorials[i].alpha) * Time.deltaTime * 5;
			}
			else
			{
				WidgetTutorials[i].alpha += (- WidgetTutorials[i].alpha) * Time.deltaTime * 5;
			}
		}
	}

	public override void OnExit()
	{
	}

}
