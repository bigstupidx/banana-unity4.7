using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class FacebookController : MonoBehaviour {

	private const float DEFAULT_OPERATION_DELAY = 2.0f;

	private static FacebookController g_instance = null;
	
	public static FacebookController Instance
	{
		get { return g_instance; }
	}	
	
	public enum EOperation
	{
		NONE = 0,
		INIT,
		LOG_IN,		
		INVITE_FRIENDS,
		MY_INFO,
	}
	
	private EOperation m_currentOperation = EOperation.NONE;
	private Queue<EOperation> m_operations = new Queue<EOperation>();
	private float m_operationDelay = 0.0f;
	
	private bool m_isFunctional = false;
	private string m_facebookName = "";
	
	void Awake()
	{
		g_instance = this;
	}
	
	void Start()
	{
		m_isFunctional = false;
		m_operationDelay = 0.0f;
		m_facebookName = "";
		m_operations.Enqueue(EOperation.INIT);	
	}
	
	// Utilities
	public bool IsFunctional
	{
		get { return m_isFunctional; }
	}
	
	public bool IsLoggedIn
	{
		get { return FB.IsLoggedIn; }
	}
	
	public bool HasMyInfo
	{
		get { return m_facebookName.Length > 0; }
	}
	
	public void Operate(EOperation op, bool isOneAtATime = true)
	{
		if( !isOneAtATime || !m_operations.Contains(op) )
		{
			m_operations.Enqueue(op);
		}
	}
	
	void Update()
	{
		m_operationDelay -= Time.deltaTime;
		if( m_operationDelay > 0.0f )
		{			
			return;
		}
		
		m_currentOperation = EOperation.NONE;
		if( m_operations.Count > 0 )
		{
			m_currentOperation = m_operations.Dequeue();
			m_operationDelay = DEFAULT_OPERATION_DELAY;
			
			CommitOperation(m_currentOperation);
		}
	}
	
	private void CommitOperation(EOperation op)
	{
		switch( m_currentOperation )
		{
		case EOperation.INIT:
			OpInit();				
			break;
			
		case EOperation.LOG_IN:
			OpLogIn();		
			break;
			
		case EOperation.INVITE_FRIENDS:
			OpInviteFriends();
			break;
			
		case EOperation.MY_INFO:
			OpMyInfo();
			break;
		}	
	}
	
	// Operations
	void OpInit()
	{
		FB.Init(() => 
		{ 
			m_isFunctional = true;
		});
	}
	
	void OpLogIn()
	{
		if( !IsFunctional )
		{
			return;
		}
		
		if( IsLoggedIn )
		{
			return;
		}		
		
		FB.Login("publish_actions", (result) => 
		{
			Operate(EOperation.MY_INFO);
		});		
	}
	
	void OpInviteFriends()
	{
		if( !IsLoggedIn )
		{
			return;
		}
		
		FB.AppRequest(
			to: null,
			filters : "",
			excludeIds : null,
			message: "Castle Attack is awesome! Check it out!",
			title: "Play Castle Attack with me!",
			callback: (result) =>
			{
			}
			); 			
	}
	
	void OpMyInfo()
	{
		if( !IsLoggedIn )
		{
			return;
		}
		
		FB.API("/me?fields=name", Facebook.HttpMethod.GET, (result) =>
		{
			Dictionary<string, object> pairs = Json.Deserialize(result.Text) as Dictionary<string, object>;
			object nameH;
			if( pairs.TryGetValue("name", out nameH) )
			{
				m_facebookName = (string)nameH;
			}
			
			Debug.Log("My id: " + FB.UserId + "\nMy name: " + m_facebookName);
		});
	}
}
