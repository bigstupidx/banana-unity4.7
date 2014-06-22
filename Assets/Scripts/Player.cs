using UnityEngine;
using System.Collections;

public class Player : Actor {

	private const int GESTURE_NONE = -1;
	private const int GESTURE_DRAG = 2;
	private const int GESTURE_SWIPE = 3;
	private const int GESTURE_SWIPE_DOWN = 4;
	private const int GESTURE_TAP = 5;

	private const int WEAPON_BOW = 0;
	private const int WEAPON_SWORD = 1;
	private const int WEAPON_CROSSBOW = 2;
	private const int WEAPON_AXE = 3;	

	private static Player g_instance = null;

	private float m_walkAccelerate;
	private bool m_isAttacking;
	private bool m_willAttack;	
	private int m_hasAttackDoneDamage;
	private float m_attackDirection;
	private int m_currentWeapon;
	private TrailRenderer m_currentWeaponTrail;
	private Arrow m_holdingArrow;
	private bool m_isDying;	
	
	private int m_equipedRangeWeapon;
	private int m_equipedMeleeWeapon;

	private float m_originalSpineZRotation;
	private float m_bendSpineTarget;

	private float m_touchBeginTime;
	private bool m_hasTouchBegan;
	private Vector2 m_lastTouchPosition;
	
	private float m_keyDelta;

	public Transform Spine;
	public Transform RightHand;
	public GameObject[] Weapons;
	public RuntimeAnimatorController[] Animations;		
	public AudioClip[] ShootingSounds;	
	
	public static Player Instance
	{
		get { return g_instance; }
	}

	protected override void Awake()
	{
		base.Awake ();
		g_instance = this;

		Reset ();
		
		Time.timeScale = 1.0f;
	}

