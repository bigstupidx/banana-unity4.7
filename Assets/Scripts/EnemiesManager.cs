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
	private bool m_isPaused = true;
	
	private float m_spawnTimeout;	
	
	public GameObject[] Templates;
	public Transform Root;

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
	
	public bool Paused
	{
		get { return m_isPaused; }
		set { m_isPaused = value; }
	}
	
	public void Reset()
	{
		foreach( Actor enemy in m_enemies )
		{
			GameObject.Destroy(enemy.gameObject);
		}
	
		m_enemies.Clear();
		m_spawnTimeout = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if( Mathf.Approximately(Time.timeScale, 0.0f) || m_isPaused )
		{
			return;
		}
		
		m_spawnTimeout -= Time.deltaTime;
		if( m_spawnTimeout <= 0.0f )
		{
			int temp = Random.Range(0, Templates.Length);
			if( temp >= Templates.Length )
			{
				temp = Templates.Length - 1;
			}
			
			Spawn(temp);
		
			m_spawnTimeout = 3.0f;
		}
	}
	
	private void Spawn(int template)
	{
		GameObject obj = GameObject.Instantiate (Templates[template]) as GameObject;
		obj.transform.parent = Root;	
		
		float spawnX = Random.Range(0, 100) < 50 ? Global.WALL_LEFT_X :	Global.WALL_RIGHT_X;
		obj.transform.localPosition = new Vector3( spawnX, -0.5f - Random.Range(0.0f, 0.25f), 0.0f);
	}
}
