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
	
	private const float ATTACK_RANGE = 1.0f;

	private static Player g_instance = null;

	private float m_walkAccelerate;
	private bool m_isAttacking;
	private bool m_willAttack;
	private bool m_hasAttackDoneDamage;
	private float m_attackDirection;
	private int m_currentWeapon;
	private TrailRenderer m_currentWeaponTrail;
	private Arrow m_holdingArrow;
	private bool m_isDying;	

	private float m_originalSpineZRotation;
	private float m_bendSpineTarget;

	private float m_touchBeginTime;
	private bool m_hasTouchBegan;
	private Vector2 m_lastTouchPosition;

	public Transform Spine;
	public Transform RightHand;
	public GameObject[] Weapons;
	public RuntimeAnimatorController[] Animations;

	public static Player Instance
	{
		get { return g_instance; }
	}

	protected override void Awake()
	{
		base.Awake ();
		g_instance = this;

		Reset ();
	}

	public void Reset()
	{
		UseWeapon(WEAPON_BOW);
		m_walkAccelerate = 0.0f;
		m_attackDirection = 0.0f;
		m_isAttacking = false;
		m_willAttack = false;
		m_hasAttackDoneDamage = false;

		m_bendSpineTarget = 0.0f;
		m_currentWeaponTrail = null;
		if (m_holdingArrow != null) {
			GameObject.Destroy(m_holdingArrow.gameObject);
		}
		m_holdingArrow = null;

		m_hasTouchBegan = false;

		m_animator.CrossFade (ANIM_IDLE, 0.25f, 0, 0.0f);
		m_isDying = false;

		Weapons [0].SetActive (true);
		for (int i=1; i<Weapons.Length; ++i) {
			Weapons[i].SetActive(false);
		}

		m_HP = 1;
		m_HP++;
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
					
					m_hasAttackDoneDamage = false;
				}
				else if( delta >= 0.3f )
				{
					if( m_holdingArrow != null )
					{
						m_holdingArrow.Detach();
					}
					m_holdingArrow = null;
					
					if( !IsRangeWeapon && !m_hasAttackDoneDamage )
					{
						foreach( Actor enemy in EnemiesManager.Instance.Enemies )
						{
							if( TestAttackHit(enemy) )
							{
								enemy.TakeHit(1);
							}
						}
						
						m_hasAttackDoneDamage = true;
					}
				}
				else
				{
					m_willAttack = false;
					m_hasAttackDoneDamage = false;
				}

				if( delta >= 0.0f && delta <= 0.8f )
				{
					if( IsRangeWeapon && delta < 0.3f && m_holdingArrow == null )
					{
						m_holdingArrow = ProjectilesManager.Instance.Create<Arrow>(ProjectilesManager.ARROW, RightHand);
					}
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
			}
			m_walkAccelerate *= 0.5f;
			m_bendSpineTarget += -m_bendSpineTarget * Time.deltaTime * 5;
		}
	}

	void LateUpdate()
	{		
		BendSpine();
	}

	void FixedUpdate()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
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
							gestureId = GESTURE_DRAG;
							gestureDelta = touch.position - m_lastTouchPosition;
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
						gestureId = GESTURE_DRAG;
						gestureDelta = (Vector2)Input.mousePosition - m_lastTouchPosition;
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

		switch (gestureId) {
		case GESTURE_DRAG:
			if( Mathf.Abs(gestureDelta.x) < Global.GESTURE_DISTANCE_THRESHOLD )
			{
				if( Mathf.Abs(m_walkAccelerate) > 0.2f )
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
				UseWeapon(WEAPON_BOW);
				Attack();
			}
			break;

		case GESTURE_SWIPE:
			if( !IsRangeWeapon || !m_isAttacking )
			{
				UseWeapon(WEAPON_SWORD);
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
		}

		m_willAttack = true;
	}

	private bool IsRangeWeapon
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
	
	public bool TestAttackHit(Actor other)
	{
		if( other.IsDying )
		{
			return false;
		}
		
		if( other.IsOnWall || other.IsNearWall ) 
		{
			Vector3 myPos = FeetPosition;		
			Vector2 otherPos = other.FeetPosition;
			
			return Utils.TestRectsHit(
				myPos.x, myPos.y + BoundingSize.y * 0.5f,
				myPos.x + m_attackDirection * (BoundingSize.x + ATTACK_RANGE), myPos.y + BoundingSize.y,
				otherPos.x - other.BoundingSize.x * 0.5f, otherPos.y,
				otherPos.x + other.BoundingSize.x * 0.5f, otherPos.y + other.BoundingSize.y);
		}
		
		return false;
	}
}
