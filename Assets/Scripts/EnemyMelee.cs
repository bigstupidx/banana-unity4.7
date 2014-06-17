using UnityEngine;
using System.Collections;

public class EnemyMelee : Actor {

	protected enum EState
	{
		PATROL = 0,
		CLIMB,
		PUSH_UP,
		ON_WALL,
		DYING
	};

	protected EState m_state = EState.PATROL;
	protected float m_climbDecideTimeout = 0.0f;

	protected int m_patrolDirection = 0;
	private bool m_hasAttackDoneDamage;	
	private bool m_isAttacking = false;

	protected virtual void Start()	
	{
		m_HP = MaxHP + ((EnemiesManager.Instance.SpawnGeneration - 1) * MaxHP);
		
		EnemiesManager.Instance.AddEnemy (this);
		m_climbDecideTimeout = Random.Range(MinTimeBeforeCharge, MaxTimeBeforeCharge);
	}

	// Update is called once per frame
	void Update () {
		if( Mathf.Approximately(Time.timeScale, 0.0f) )
		{
			return;
		}

		if (m_state == EState.DYING) {
			return;
		}

		if (m_HP <= 0) {
			if( m_state == EState.ON_WALL )
			{
				m_dyingFallRotation = new Vector3(0, Random.Range(-10, 10) * 10, Random.Range(-10, 10) * 10);
			}
		
			SetOnDying();
			m_state = EState.DYING;
			m_animator.CrossFade(ANIM_DYING, 0.0f, 0, 0.0f);
			m_animator.speed = 1.0f;
		} else {
			if( Player.Instance.IsDying )
			{
				m_animator.enabled = false;
				return;
			}
			
			if( Blizzard.Instance.Duration > 0.0f )
			{
				m_speedFactor += (0.01f - m_speedFactor) * Time.deltaTime * 2;
			}
			else
			{
				m_speedFactor += (1.0f - m_speedFactor) * Time.deltaTime;
			}
		
			switch (m_state) {
			case EState.PATROL:
				Patrol();
				break;
			case EState.CLIMB:
				Climb();
				break;
			case EState.PUSH_UP:
				PushUp();
				break;
			case EState.ON_WALL:
				OnWall();
				break;		
			}
		}
	}

	void LateUpdate()
	{
		if( Mathf.Approximately(Time.timeScale, 0.0f) )
		{
			return;
		}

		if (m_state == EState.DYING) {
			FaceToImmediate(0);
			FallOnDying();
		}
		
		m_speedFactor += (1.0f - m_speedFactor) * Time.deltaTime;
	}

	protected virtual void Patrol()
	{
		Vector3 pos = transform.localPosition;
		pos.z = Global.PATROL_Z + FeetPosition.y * 10;

		if (m_patrolDirection == 0) {
			m_patrolDirection = pos.x < 0 ? 1 : -1;
		}
		
		if ((m_patrolDirection < 0 && pos.x < Global.WALL_LEFT_X) 
		    || (m_patrolDirection > 0 && pos.x > Global.WALL_RIGHT_X))
		{
			m_patrolDirection = -m_patrolDirection;
		}

		if (m_climbDecideTimeout <= 0.0f && pos.x >= (Global.WALL_MIN_X + BoundingSize.x * 0.5f) && pos.x <= (Global.WALL_MAX_X - BoundingSize.x * 0.5f)) {
			m_state = EState.CLIMB;
			m_animator.SetTrigger(VAR_CLIMB);
		}
		else {
			FaceTo (m_patrolDirection * 90, 5);
			pos.x += m_patrolDirection * MoveSpeed * Time.deltaTime * m_speedFactor;	
			m_animator.SetFloat (VAR_WALK, 1.0f);
			m_animator.speed = m_speedFactor * MoveSpeed * 0.75f;
			
			transform.localPosition = pos;
			
			m_climbDecideTimeout -= Time.deltaTime * m_speedFactor;
		}
	}

	protected virtual void Climb()
	{
		if( !IsCurrentAnim(ANIM_CLIMB) )
		{
			return;
		}
	
		FaceTo (0, 10);
		
		Vector3 pos = transform.localPosition;
		pos.z = Global.CLIMB_Z + (FeetPosition.y * 10);
		
		bool stopClimbing = false;
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
		}
		
		transform.localPosition = pos;
	}

	protected virtual void PushUp()
	{
		Vector3 pos = transform.localPosition;
		m_animator.speed = m_speedFactor * 0.5f;

		if (FeetPosition.y >= Global.WALL_TOP_Y + 0.5f) {
			pos.z = Global.ON_WALL_Z;
		}

		transform.localPosition = pos;

		if (IsCurrentAnim (ANIM_IDLE)) {
			m_state = EState.ON_WALL;
			m_isAttacking = false;
		}
	}

	protected virtual void OnWall()
	{
		StandOnWall ();
		
		Vector3 playerPos = Player.Instance.transform.localPosition;
		FaceTo (playerPos, 5);

		if (m_isAttacking) {
			m_animator.speed = m_speedFactor;			
			
			if( IsCurrentAnim(ANIM_ATTACK) )
			{
				float delta = GetAnimationTime() - m_lastAnimationLoop;
				if( !m_hasAttackDoneDamage && delta > AttackTriggerTime && delta < 0.95f )
				{
					if( IsInDamageRange(Player.Instance) )
					{
						Player.Instance.TakeHit(AttackStrength);
						
						ProjectilesManager.Instance.CreateOnActor(HitEffect, Player.Instance);
					}
					m_hasAttackDoneDamage = true;
				}
				
				if( delta >= 0.95f )
				{
					m_hasAttackDoneDamage = false;
					m_isAttacking = false;
				}
			
				SetAnimationCounter();
			}
			else			
			{
				m_animator.SetBool (VAR_ATTACK, true);
				ResetAnimationCounter();
				m_hasAttackDoneDamage = false;
			}
		} else {			
			
			if( !IsCurrentAnim(ANIM_ATTACK) )
			{
				Vector3 pos = transform.localPosition;
				m_patrolDirection = pos.x < playerPos.x ? 1 : -1;
	
				// Check if being blocked by others ?
				foreach( Actor ally in EnemiesManager.Instance.Enemies )
				{				
					if( ally == this || ally.IsDying || !ally.IsOnWall )
					{
						continue;
					}
					
					if( ( ally.IsHorizontalOverlapWith(this) ) &&
					   (( m_patrolDirection > 0 && ally.transform.localPosition.x > transform.localPosition.x )
					 || ( m_patrolDirection < 0 && ally.transform.localPosition.x < transform.localPosition.x )) )
					{
						m_animator.SetFloat(VAR_WALK, 0.0f);
						return;
					}
				}
	
				m_animator.SetFloat (VAR_WALK, 1.0f);
	
				pos.x += m_patrolDirection * MoveSpeed * m_speedFactor * Time.deltaTime;
				m_animator.speed = MoveSpeed * m_speedFactor;
	
				transform.localPosition = pos;
				
				if( CanAttackMelee(Player.Instance) )
				{
					m_isAttacking = true;
				}
			}
			else
			{
				m_animator.SetBool (VAR_ATTACK, false);
				m_animator.speed = m_speedFactor;
			}
		}
	}
	
	public override bool IsDying {
		get {
			return m_HP <= 0;
		}
	}
	
	public override bool IsOnWall
	{
		get { return m_state == EState.ON_WALL; }
	}
	
	public override bool IsNearWall
	{
		get { return m_state == EState.PUSH_UP; }
	}
	
	public override bool IsClimbing
	{
		get { return m_state == EState.CLIMB; }
	}
}
