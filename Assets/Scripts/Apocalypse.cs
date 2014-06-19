using UnityEngine;
using System.Collections;

public class Apocalypse : MonoBehaviour {

	private static Apocalypse g_instance = null;
	
	public static Apocalypse Instance
	{
		get { return g_instance; }
	}
	
	void Awake()
	{
		g_instance = this;
	}
	
	private int m_fireballsToSpawn = 0;
	private float m_fireballDelay;
	
	public void Reset()
	{
		m_fireballsToSpawn = 0;
		m_fireballDelay = 0.0f;
	}
	
	public int RemainingFireballs
	{
		get { return m_fireballsToSpawn; }
		set { m_fireballsToSpawn = value; }
	}
	
	// Update is called once per frame
	void Update () {
		SpawnFireBalls();
	}
		
	private void SpawnFireBalls()	
	{
		if( m_fireballsToSpawn > 0 )
		{
			m_fireballDelay -= Time.deltaTime;
			if( m_fireballDelay <= 0.0f )
			{
				m_fireballDelay = Random.Range(0.1f, 0.4f);
				
				--m_fireballsToSpawn;		
				
				Vector3 pos;
				pos.x = Random.Range( Global.WALL_MIN_X + 1.0f, Global.WALL_MAX_X - 1.0f );
				pos.y = Random.Range( Global.GROUND_Y + 1.0f, Global.WALL_TOP_Y );				
				
				foreach( Actor enemy in EnemiesManager.Instance.Enemies )
				{
					if( Random.Range(0, 100) < 50 )
					{
						Vector3 enemyPos = (Vector3)enemy.CenterPosition;
						if( enemyPos.x > Global.WALL_MIN_X && enemyPos.x < Global.WALL_MAX_X )
						{
							if( enemyPos.y < Global.GROUND_Y + 1.0f )
							{
								enemyPos.y = Global.GROUND_Y + 1.0f;
							}
							else if( enemyPos.y > Global.WALL_TOP_Y - 1.0f )
							{
								enemyPos.y = Global.WALL_TOP_Y - 1.0f;
							}
							pos = enemyPos;
						}
						
					}
					
					if( Random.Range(0, 100) < 50 )
					{
						break;
					}
				}
				
				pos.z = Global.PROJECTILE_Z;				
				ProjectilesManager.Instance.Create(ProjectilesManager.EXPLOSIVE, pos);
			}
		}
	}
}
