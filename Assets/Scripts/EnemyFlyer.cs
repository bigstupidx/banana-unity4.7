using UnityEngine;
using System.Collections;

public class EnemyFlyer : Actor {
	protected enum EState
	{
		PATROL = 0,		
		ON_WALL,
		DYING
	};
	
	protected EState m_state = EState.PATROL;
	protected float m_attackDecideTimeout = 0.0f;
	
	protected int m_patrolDirection = 0;
	private bool m_hasAttackDoneDamage;	
	private bool m_hasDoneAttacking;
	private bool m_isAttacking = false;
	
	void Start()	
	{
		m_HP = MaxHP + (((EnemiesManager.Instance.SpawnGeneration - 1) * MaxHP) / 3);
		
		EnemiesManager.Instance.AddEnemy (this);
		m_attackDecideTimeout = Random.Range(MinTimeBeforeCharge, MaxTimeBeforeCharge);
		
		Vector3 pos = transform.localPosition;
		pos.y = Random.Range(Global.WALL_TOP_Y * 0.25f, Global.WALL_TOP_Y * 0.75f);
		transform.localPosition = pos;
		
		m_hasDoneAttacking = false;
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
	}
	
	protected virtual void Patrol()
	{
		Vector3 pos = transform.localPosition;
		pos.z = Global.PATROL_Z;
		
		if (m_patrolDirection == 0) {
			m_patrolDirection = pos.x < 0 ? 1 : -1;
		}
		
		if ((m_patrolDirection < 0 && pos.x < Global.WALL_LEFT_X) 
		    || (m_patrolDirection > 0 && pos.x > Global.WALL_RIGHT_X))
		{
			if (m_attackDecideTimeout <= 0.0f) {
				m_hasDoneAttacking = false;
				m_state = EState.ON_WALL;
				pos.y = Global.WALL_TOP_Y + Player.Instance.BoundingSize.y * 0.75f + BoundingSize.y * 0.5f;
				pos.z = Global.ON_WALL_Z;
				transform.localPosition = pos;	
				m_isAttacking = false;			
				return;
			}
			
			pos.y = Random.Range(Global.WALL_TOP_Y * 0.25f, Global.WALL_TOP_Y * 0.75f);
			m_patrolDirection = -m_patrolDirection;
		}		
				
		FaceTo (m_patrolDirection * 90, 5);
		pos.x += m_patrolDirection * MoveSpeed * Time.deltaTime * m_speedFactor;	
		m_animator.SetFloat (VAR_WALK, 1.0f);
		m_animator.speed = m_speedFactor * MoveSpeed * 0.4f;
		
		transform.localPosition = pos;
		
		m_attackDecideTimeout -= Time.deltaTime * m_speedFactor;
	}
	
	protected virtual void OnWall()
	{
		Vector3 playerPos = Player.Instance.transform.localPosition;		
		
		if (m_isAttacking ) {						
			FaceTo (playerPos, 5);
			m_animator.speed = m_speedFactor;			
			
			if( IsCurrentAnim(ANIM_ATTACK) )
			{
				float delta = GetAnimationTime() - m_lastAnimationLoop;
				if( !m_hasAttackDoneDamage && delta > AttackTriggerTime && delta < 0.95f )
				{
					if( IsInDamageRange (Player.Instance) )
					{
						Player.Instance.TakeHit(AttackStrength);										
						ProjectilesManager.Instance.CreateOnActor(HitEffect, Player.Instance);
					}
					
					m_hasDoneAttacking = true;
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
			
			Vector3 pos = transform.localPosition;
			
			if( !IsCurrentAnim(ANIM_ATTACK) )
			{
				m_patrolDirection = pos.x < playerPos.x ? 1 : -1;
				
				FaceTo (m_patrolDirection * 90, 5);
				
				m_animator.SetFloat (VAR_WALK, 1.0f);
				
				pos.x += m_patrolDirection * MoveSpeed * m_speedFactor * Time.deltaTime;
				m_animator.speed = MoveSpeed * m_speedFactor * 0.4f;
				
				transform.localPosition = pos;
			}
			else
			{
				m_animator.SetBool (VAR_ATTACK, false);
				m_animator.speed = m_speedFactor;
			}
			
			if( m_hasDoneAttacking )
			{
				pos.z = Global.PATROL_Z;
				
				
				m_animator.SetBool (VAR_ATTACK, false);
				m_state = EState.PATROL;
				m_attackDecideTimeout = Random.Range(MinTimeBeforeCharge, MaxTimeBeforeCharge);
			}	
			
			transform.localPosition = pos;	
			
			if( CanFlyerAttack (Player.Instance) )
			{
				m_isAttacking = true;
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
		get { return transform.localPosition.y > Global.WALL_TOP_Y; }
	}
	
	public override bool IsNearWall
	{
		get { return false; }
	}
	
	public override bool IsClimbing
	{
		get { return false; }
	}
}
