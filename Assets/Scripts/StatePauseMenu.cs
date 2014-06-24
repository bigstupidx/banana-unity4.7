using UnityEngine;
using System.Collections;

public class StatePauseMenu : GameState {

	public UIToggle ToggleSound;
	public UIButton FacebookButton;
	public UIButton InviteButton;	
	public UILabel ScoreLabel;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonBack")).onClick += (obj) =>
		{
			StartCoroutine (OnBackButtonClick ());
		};
		
		UIEventListener.Get (FindChild ("ButtonLeaderboard")).onClick += (obj) =>
		{
			StartCoroutine (OnLeaderboardButtonClick ());
		};
		
		UIEventListener.Get (FindChild ("ButtonShop")).onClick += (obj) =>
		{
			StartCoroutine (OnShopButtonClick ());
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
	
	private IEnumerator OnBackButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PopState();
	}
	
	private IEnumerator OnLeaderboardButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PushState(StateManager.Instance.Leaderboard);
	}
	
	private IEnumerator OnShopButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PushState(StateManager.Instance.Shop);
	}
	
	public override void OnEnter()
	{
		ScoreLabel.text = PlayerStash.Instance.CurrentScore.ToString();		
		
		EnemiesManager.Instance.Paused = false;
		ToggleSound.value = PlayerStash.Instance.IsSound;
	}
	
	public override void OnUpdate()
	{
		PlayerStash.Instance.IsSound = ToggleSound.value;
		
		FacebookButton.gameObject.SetActive(FacebookController.Instance.CanOperateLogIn());
		InviteButton.gameObject.SetActive(FacebookController.Instance.CanOperateInviteFriends());
	}
	
	public override void OnExit()
	{
	}
	
	public override void OnBackKey()
	{
		AdsController.Instance.OnQuit();
		Application.Quit();
	}
}
