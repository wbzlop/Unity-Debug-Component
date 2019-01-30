using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


/// <summary>
/// 调试工具
/// </summary>
public class DebugTool : MonoBehaviour {

	enum OperationType
	{
		openBox = 		1<< 1,
		showLog = 		1<< 2,
		showError = 	1<< 3,
		showException = 1<< 4,
		showAssert = 	1<< 5,
		showWarning = 	1<< 6,
		showFPS = 		1<< 7,
		showStack = 	1<< 8,
		showMono = 		1<< 9,
		showMemory = 	1<< 10,

	}

	private long status = 0;

	private GUISkin skin;

	private RectOffset padding;

	private List<LogInfo> logList;

	private Vector2 pos;

	private float fpsMeasuringDelta = 1.0f;

	private float timePassed;
	private int m_FrameCount = 0;
	private float m_FPS = 0.0f;

	private float memoryMeasuringDelta = 1.0f;
	private float memorytimePassed;

	// DPI scaling
	private float? scaleFactor;

	private static Dictionary<string,System.Action> extendButtons = new Dictionary<string, System.Action> ();
	private static List<ToogleInfo> extendToogles = new List<ToogleInfo>();

	protected float ScaleFactor
	{
		get
		{
			if (!this.scaleFactor.HasValue)
			{
				this.scaleFactor = Screen.dpi / 160;
			}

			return this.scaleFactor.Value;
		}
	}


	/// <summary>
	/// 增加按钮调试事件
	/// </summary>
	/// <param name="title">Title.</param>
	/// <param name="action">Action.</param>
	public static void AddButton(string title,System.Action action)
	{
		if(extendButtons.ContainsKey(title))
		{
			Debug.LogError ("已经存在相同名称的按钮:" + title);
			return;
		}

		extendButtons [title] = action;

	}

	/// <summary>
	/// 增加状态调试事件
	/// </summary>
	/// <param name="title">Title.</param>
	/// <param name="action">Action.</param>
	public static void AddToogle(string title,System.Action<bool> action)
	{
		ToogleInfo info = new ToogleInfo (title,false,action);

		extendToogles.Add (info);
	}

	void OnGUI()
	{
		
		SetGUISkin ();

		GUILayout.BeginHorizontal ();

		if(GetOperation(OperationType.showFPS))
		{
			GUI.color = Color.red;
			GUILayout.Label ( string.Format("FPS:{0:00.0}",m_FPS));
			GUI.color = Color.white;
		}
		if(GetOperation(OperationType.showMono))
		{
			GUI.color = Color.red;
			GUILayout.Label ( string.Format("MONO内存:{0:0.00}M",Profiler.GetMonoUsedSizeLong() / 1048576.0f));
			GUI.color = Color.white;


		}
		GUILayout.EndHorizontal ();
		if(GetOperation(OperationType.showMemory))
		{
			GUI.color = Color.red;
			GUILayout.Label ( string.Format("实际内存:{0:0.00}M",Profiler.GetTotalAllocatedMemoryLong() / 1048576.0f));
			GUILayout.Label ( string.Format("临时内存:{0:0.00}M",Profiler.GetTotalUnusedReservedMemoryLong() / 1048576.0f));
			GUILayout.Label ( string.Format("总内存:{0:0.00}M",Profiler.GetTotalReservedMemoryLong() / 1048576.0f));
			GUI.color = Color.white;
		}


	
		if(GetOperation(OperationType.openBox))
		{ 
			GUILayout.BeginHorizontal ();

			if(GUILayout.Button("关闭Debug设置"))
			{
				RemoveOperation (OperationType.openBox);
			}


			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			//=================
			GUILayout.BeginVertical ();

			DrawToogle (OperationType.showLog,"显示Log");
			DrawToogle (OperationType.showError,"显示Error");
			DrawToogle (OperationType.showException,"显示Exception");
			DrawToogle (OperationType.showWarning,"显示Warning");
			DrawToogle (OperationType.showAssert,"显示Assert");
			DrawToogle (OperationType.showFPS,"显示FPS");
	
			GUILayout.EndVertical ();

			//=================

			GUILayout.BeginVertical ();
	
			DrawToogle (OperationType.showStack,"显示堆栈");
			DrawToogle (OperationType.showMono,"显示mono");
			DrawToogle (OperationType.showMemory,"显示内存");

			if (GUILayout.Button ("清除log")) {

				logList.Clear ();
			}
				
			GUILayout.EndVertical ();

			GUILayout.EndHorizontal ();





			//===========拓展调试==========
			//按钮
			foreach (string key in extendButtons.Keys) {

				if(GUILayout.Button(key))
				{
					extendButtons [key] ();
				}

			}
			//toogle
			foreach (ToogleInfo info in extendToogles) {
				if (info.flag)
					GUI.color = Color.green;
				bool temp = GUILayout.Toggle (info.flag,info.title);
				if (temp != info.flag)
					info.flag = temp;
	
				GUI.color = Color.white;
			}

		}
		else
		{
			GUILayout.BeginHorizontal ();
			if(GUILayout.Button("Debug设置"))
			{
				AddOperation (OperationType.openBox);

			}

			GUILayout.EndHorizontal ();
		}


	

	//===============打印 log ==================
		pos = GUILayout.BeginScrollView (pos);
		for (int i = 0; i < logList.Count; i++) {

			LogInfo info = logList[i];

			switch (info.type) {
			case LogType.Log:
				GUI.color = Color.white;
				WriteLog (OperationType.showLog,info);

				break;
			case LogType.Warning:
				GUI.color = Color.yellow;
				WriteLog (OperationType.showWarning,info);

				break;
			case LogType.Error:
				GUI.color = Color.red;
				WriteLog (OperationType.showError,info);

				break;
			case LogType.Assert:
				GUI.color = Color.red;
				WriteLog (OperationType.showAssert,info);

				break;
			case LogType.Exception:
				GUI.color = Color.red;
				WriteLog (OperationType.showException,info);

				break;
			default:
				break;
			}

			GUI.color = Color.white;


		}

		GUILayout.EndScrollView ();
	}