	public void Reset()
	{
		m_equipedRangeWeapon = PlayerStash.Instance.PurchasedCrossbow > 0 ? WEAPON_CROSSBOW : WEAPON_BOW;
		m_equipedMeleeWeapon = PlayerStash.Instance.PurchasedAxe > 0 ? WEAPON_AXE : WEAPON_SWORD;
	
		UseWeapon(m_equipedRangeWeapon);
		m_walkAccelerate = 0.0f;
		m_attackDirection = 0.0f;
		m_isAttacking = false;
		m_willAttack = false;
		m_hasAttackDoneDamage = 0;
		m_hasAttackPlaySound = 0;
		m_keyDelta = 0.0f;

		m_bendSpineTarget = 0.0f;
		m_currentWeaponTrail = null;
		if (m_holdingArrow != null) {
			GameObject.Destroy(m_holdingArrow.gameObject);
		}
		m_holdingArrow = null;

		m_hasTouchBegan = false;

		m_animator.CrossFade (ANIM_IDLE, 0.1f, 0, 0.0f);
		m_animator.SetBool(VAR_ATTACK, false);
		m_animator.SetFloat(VAR_WALK, 0.0f);
		m_isDying = false;

		m_HP = MaxHP;
	}

	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}
		
		if( m_isDying )
		{
			return;
		}
		
		if( m_HP <= 0 )
		{
			Utils.PlaySoundRandomly(this.audio, KilledSounds);
		
			m_isDying = true;
			m_animator.CrossFade(ANIM_DYING, 0.0f, 0, 0.0f);
			return;
		}

		if (m_isAttacking) {
			if (m_currentWeaponTrail != null) {
				m_currentWeaponTrail.enabled = true;
			}

			if( IsCurrentAnim(ANIM_ATTACK) )
			{
				float delta = GetAnimationTime() - m_lastAnimationLoop;

				if( delta >= 0.8f )
				{
					if( !m_willAttack )
					{
						if( m_holdingArrow != null )
						{
							m_holdingArrow.Detach();
						}
						m_holdingArrow = null;

						if (m_currentWeaponTrail != null) {
							m_currentWeaponTrail.enabled = false;
						}

						m_animator.SetBool(VAR_ATTACK, false);
						ResetAnimationCounter();
						m_isAttacking = false;						
					}
					
					m_hasAttackDoneDamage = 0;	
					m_hasAttackPlaySound = 0;	
				}
				else if( IsRangeWeapon )
				{
					if( delta >= AttackTriggerTime )
					{
						if( m_holdingArrow != null )
						{
							m_holdingArrow.Detach();
							
							if( m_currentWeapon == WEAPON_CROSSBOW )
							{
								Arrow leftArrow = ProjectilesManager.Instance.Create<Arrow>(ProjectilesManager.ARROW, RightHand);
								leftArrow.Detach();
								leftArrow.DeltaX = -2;
								
								Arrow rightArrow = ProjectilesManager.Instance.Create<Arrow>(ProjectilesManager.ARROW, RightHand);
								rightArrow.Detach();
								rightArrow.DeltaX = 2;
							}							
						}
						m_holdingArrow = null;						
					}
					else
					{
						m_willAttack = false;
						m_hasAttackDoneDamage = 0;
						
						if( m_hasAttackPlaySound == 0 )
						{					
							m_hasAttackPlaySound = 1;
							Utils.PlaySoundRandomly(this.audio, ShootingSounds);
						}
					}
				}
				else
				{
					if( m_currentWeapon == WEAPON_SWORD )
					{
						if( delta >= AttackTriggerTime )
						{						
							if( m_hasAttackDoneDamage == 0 )
							{
								CheckMeleeHitEnemies();								
								++m_hasAttackDoneDamage;
							}						
						}
						else
						{
							m_willAttack = false;
							m_hasAttackDoneDamage = 0;
						}
					}
					// WEAPON_AXE
					else
					{
						if( delta >= 0.4f )
						{
							if( m_hasAttackDoneDamage == 2 )
							{
								CheckMeleeHitEnemies(2);								
								++m_hasAttackDoneDamage;
							}		
						}
						else if( delta >= 0.3f )
						{
							if( m_hasAttackDoneDamage == 1 )
							{
								m_hasAttackPlaySound = 0;
								m_attackDirection = -m_attackDirection;
								++m_hasAttackDoneDamage;
							}
						}
						else if( delta >= 0.1f )
						{						
							if( m_hasAttackDoneDamage == 0 )
							{
								CheckMeleeHitEnemies(2);
								
								++m_hasAttackDoneDamage;								
							}						
						}
						else 
						{
							m_hasAttackDoneDamage = 0;
						}
							
						if( delta < 0.5f )
						{
							m_willAttack = false;
						}
					}
					
					if( m_hasAttackPlaySound == 0 )
					{					
						m_hasAttackPlaySound = 1;
						Utils.PlaySoundRandomly(this.audio, SlashingSounds);
					}
				}
				
				if( m_holdingArrow == null && IsRangeWeapon && delta >= 0.0f && delta < AttackTriggerTime )
				{
					m_holdingArrow = ProjectilesManager.Instance.Create<Arrow>(ProjectilesManager.ARROW, RightHand);
				}

				SetAnimationCounter();
			}

			if( IsRangeWeapon )
			{
				FaceTo(180, 5);
				m_bendSpineTarget += (-60.0f - m_bendSpineTarget) * Time.deltaTime * 5;
			}
			else
			{
				FaceTo(m_attackDirection * 90.0f, 10);
				
				m_bendSpineTarget += -m_bendSpineTarget * Time.deltaTime * 5;
			}
		} else {

			if (m_currentWeaponTrail != null) {
				m_currentWeaponTrail.enabled = false;
			}

			float accelerateDist = Mathf.Abs(m_walkAccelerate);
			m_animator.SetFloat (VAR_WALK, accelerateDist);

			if( accelerateDist > 0.0001f )
			{
				Vector3 pos = transform.localPosition;

				if( m_walkAccelerate < 0.0f )
				{
					FaceTo(-90, accelerateDist * 10);
				}
				else
				{
					FaceTo(90, accelerateDist * 10);
				}

				pos.x += m_walkAccelerate * Time.deltaTime * Global.PLAYER_WALK_SPEED;
				if( pos.x < Global.WALL_MIN_X )
				{
					pos.x = Global.WALL_MIN_X;
				}
				else if( pos.x > Global.WALL_MAX_X )
				{
					pos.x = Global.WALL_MAX_X;
				}

				transform.localPosition = pos;
				
				if( !Spine.audio.isPlaying )
				{
					Spine.audio.Play();
				}
				Spine.audio.pitch = accelerateDist * 0.75f;
			}
			else			
			{
				if( Spine.audio.isPlaying )
				{
					Spine.audio.Stop();
				}
			}
			m_walkAccelerate *= 0.9f;
			m_bendSpineTarget += -m_bendSpineTarget * Time.deltaTime * 5;
		}
	}
	
	private void CheckMeleeHitEnemies(int damage = 1)
	{
		foreach( Actor enemy in EnemiesManager.Instance.Enemies )
		{
			if( TestAttackHit(m_attackDirection, enemy) )
			{
				enemy.TakeHit(damage);								
				ProjectilesManager.Instance.CreateOnActor(ProjectilesManager.BAM, enemy);
				
				Utils.PlaySoundRandomly(RightHand.audio, SlashingImpactSounds);
			}
		}
	}

	void LateUpdate()
	{		
		if( Mathf.Approximately(Time.timeScale, 0.0f) )
		{
			return;
		}
				
		BendSpine();
	}

	void FixedUpdate()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}
		
		if( StateManager.Instance.IsInLeaderboard || StateManager.Instance.IsInShop )
		{
			return;
		}

		int gestureId = GESTURE_NONE;
		Vector2 gestureDelta = Vector2.zero;
		Vector2 currentTouchPosition = Vector2.zero;
		bool gotTouchCycle = false;

		if( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);
			
			if( !Utils.IsTouchOnGUI((Vector3)touch.position) )
			{
				if( touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended )
				{
					if( m_hasTouchBegan )
					{
						if( Time.realtimeSinceStartup - m_touchBeginTime > Global.GESTURE_TIME )
						{							
							gestureDelta = touch.position - m_lastTouchPosition;
							if( Mathf.Abs(gestureDelta.x) >= Global.GESTURE_DRAG_THRESHOLD )
							{
								gestureId = GESTURE_SWIPE;
								m_attackDirection = Mathf.Sign(gestureDelta.x);
							}
							else
							{
								gestureId = GESTURE_DRAG;
							}
							m_lastTouchPosition = touch.position;
						}
					}
					else
					{
						m_hasTouchBegan = true;
						m_touchBeginTime = Time.realtimeSinceStartup;
						m_lastTouchPosition = touch.position;
					}
	
					currentTouchPosition = touch.position;
				}
				else
				{
					currentTouchPosition = touch.position;
					gotTouchCycle = m_hasTouchBegan;
					m_hasTouchBegan = false;
				}
			}
		}
		else
		{
		#if !UNITY_EDITOR
			gotTouchCycle = m_hasTouchBegan;
			m_hasTouchBegan = false;
			m_walkAccelerate = 0.0f;
		#endif			
		}

