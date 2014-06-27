using UnityEngine;
using System.Collections;

public class StateAbout : GameState {

	public UILabel AboutText;
	
	private string m_originalAboutText;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonBack")).onClick += (obj) =>
		{
			StartCoroutine (OnBackButtonClick ());
		};		
	}
	
	private IEnumerator OnBackButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PopState();
	}
	
	public override void OnEnter()
	{
		m_originalAboutText = AboutText.text;
		
		AboutText.text = m_originalAboutText.Replace("{version}", "Version " + CurrentBundleVersion.version);
	}
	
	public override void OnUpdate()
	{		
	}
	
	public override void OnExit()
	{	
		AboutText.text = m_originalAboutText;
	}
	
	public override void OnBackKey()
	{
		StateManager.Instance.PopState();
	}
}
