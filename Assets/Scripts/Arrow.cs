using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {

	private const float ARROW_SPEED = 20;
	private const float ARROW_WIDTH = 0.1f;

	public GameObject Trail;

	private bool m_isDetached = false;
	public float DeltaX = 0.0f;
	
	private float m_nextY;

	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}

		if (m_isDetached) {
			Vector3 pos = transform.localPosition;
			
			if( pos.y <= Global.HELL_Y )
			{
				m_isDetached = false;
				GameObject.Destroy(gameObject);				
				return;
			}
			
			m_nextY = pos.y - ARROW_SPEED * Time.deltaTime;
			
			// Test hit with enemies
			foreach( Actor enemy in EnemiesManager.Instance.Enemies )
			{
				if( TestHit(enemy) )
				{
					GameObject.Destroy(this.gameObject);
					enemy.TakeHit(1);
					
					Vector3 projectilePos = (Vector3)enemy.FeetPosition;
					projectilePos.y += enemy.BoundingSize.y;
					if( projectilePos.y > pos.y )
					{
						projectilePos.y = pos.y;
					}
					projectilePos.z = Global.PROJECTILE_Z;
					
					ProjectilesManager.Instance.Create(ProjectilesManager.POW2, projectilePos);
					return;
				}
			}
			
			// Fly
			pos.x += DeltaX * Time.deltaTime;
			pos.y = m_nextY;
			transform.localPosition = pos;

			Vector3 trailScale = Trail.transform.localScale;
			trailScale.y += 2 * Time.deltaTime;
			trailScale.x += 0.1f * Time.deltaTime;
			Trail.transform.localScale = trailScale;
		}
	}

	public void Detach()
	{
		Trail.SetActive (true);

		transform.parent = ProjectilesManager.Instance.Root;
		transform.localRotation = Quaternion.Euler(90, 0, 0);
		m_isDetached = true;

		Vector3 pos = transform.localPosition;
		pos.z = Global.PROJECTILE_Z;
		transform.localPosition = pos;
	}
	
	public bool TestHit(Actor other)
	{
		if( other.IsDying || other.IsOnWall )
		{
			return false;
		}
	
		Vector3 myPos = transform.localPosition;		
		Vector2 otherPos = other.FeetPosition;		
		
		return Utils.TestRectsHit(
			myPos.x - ARROW_WIDTH, m_nextY,
			myPos.x + ARROW_WIDTH, myPos.y,
			otherPos.x - other.BoundingSize.x * 0.5f, otherPos.y,
			otherPos.x + other.BoundingSize.x * 0.5f, otherPos.y + other.BoundingSize.y);
	}
}