#if UNITY_EDITOR
		if (Input.GetMouseButton (0)) 
		{
			if( !Utils.IsTouchOnGUI(Input.mousePosition) )
			{
				if( m_hasTouchBegan )
				{
					if( Time.realtimeSinceStartup - m_touchBeginTime > Global.GESTURE_TIME )
					{
						gestureDelta = (Vector2)Input.mousePosition - m_lastTouchPosition;
						if( Mathf.Abs(gestureDelta.x) >= Global.GESTURE_DRAG_THRESHOLD )
						{
							gestureId = GESTURE_SWIPE;
							m_attackDirection = Mathf.Sign(gestureDelta.x);
						}
						else
						{
							gestureId = GESTURE_DRAG;
						}
						m_lastTouchPosition = Input.mousePosition;
					}
				}
				else
				{
					m_hasTouchBegan = true;
					m_touchBeginTime = Time.realtimeSinceStartup;
					m_lastTouchPosition = Input.mousePosition;
				}
				
				currentTouchPosition = Input.mousePosition;
			}			
		}
		else
		{
			currentTouchPosition = Input.mousePosition;
			gotTouchCycle = m_hasTouchBegan;
			m_hasTouchBegan = false;
			
			m_walkAccelerate = 0.0f;
		}
#endif

		if (gotTouchCycle && Time.realtimeSinceStartup - m_touchBeginTime <= Global.GESTURE_TIME) {
			gestureDelta = currentTouchPosition - m_lastTouchPosition;

			float distX = Mathf.Abs(gestureDelta.x);
			float distY = Mathf.Abs(gestureDelta.y);

			if( Mathf.Max(distX, distY) <= Global.GESTURE_DISTANCE_THRESHOLD )
			{
				gestureId = GESTURE_TAP;
			}
			else if( distY > distX && gestureDelta.y < 0 )
			{
				gestureId = GESTURE_SWIPE_DOWN;
			}
			else 
			{
				if( gestureDelta.x != 0.0f )
				{
					gestureId = GESTURE_SWIPE;
					m_attackDirection = Mathf.Sign(gestureDelta.x);
				}
			}
		}
		
