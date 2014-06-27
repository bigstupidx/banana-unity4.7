using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class FacebookController : MonoBehaviour {

	private const float DEFAULT_OPERATION_DELAY = 1.0f;
	private const float DEFAULT_OPERATION_TIMEOUT = 30.0f;

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
		POST_TO_WALL,
	}
	
	public enum EWorkingState
	{
		NONE = 0,
		START,
		ONGOING,
		DONE,
	}
	
	private class WorkThread
	{
		public EOperation op;
		public float timeout;
		public EWorkingState state;
		
		public WorkThread(EOperation nop, float ntimeout)
		{
			op = nop;
			timeout = ntimeout;
			state = EWorkingState.START;
		}
	}
	
	private Queue<EOperation> m_operations = new Queue<EOperation>();
	private List<WorkThread> m_onGoingOperations = new List<WorkThread>();
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
	
	public string MyID
	{
		get { return FB.UserId; }
	}
	
	public string MyFullName
	{
		get { return m_facebookName; }
	}
	
	public void Operate(EOperation op, bool isOneAtATime = true)
	{
		if( !isOneAtATime || !IsWorking(op) )
		{
			m_operations.Enqueue(op);
		}
	}
	
	public bool IsWorking(EOperation op)
	{
		if( m_operations.Contains(op) )
		{
			return true;
		}
		
		foreach( WorkThread work in m_onGoingOperations )
		{
			if( work.op == op )
			{
				return true;
			}
		}
		
		return false;
	}
	
	public bool CanOperateLogIn()
	{
		return IsFunctional && !IsLoggedIn && !IsWorking(EOperation.LOG_IN);
	}
	
	public bool CanOperateInviteFriends()
	{
		return IsLoggedIn && !IsWorking(EOperation.INVITE_FRIENDS);
	}
	
	public bool CanOperatePost()
	{
		return IsLoggedIn && !IsWorking(EOperation.POST_TO_WALL) && PlayerStash.Instance.CurrentScore > 0;
	}
	
	void Update()
	{		
		if( m_operationDelay > 0.0f )
		{			
			m_operationDelay -= Time.fixedDeltaTime;
			return;
		}
		
		if( m_operations.Count > 0 )
		{
			EOperation op = m_operations.Dequeue();
			m_operationDelay = DEFAULT_OPERATION_DELAY;
			
			m_onGoingOperations.Add(new WorkThread(op, DEFAULT_OPERATION_TIMEOUT));			
		}
		
		for( int i = m_onGoingOperations.Count - 1; i >= 0; --i )
		{
			WorkThread work = m_onGoingOperations[i];
			if( work.state == EWorkingState.DONE )
			{
				m_onGoingOperations.RemoveAt(i);
			}
			else
			{
				CommitOperation(work);
			}
		}
	}
	
	private void CommitOperation(WorkThread work)
	{
		switch( work.op )
		{
		case EOperation.INIT:
			OpInit(work);				
			break;
			
		case EOperation.LOG_IN:
			OpLogIn(work);		
			break;
			
		case EOperation.INVITE_FRIENDS:
			OpInviteFriends(work);
			break;
			
		case EOperation.MY_INFO:
			OpMyInfo(work);
			break;
			
		case EOperation.POST_TO_WALL:
			OpPostToWall(work);
			break;
		}			
		
		if( work.state == EWorkingState.ONGOING )
		{
			work.timeout -= Time.fixedDeltaTime;
			if( work.timeout <= 0.0f )
			{
				work.state = EWorkingState.DONE;
			}
		}
	}
	
	// Operations
	void OpInit(WorkThread work)
	{
		if( work.state == EWorkingState.START )
		{
			work.state = EWorkingState.ONGOING;
			
			FB.Init(() => 
			{ 
				m_isFunctional = true;
				work.state = EWorkingState.DONE;
			});			
		}
	}
	
	void OpLogIn(WorkThread work)
	{
		if( !IsFunctional )
		{
			work.state = EWorkingState.DONE;
			return;
		}
		
		if( IsLoggedIn )
		{			
			work.state = EWorkingState.DONE;
			
			Operate(EOperation.MY_INFO);
			return;
		}		
		
		if( work.state == EWorkingState.START )
		{
			work.state = EWorkingState.ONGOING;
			
			FB.Login("", (result) => 
			{
				if( FB.IsLoggedIn )
				{
					Operate(EOperation.MY_INFO);
				}
				work.state = EWorkingState.DONE;
			});		
		}
	}
	
	void OpInviteFriends(WorkThread work)
	{
		if( !IsLoggedIn )
		{
			work.state = EWorkingState.DONE;
			return;
		}
		
		if( work.state == EWorkingState.START )
		{
			work.state = EWorkingState.ONGOING;
			
			FB.AppRequest(
				to: null,
				filters : "",
				excludeIds : null,
				message: "Castle Attack is awesome! Check it out!",
				title: "Play Castle Attack with me!",
				callback: (result) =>
				{
					work.state = EWorkingState.DONE;
				}
				); 
		}			
	}
	
	void OpPostToWall(WorkThread work)
	{
		if( !IsLoggedIn )
		{
			work.state = EWorkingState.DONE;
			return;
		}
		
		if( work.state == EWorkingState.START )
		{
			work.state = EWorkingState.ONGOING;
			
			FB.Feed(                                                                                                                 
			        linkCaption: "I scored " + PlayerStash.Instance.CurrentScore + " in Castle Attack! Can you beat it?",               
			        picture: "http://kinoastudios.com/CastleDefender/logolarge.jpg",
			        linkName: "Beat me at Castle Attack!",                                                                 
			        link: "http://www.facebook.com/pages/Kinoastudios/397813703654774",
			        callback: (result) =>
			        {
						work.state = EWorkingState.DONE;
			        }
			        );  
		}			
	}
	
	void OpMyInfo(WorkThread work)
	{
		if( !IsLoggedIn )
		{
			work.state = EWorkingState.DONE;
			return;
		}
		
		if( work.state == EWorkingState.START )
		{
			work.state = EWorkingState.ONGOING;
			
			FB.API("/me?fields=name", Facebook.HttpMethod.GET, (result) =>
			{
				Dictionary<string, object> pairs = Json.Deserialize(result.Text) as Dictionary<string, object>;
				object nameH;
				if( pairs.TryGetValue("name", out nameH) )
				{
					m_facebookName = (string)nameH;
				}
				
				Debug.Log("My id: " + FB.UserId + "\nMy name: " + m_facebookName);
				work.state = EWorkingState.DONE;
			});
		}
	}
}
