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
		
		m_spawnPrograms = new SpawnProgram[7];
		m_enemiesKilledCount = new int[7];
		
		Init();
	}
	
	private struct SpawnProgram
	{
		public int id;
		public int level;
		public int cost;
		public int chance;
		
		public SpawnProgram( int nid, int nlevel, int ncost, int nchance )
		{
			id = nid;
			level = nlevel;
			cost = ncost;
			chance = nchance;
		}
	}

	private List<Actor> m_enemies = new List<Actor>();
	private bool m_isPaused = true;
	
	private float m_spawnTimeout;	
	
	public GameObject[] Templates;
	public Transform Root;
	
	private SpawnProgram[] m_spawnPrograms;
	private int[] m_enemiesKilledCount;
	
	private const int LEVELS_PER_GENERATION = 15;	
	private const float TIME_PER_LEVEL = 8.0f;
	private const float TIME_PER_SPAWN = 1.0f;
	private const int STARTING_BUDGET = 20;
	private const int BUDGET_PER_LEVEL = 20;
	private const int BUDGET_INCREASE_PER_LEVEL = 10;
	private const int BUDGET_INCREASE_PER_GENERATION = 10;
	
	public ObsecuredInt SpawnGeneration = 1;
	private int m_currentLevel;
	private float m_currentLevelTime;
	private int m_currentBudget;	
	
	public bool IsTestingMode = false;
	
	public void Start()
	{
		m_spawnPrograms[0] = new SpawnProgram(0, 0, 7, 400);
		m_spawnPrograms[1] = new SpawnProgram(1, 2, 20, 380);
		m_spawnPrograms[2] = new SpawnProgram(2, 4, 24, 360);
		m_spawnPrograms[3] = new SpawnProgram(3, 6, 28, 320);
		m_spawnPrograms[4] = new SpawnProgram(4, 8, 32, 300);
		m_spawnPrograms[5] = new SpawnProgram(5, 10, 36, 280);
		m_spawnPrograms[6] = new SpawnProgram(6, 12, 40, 260);		
		
		/*m_spawnPrograms[0] = new SpawnProgram(0, 0, 7, 400);
		m_spawnPrograms[1] = new SpawnProgram(1, 0, 12, 380);
		m_spawnPrograms[2] = new SpawnProgram(2, 0, 16, 360);
		m_spawnPrograms[3] = new SpawnProgram(3, 0, 20, 320);
		m_spawnPrograms[4] = new SpawnProgram(4, 0, 25, 300);
		m_spawnPrograms[5] = new SpawnProgram(5, 0, 30, 280);
		m_spawnPrograms[6] = new SpawnProgram(6, 0, 35, 260);	*/
	}
	
	public void AddEnemy(Actor enemy)
	{
		if (enemy != null && !m_enemies.Contains (enemy)) {
			m_enemies.Add (enemy);
			
			if( IsTestingMode )
			{
				enemy.MinTimeBeforeCharge = 1;
				enemy.MaxTimeBeforeCharge = 1;
			}
		}
	}
	
	public void RemoveEnemy(Actor enemy)
	{		
		m_enemies.Remove (enemy);	
		++m_enemiesKilledCount[enemy.TemplateId];
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
	
		m_enemies.Clear ();		
		
		Init();
	}
	
	private void Init()
	{
		m_currentLevel = 1;
		SpawnGeneration = 1;
		m_spawnTimeout = 0.0f;
		m_currentLevelTime = TIME_PER_LEVEL;	
		m_currentBudget = STARTING_BUDGET;
		
		for (int i=0; i<m_enemiesKilledCount.Length; ++i )
		{
			m_enemiesKilledCount[i] = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if( Mathf.Approximately(Time.timeScale, 0.0f) || m_isPaused )
		{
			return;
		}
		
		m_currentLevelTime -= Time.deltaTime;
		if( m_currentLevelTime <= 0.0f )
		{
			m_spawnTimeout = 0.0f;
			m_currentLevelTime = TIME_PER_LEVEL;
			
			++m_currentLevel;
			if( m_currentLevel > LEVELS_PER_GENERATION )
			{
				++SpawnGeneration;
				m_currentLevel = 1;
			}	
			
			m_currentBudget += BUDGET_PER_LEVEL + (BUDGET_INCREASE_PER_LEVEL * m_currentLevel) + (BUDGET_INCREASE_PER_GENERATION * (SpawnGeneration-1));
		}		
		
		m_spawnTimeout -= Time.deltaTime;
		if( m_spawnTimeout <= 0.0f )
		{			
			int totalChance = 0;
			foreach( SpawnProgram prog in m_spawnPrograms )
			{
				if( m_currentLevel >= prog.level )
				{
					totalChance += prog.chance;
				}
			}
			
			int select = 0;
			int dice = Random.Range(0, totalChance);
			foreach( SpawnProgram prog in m_spawnPrograms )
			{
				if( m_currentLevel >= prog.level && dice <= prog.chance )
				{
					break;
				}
				
				dice -= prog.chance;
				++select;
			}
			
			if( select >= m_spawnPrograms.Length )
			{
				select = m_spawnPrograms.Length - 1;
			}
			
			int cost = m_spawnPrograms[select].cost;
			if( m_currentBudget >= cost )
			{
				m_currentBudget -= cost;			
				Spawn(select);
				
				m_spawnTimeout = TIME_PER_SPAWN;
			}
			else
			{
				m_spawnTimeout = 0.1f;
			}			
		}
		
		if( m_enemies.Count < 1 )
		{
			m_currentLevelTime = 0.0f;
		}
	}
	
	private void Spawn(int template)
	{
		GameObject obj = GameObject.Instantiate (Templates[template]) as GameObject;
		obj.transform.parent = Root;	
		
		float spawnX = Random.Range(0, 100) < 50 ? Global.WALL_LEFT_X :	Global.WALL_RIGHT_X;
		obj.transform.localPosition = new Vector3( spawnX, -0.5f - Random.Range(0.0f, 0.25f), 0.0f);
		
		Actor act = obj.GetComponent<Actor>();
		act.TemplateId = template;		
		
		AddEnemy(act);
	}
	
	public int[] KillingStatistics
	{
		get { return m_enemiesKilledCount; }
	}
	
	private static string makeCountString(int count, string name)
	{
		return count + "  " + name + (count > 0 ? "s" : "" );
	}
}
