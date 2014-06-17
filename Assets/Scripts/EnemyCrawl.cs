using UnityEngine;
using System.Collections;

public class EnemyCrawl : EnemyMelee {

	private float m_bending = 0.0f;
	private int m_dodgeDirection = 0;
	private float m_dodgingSenseTimeout = 0.0f;

	protected override void Start()
	{
		base.Start();
		
		m_HP = MaxHP + (((EnemiesManager.Instance.SpawnGeneration - 1) * MaxHP) >> 1);
	}

	protected override void Climb()
	{
		// Bend on wall
		m_bending += Utils.shortestAngle(m_bending, -90) * Time.deltaTime * 2 * m_speedFactor;
		
		Quaternion quat = transform.localRotation;
		Vector3 euler = quat.eulerAngles;
		euler.x = m_bending;
		euler.y += Utils.shortestAngle(euler.y, 0) * Time.deltaTime * m_speedFactor;
		quat.eulerAngles = euler;
		transform.localRotation = quat;		
		
		Vector3 pos = transform.localPosition;
		pos.z = Global.CLIMB_Z + (FeetPosition.y * 10.0f);
		
		if( Player.Instance.IsRangeWeapon && Player.Instance.IsAttacking && IsInArcherRange(Player.Instance) )		
		{
			m_dodgingSenseTimeout = 1.0f;
			if( m_dodgeDirection == 0 )
			{
				m_dodgeDirection = pos.x < Player.Instance.transform.position.x ? -1 : 1;
				if( (m_dodgeDirection < 0 && pos.x - (BoundingSize.x + Player.Instance.BoundingSize.x) < Global.WALL_MIN_X)			
				   || (m_dodgeDirection > 0 && pos.x + (BoundingSize.x + Player.Instance.BoundingSize.x) > Global.WALL_MAX_X) )
				{
					m_dodgeDirection = -m_dodgeDirection;
				}			
			}
		}
		else if( m_dodgingSenseTimeout > 0.0f )
		{
			if( m_dodgeDirection == 0 )
			{
				m_dodgeDirection = pos.x < Player.Instance.transform.position.x ? -1 : 1;				
			}			
		}
		else
		{
			m_dodgeDirection = 0;
		}
		
		if( m_dodgingSenseTimeout > 0.0f )
		{
			m_dodgingSenseTimeout -= Time.deltaTime;
		}
		
		bool stopClimbing = false;	
			
		if( pos.y > BoundingSize.y && m_dodgeDirection != 0 )
		{
			if( (m_dodgeDirection < 0 && pos.x - BoundingSize.x * 0.5f < Global.WALL_MIN_X)			
			   || (m_dodgeDirection > 0 && pos.x + BoundingSize.x * 0.5f > Global.WALL_MAX_X) )
			{
				m_dodgeDirection = -m_dodgeDirection;
			}
			
			foreach( Actor ally in EnemiesManager.Instance.Enemies )
			{
				if( ally == this || !ally.IsClimbing )
				{
					continue;
				}
				
				if( this.IsOverlapWith(ally) 
					&& (( m_dodgeDirection < 0 && FeetPosition.x > ally.FeetPosition.x )
				   || ( m_dodgeDirection > 0 && FeetPosition.x < ally.FeetPosition.x )))
				{
					m_dodgeDirection = -m_dodgeDirection;
					stopClimbing = true;
					break;
				}
			}
			
			if( !stopClimbing )
			{
				pos.x += m_dodgeDirection * MoveSpeed * m_speedFactor * Time.deltaTime;				
				m_animator.speed = MoveSpeed * m_speedFactor * 0.4f;
			}
			else			
			{
				m_animator.speed = 0.1f;				
			}
		}
		else		
		{
			foreach( Actor ally in EnemiesManager.Instance.Enemies )
			{
				if( ally == this || (!ally.IsClimbing && !ally.IsNearWall && !ally.IsOnWall) )
				{
					continue;
				}
				
				if( (ally.IsOnWall || ally.IsNearWall) && this.IsClimbingReachWall && this.IsHorizontalOverlapWith(ally) )
				{
					stopClimbing = true;
					break;
				}
				
				if( this.IsOverlapWith(ally) )
				{
					if( FeetPosition.y == ally.FeetPosition.y )
					{
						pos.y -= 0.00001f;
						stopClimbing = true;
						break;
					}
					else if( FeetPosition.y < ally.FeetPosition.y )
					{
						stopClimbing = true;
						break;
					}
				}
			}
			
			if( !stopClimbing )
			{			
				pos.y += ClimbSpeed * m_speedFactor * Time.deltaTime;				
				m_animator.speed = ClimbSpeed * m_speedFactor * 0.4f;			
				
				if (IsClimbingReachWall) {
					m_state = EState.PUSH_UP;
					m_animator.SetTrigger(VAR_PUSH_UP);
				}
			}
			else		
			{
				m_animator.speed = 0.1f;
				m_dodgingSenseTimeout = 1.0f;
			}
		}
		
		transform.localPosition = pos;
	}
	
	protected override void PushUp()
	{
		Vector3 pos = transform.localPosition;
		m_animator.speed = m_speedFactor * 0.5f;		
		
		if (FeetPosition.y >= Global.WALL_TOP_Y + 0.5f || pos.z >= Global.ON_WALL_Z - 0.1f) {			
			pos.z = Global.ON_WALL_Z;
			transform.localPosition = pos;
			
			StandOnWall();
			
			// Bend on wall
			m_bending += Utils.shortestAngle(m_bending, 0) * Time.deltaTime * 4 * m_speedFactor;
			Quaternion quat = transform.localRotation;
			Vector3 euler = quat.eulerAngles;
			euler.x = m_bending;
			quat.eulerAngles = euler;
			transform.localRotation = quat;		
			
			if( Mathf.Abs(m_bending) < 1.0f || Mathf.Abs(m_bending) > 359.0f )
			{
				m_state = EState.ON_WALL;
			}
		}
		else		
		{
			pos.y += ClimbSpeed * m_speedFactor * Time.deltaTime;
			transform.localPosition = pos;
			
			// Bend on wall
			Quaternion quat = transform.localRotation;
			Vector3 euler = quat.eulerAngles;			
			m_bending += Utils.shortestAngle(m_bending, 0) * Time.deltaTime * 1 * m_speedFactor;
			euler.x = m_bending;
			quat.eulerAngles = euler;
			transform.localRotation = quat;		
		}		
	}
	
	public override Vector2 FeetPosition
	{
		get 
		{ 
			return new Vector2(transform.localPosition.x, transform.localPosition.y - BoundingSize.y * ( IsClimbing ? 1.0f : 0.5f ));
		}
		
		set
		{
			Vector3 pos = (Vector3)value;			
			pos.y += BoundingSize.y * 0.5f;
			transform.localPosition = pos;			
		}
	}
	
	public override void TakeHit(int damage)
	{
		m_HP -= damage;
		m_speedFactor = 0.0f;
	}
}