	private void DrawToogle(OperationType type,string title)
	{
		bool flag = GetOperation (type);
		if (flag)
			GUI.color = Color.green;
		flag = GUILayout.Toggle (GetOperation(type),title);
		SetOperation (type, flag);
		GUI.color = Color.white;
	}


	private void WriteLog(OperationType type,LogInfo info)
	{
		string infoStr = string.Empty;
		if(GetOperation(type))
		{			
			infoStr = info.log;
			if(GetOperation(OperationType.showStack))
			{
				infoStr = info.log + "\n" + info.stack;
			}

			if(!infoStr.Equals(string.Empty))
			{
				GUILayout.Label (infoStr);
			}

		}
	}

	// Use this for initialization
	void Start () {

		padding = new RectOffset (20,20,20,20);

		timePassed = 0.0f;
		memorytimePassed = 0;

	}

	void Awake()
	{
		status = long.Parse(PlayerPrefs.GetString ("DEBUG","0"));

		logList = new List<LogInfo> ();
		Application.logMessageReceived += (string condition, string stackTrace, LogType type) => {

			LogInfo logInfo = new LogInfo(condition,stackTrace,type);
			logList.Add(logInfo);

		};
	}

	void OnApplicationQuit()
	{
		PlayerPrefs.SetString ("DEBUG",status.ToString());
	}
	
	// Update is called once per frame
	void Update () {

		if(GetOperation(OperationType.showFPS))
		{
			m_FrameCount = m_FrameCount + 1;
			timePassed = timePassed + Time.deltaTime;

			if (timePassed > fpsMeasuringDelta)
			{
				m_FPS = m_FrameCount / timePassed;

				timePassed = 0.0f;
				m_FrameCount = 0;
			}
		}
	}

	private bool GetOperation(OperationType type)
	{
		return (status & (long)type) != 0;
	}

	private void RemoveOperation(OperationType type)
	{
		status &=  ~(long)type;
	}

	private void AddOperation(OperationType type)
	{
		status |= (long)type;
	}

	private void SetOperation(OperationType type,bool flag)
	{
		if(flag)
		{
			AddOperation (type);
		}
		else
		{
			RemoveOperation (type);
		}
	}

	private void SetGUISkin()
	{
		GUI.skin.button.fontSize = (int)(ScaleFactor * 16);
		GUI.skin.button.padding = padding;
		GUI.skin.toggle.fontSize = (int)(ScaleFactor * 16);
		GUI.skin.toggle.padding = padding;

		GUI.skin.label.fontSize = (int)(ScaleFactor * 14);
		GUI.skin.label.padding = padding;

		GUI.skin.verticalScrollbar.fixedWidth = (int)(ScaleFactor * 16);
		GUI.skin.verticalScrollbarThumb.fixedWidth = (int)(ScaleFactor * 15);

	}



	class LogInfo
	{
		public string log {
			get;
			set;
		}
		public string stack {
			get;
			set;
		}
		public LogType type {
			get;
			set;
		}
		public LogInfo(string _log,string _stack,LogType _type)
		{
			log 	= _log;
			stack 	= _stack;
			type 	= _type;
		}
	}

	class ToogleInfo
	{
		private bool Flag = false;
		public string title {
			get;
			set;
		}

		public bool flag {
			get{ return Flag;}
			set{ 
				Flag = value;
				if(action != null)
				{
					action (Flag);
				}
			}
		}

		public System.Action<bool> action {
			get;
			set;
		}

		public ToogleInfo(string _title,bool _flag,System.Action<bool> _action)
		{
			title = _title;
			Flag = _flag;
			action = _action;
		}
	}


}
