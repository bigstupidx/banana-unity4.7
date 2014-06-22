using UnityEngine;
using System.Collections;

public class EnemyRanged : Actor {
	
	protected enum EState
	{
		PATROL = 0,
		SHOOT,
		DYING
	};
	
	protected EState m_state = EState.PATROL;
	protected float m_attackDecideTimeout = 0.0f;
	
	protected int m_patrolDirection = 0;
	private bool m_hasAttackDoneDamage;		
	
	private Projectile m_holdingProjectile = null;
	
	public Transform RightHand;
	
	void Start()	
	{
		m_HP = MaxHP + (((EnemiesManager.Instance.SpawnGeneration - 1) * MaxHP) / 2);
		
		EnemiesManager.Instance.AddEnemy (this);
		
		m_attackDecideTimeout = Random.Range(MinTimeBeforeCharge, MaxTimeBeforeCharge);
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
				
			case EState.SHOOT:
				Shoot();
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
		
		if (m_attackDecideTimeout <= 0.0f && CanDoRangedAttack) {
			m_state = EState.SHOOT;
			m_attackDecideTimeout = Random.Range(MinTimeBeforeCharge, MaxTimeBeforeCharge);
			
			m_hasAttackDoneDamage = false;
			m_animator.SetBool(VAR_ATTACK, true);
			ResetAnimationCounter();
			
			m_holdingProjectile = ProjectilesManager.Instance.Create<Projectile>(HitEffect, RightHand);
		}
		else {
			FaceTo (m_patrolDirection * 90, 5);
			pos.x += m_patrolDirection * MoveSpeed * Time.deltaTime * m_speedFactor;	
			m_animator.SetFloat (VAR_WALK, 1.0f);
			m_animator.speed = m_speedFactor * MoveSpeed * 0.75f;
			
			transform.localPosition = pos;
			
			m_attackDecideTimeout -= Time.deltaTime * m_speedFactor;
		}
	}
	
	public virtual void Shoot()
	{
		FaceTo(Player.Instance.transform.localPosition, 5);
		
		if( IsCurrentAnim(ANIM_ATTACK) )
		{
			float delta = GetAnimationTime() - m_lastAnimationLoop;
			
			if( delta >= 0.95f )
			{
				m_animator.SetBool(VAR_ATTACK, false);
				m_state = EState.PATROL;
				m_hasAttackDoneDamage = false;
			}
			else if( delta > AttackTriggerTime )
			{
				if( !m_hasAttackDoneDamage )
				{
					if( m_holdingProjectile != null )
					{
						Utils.PlaySoundRandomly(this.audio, SlashingSounds);
					
						m_holdingProjectile.Detach();
						
						Vector3 playerPos = Player.Instance.transform.localPosition;
						playerPos.y += Player.Instance.BoundingSize.y * 0.5f;
						
						Vector3 projPos = m_holdingProjectile.transform.localPosition;
						projPos.z = playerPos.z - 4.0f;
						
						Vector3 dir = playerPos - projPos;
						m_holdingProjectile.SetDirection(dir.normalized);						
						
						m_holdingProjectile = null;
					}
					
					m_hasAttackDoneDamage = true;
				}
			}
			
			SetAnimationCounter();	
		}
	}
	
	public override bool IsDying {
		get {
			return m_HP <= 0;
		}
	}
	
	public override bool IsOnWall
	{
		get { return false; }
	}
	
	public override bool IsNearWall
	{
		get { return false; }
	}
	
	public override bool IsClimbing
	{
		get { return false; }
	}
	
	protected virtual bool CanDoRangedAttack
	{
		get { return ( transform.localPosition.x >= (Global.WALL_MIN_X + BoundingSize.x * 0.25f) && transform.localPosition.x <= (Global.WALL_MAX_X - BoundingSize.x * 0.25f) ); }
	}
}
