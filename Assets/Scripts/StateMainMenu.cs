using UnityEngine;
using System.Collections;

public class StateMainMenu : GameState {

	public UIToggle ToggleSound;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonPlay")).onClick += (obj) =>
		{
			StartCoroutine (OnPlayButtonClick ());
		};
	}
	
	private IEnumerator OnPlayButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		EnemiesManager.Instance.Reset();
		StateManager.Instance.PushState (StateManager.Instance.InGameMenu);		
	}

	public override void OnEnter()
	{
		ToggleSound.value = PlayerStash.Instance.IsSound;
	}

	public override void OnUpdate()
	{
		PlayerStash.Instance.IsSound = ToggleSound.value;
		
		Vector3 pos = Camera.main.transform.localPosition;
		pos.y += (11.0f - pos.y) * Time.deltaTime * 2;
		Camera.main.transform.localPosition = pos;
	}

	public override void OnExit()
	{
	}

}
