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
	private int m_currentWeapon;

	private float m_originalSpineZRotation;
	private float m_bendSpineTarget;

	private float m_touchBeginTime;
	private bool m_hasTouchBegan;
	private Vector2 m_lastTouchPosition;

	public Transform Spine;

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
		m_currentWeapon = WEAPON_BOW;
		m_walkAccelerate = 0.0f;
		m_isAttacking = false;
		m_willAttack = false;

		m_bendSpineTarget = 0.0f;

		m_hasTouchBegan = false;

		m_animator.CrossFade ("Idle", 0.25f, 0, 0.0f);

		m_HP = 10;
		m_HP++;
	}

	void Update()
	{
		if (Mathf.Approximately (Time.timeScale, 0.0f)) {
			return;
		}

		if (m_isAttacking) {

			float delta = GetAnimationTime() - m_lastAnimationLoop;

			if( delta >= 0.8f )
			{
				if( !m_willAttack )
				{
					m_animator.SetBool("Attack", false);
					ResetAnimationCounter();
					m_isAttacking = false;
				}
			}
			else if( delta >= 0.5f )
			{
				// TODO
			}
			else
			{
				m_willAttack = false;
			}

			SetAnimationCounter();

			if( IsRangeWeapon )
			{
				FaceTo(180, 5);
				m_bendSpineTarget += (-60.0f - m_bendSpineTarget) * Time.deltaTime * 5;
			}
			else
			{
				m_bendSpineTarget += -m_bendSpineTarget * Time.deltaTime * 5;
			}
		} else {

			float accelerateDist = Mathf.Abs(m_walkAccelerate);
			m_animator.SetFloat ("Walk", accelerateDist);

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
		bool gotTouchCycle = false;

		if( Input.touchCount > 0 )
		{
			Touch touch = Input.GetTouch(0);

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
			}
			else
			{
				gotTouchCycle = m_hasTouchBegan;
				m_hasTouchBegan = false;
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
		if (Input.GetMouseButton (0)) {
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
		}
		else
		{
			gotTouchCycle = m_hasTouchBegan;
			m_hasTouchBegan = false;
		}
#endif

		if (gotTouchCycle && Time.realtimeSinceStartup - m_touchBeginTime <= Global.GESTURE_TIME) {
			gestureDelta = (Vector2)Input.mousePosition - m_lastTouchPosition;

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
				gestureId = GESTURE_SWIPE;
			}
		}

		switch (gestureId) {
		case GESTURE_DRAG:
			if( gestureDelta.x == 0.0f )
			{
				m_walkAccelerate += Mathf.Sign(m_walkAccelerate) * 0.5f;
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
			m_currentWeapon = WEAPON_BOW;
			Attack();
			break;

		case GESTURE_SWIPE:

			break;
		}
	}

	private void Attack()
	{
		if (!m_animator.GetBool ("Attack")) {
			ResetAnimationCounter();
		}

		m_animator.SetBool("Attack", true);
		m_animator.SetFloat ("Walk", 0.0f);
		m_isAttacking = true;
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
}
