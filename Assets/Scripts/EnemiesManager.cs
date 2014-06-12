using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemiesManager : MonoBehaviour {

	private static EnemiesManager g_instance = null;

	public static EnemiesManager Instance
	{
		get { return g_instance; }
	}

	void Awake()
	{
		g_instance = this;
	}

	private List<Actor> m_enemies = new List<Actor>();

	public void Clear()
	{
		m_enemies.Clear ();
	}

	public void AddEnemy(Actor enemy)
	{
		if (!m_enemies.Contains (enemy)) {
			m_enemies.Add (enemy);
		}
	}
	
	public void RemoveEnemy(Actor enemy)
	{		
		m_enemies.Remove (enemy);	
	}
	
	public List<Actor> Enemies
	{
		get { return m_enemies; }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
