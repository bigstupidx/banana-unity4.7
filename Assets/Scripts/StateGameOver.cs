using UnityEngine;
using System.Collections;

public class StateGameOver : GameState {

	public UIWidget Container;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonMainMenu")).onClick += (obj) =>
		{
			StartCoroutine (OnMainMenuButtonClick ());
		};
		
		UIEventListener.Get (FindChild ("ButtonLeaderboard")).onClick += (obj) =>
		{
			StartCoroutine (OnLeaderboardButtonClick ());
		};
	}
	
	private IEnumerator OnMainMenuButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
				
		StateManager.Instance.SetState(StateManager.Instance.MainMenu);
	}
	
	private IEnumerator OnLeaderboardButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.SetState(StateManager.Instance.MainMenu);
		StateManager.Instance.PendState(StateManager.Instance.Leaderboard);
	}
	
	public override void OnEnter()
	{		
		Vector3 pos = Container.transform.localPosition;
		pos.x = -1000;
		Container.transform.localPosition = pos;
		
		Container.GetComponent<SpringPosition>().enabled = true;
	}
	
	public override void OnUpdate()
	{
	}
	
	public override void OnExit()
	{
	}
}
