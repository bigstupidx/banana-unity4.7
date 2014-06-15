using UnityEngine;
using System.Collections;

public class Cocktail : Projectile {
	
	private bool m_isDetached = false;
	
	private float m_force = Global.WALL_TOP_Y;
	
	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}
		
		if (m_isDetached) {
			Vector3 pos = transform.localPosition;
			
			if( pos.y >= Global.HEAVEN_Y )
			{
				m_isDetached = false;
				GameObject.Destroy(gameObject);				
				return;
			}
			
			float nextY = pos.y + m_force * Time.deltaTime;
			m_force -= Time.deltaTime * 6.5f;
			
			// Fly
			pos.y = nextY;
			
			if( m_force < 0.0f )
			{	
				pos.z = Global.ON_WALL_Z;	
				
				bool explode = false;
				Vector2 feetPos = Player.Instance.FeetPosition;
				Vector2 bound = Player.Instance.BoundingSize;
				if( Utils.IsPtInRect(pos.x, pos.y,
				                     feetPos.x - bound.x * 0.5f, feetPos.y - 1.0f,
				                     feetPos.x + bound.x * 0.5f, feetPos.y + bound.y) )
				{
					Player.Instance.TakeHit(1);
					pos.z = Global.PROJECTILE_Z;
					explode = true;
				}
				
				if( nextY <= Global.WALL_TOP_Y || explode )
				{
					GameObject.Destroy(this.gameObject);
					
					ProjectilesManager.Instance.Create(ProjectilesManager.BOOM1, pos);
					return;
				}
			}
			
			transform.localPosition = pos;			
			
			transform.Rotate(new Vector3(Time.deltaTime * 400, Time.deltaTime * 400, -Time.deltaTime * 400));
		}
	}
	
	public override void Detach()
	{		
		transform.parent = ProjectilesManager.Instance.Root;		
		m_isDetached = true;
		
		Vector3 pos = transform.localPosition;
		pos.z = Global.PROJECTILE_Z;
		transform.localPosition = pos;
	}	
	
	public override void SetDirection(Vector3 dir)
	{
	}
}
