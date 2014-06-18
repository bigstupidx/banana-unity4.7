using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StateManager : MonoBehaviour {

	private static StateManager g_instance = null;

	public GameState MainMenu;
	public GameState InGameMenu;
	public GameState Leaderboard;
	public GameState Shop;

	private GameState m_currentState;
	private GameState m_nextState;

	private Stack<GameState> m_stackStates = new Stack<GameState>();

	public static StateManager Instance
	{
		get { return g_instance; }
	}

	void Awake()
	{
		Global.Init ();

		MainMenu.gameObject.SetActive (false);
		InGameMenu.gameObject.SetActive (false);

		g_instance = this;
	}

	void Start()
	{
		m_currentState = MainMenu;
		m_currentState.gameObject.SetActive (true);
		m_currentState.OnEnter ();

		m_nextState = m_currentState;
	}
	
	// Update is called once per frame
	void Update () {
		if (m_currentState != m_nextState) {
			m_currentState.OnExit();
			m_currentState.gameObject.SetActive(false);

			m_currentState = m_nextState;
			m_currentState.gameObject.SetActive(true);
			m_currentState.OnEnter();
		}

		m_currentState.OnUpdate ();
	}

	public void PushState(GameState state)
	{
		m_stackStates.Push (m_currentState);
		m_nextState = state;
	}

	public void PopState()
	{
		m_nextState = m_stackStates.Pop ();
	}

	public void SetState(GameState state)
	{
		m_stackStates.Clear ();
		m_nextState = state;
	}
}