#if UNITY_EDITOR
	
		if( Input.GetKey(KeyCode.Alpha1) )
		{
			++PlayerStash.Instance.PurchasedFire;
		}
		if( Input.GetKey(KeyCode.Alpha2) )
		{
			++PlayerStash.Instance.PurchasedSnow;
		}
		if( Input.GetKey(KeyCode.A) )
		{
			PlayerStash.Instance.PurchasedAxe = 1;
		}
		if( Input.GetKey(KeyCode.C) )
		{
			PlayerStash.Instance.PurchasedCrossbow = 1;
		}
		if( Input.GetKey(KeyCode.K) )
		{
			Player.Instance.TakeHit(1000);
		}
		if( Input.GetKeyUp(KeyCode.Q) )
		{
			Application.CaptureScreenshot("screenshot_" + (int)(Time.time*1000) + ".png");
		}

#endif
		if( m_keyDelta > 0.0f )
		{
			m_keyDelta -= Time.deltaTime * 1000;
			if( m_keyDelta < 0.0f )
			{
				m_keyDelta = 0.0f;
			}
		}
		else if( m_keyDelta < 0.0f )
		{
			m_keyDelta += Time.deltaTime * 1000;
			if( m_keyDelta > 0.0f )
			{
				m_keyDelta = 0.0f;
			}
		}
		if( Input.GetKey(KeyCode.LeftArrow) )
		{
			m_keyDelta = -1;
		}
		else if( Input.GetKey(KeyCode.RightArrow) )
		{
			m_keyDelta = 1;
		}
		
		if( Mathf.Abs(m_keyDelta) > 0.1f )
		{
			gestureId = GESTURE_DRAG;
			gestureDelta.x = m_keyDelta * Global.GESTURE_DISTANCE_THRESHOLD;	
			m_attackDirection = Mathf.Sign(m_keyDelta);
		}
		
		if( Input.GetKey(KeyCode.Space) )
		{
			gestureId = GESTURE_SWIPE_DOWN;
		}
		
		if( Input.GetKey(KeyCode.Return) )
		{
			gestureId = GESTURE_SWIPE;
			if( m_attackDirection == 0 )
			{
				m_attackDirection = 1;
			}
		}

		switch (gestureId) {
		case GESTURE_DRAG:
			if( Mathf.Abs(gestureDelta.x) < Global.GESTURE_DISTANCE_THRESHOLD )
			{
				if( Mathf.Abs(m_walkAccelerate) > 0.1f )
				{
					m_walkAccelerate += Mathf.Sign(m_walkAccelerate) * 0.5f;
				}
			}
			else
			{
				m_walkAccelerate += Mathf.Sign(gestureDelta.x) * 0.5f;
			}

			if( Mathf.Abs(m_walkAccelerate) > 1.0f )
			{
				m_walkAccelerate = Mathf.Sign(m_walkAccelerate);
			}
			break;

		case GESTURE_TAP:
		case GESTURE_SWIPE_DOWN:
			if( IsRangeWeapon || !m_isAttacking )
			{
				UseWeapon(m_equipedRangeWeapon);
				Attack();
			}
			break;

		case GESTURE_SWIPE:
			if( !IsRangeWeapon || !m_isAttacking )
			{
				UseWeapon(m_equipedMeleeWeapon);
				Attack();
			}
			break;
		}
	}

	private void Attack()
	{
		if (!m_animator.GetBool (VAR_ATTACK)) {
			ResetAnimationCounter();
		}

		if (!m_isAttacking) {
			m_animator.SetBool (VAR_ATTACK, true);
			m_animator.SetFloat (VAR_WALK, 0.0f);
			m_isAttacking = true;			
			
			m_hasAttackPlaySound = 0;
		}		

		m_willAttack = true;
	}

	public bool IsRangeWeapon
	{
		get { return m_currentWeapon == WEAPON_BOW || m_currentWeapon == WEAPON_CROSSBOW; }
	}

	private void BendSpine()
	{
		Quaternion quat = Spine.localRotation;
		Vector3 euler = quat.eulerAngles;
		euler.z += m_bendSpineTarget;
		quat.eulerAngles = euler;
		Spine.localRotation = quat;
	}

	private void UseWeapon(int weapon)
	{
		m_currentWeapon = weapon;

		for( int i=0; i<Weapons.Length; ++i )
		{
			if( i == m_currentWeapon )
			{
				Weapons[i].SetActive(true);
			}
			else
			{
				Weapons[i].SetActive(false);
			}
		}

		m_animator.runtimeAnimatorController = Animations [m_currentWeapon];
		m_currentWeaponTrail = Weapons [m_currentWeapon].GetComponentInChildren<TrailRenderer> ();
		
		AttackRange = m_currentWeapon == WEAPON_AXE ? 1.25f : 1;		
	}
	
	public override bool IsDying 
	{
		get { return m_isDying; }
	}
	
	public override bool IsOnWall
	{
		get { return true; }
	}
	
	public override bool IsNearWall
	{
		get { return false; }
	}
	
	public override bool IsClimbing
	{
		get { return false; }
	}
	
	public override Vector2 FeetPosition
	{
		get 
		{ 			
			return (Vector2)transform.localPosition;
		}
		
		set
		{
			transform.localPosition = (Vector3)value;
		}
	}
	
	public bool IsAttacking
	{
		get { return m_isAttacking; }
	}
	
	public bool TestAttackHit(float dir, Actor other)
	{
		if( other.IsDying )
		{
			return false;
		}
		
		if( other.IsOnWall || other.IsNearWall ) 
		{
			Vector3 myPos = FeetPosition;		
			Vector2 otherPos = other.FeetPosition;
			
			float myx1 = myPos.x;
			float myx2 = myPos.x + dir * (BoundingSize.x + AttackRange);
			
			if( myx1 > myx2 )
			{
				float temp = myx1;
				myx1 = myx2;
				myx2 = temp;
			}
			
			return Utils.TestRectsHit(
				myx1, myPos.y + BoundingSize.y * 0.1f,
				myx2, myPos.y + BoundingSize.y,
				otherPos.x - other.BoundingSize.x * 0.5f, otherPos.y,
				otherPos.x + other.BoundingSize.x * 0.5f, otherPos.y + other.BoundingSize.y);
		}
		
		return false;
	}
}
