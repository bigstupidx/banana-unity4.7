using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BuildPlatform
{
	static string m_sDDLLocation = Application.dataPath + "/../ClassLibrary/";
	
	static string m_sBuildPath =  Application.dataPath + "/../Builds/CastleAttack.exe";
	
	static string[] m_Scenes = { "Assets/Scenes/Splash.unity", "Assets/Scenes/Main.unity" };
	
	[MenuItem("Tools/Build StandAlone")]
	private static void BuildStandAlone()  {
		EditorUtilities.GenerateVersion();
		
		UseBuildDLLs();
		
		//build the file 
		BuildOptions Options = BuildOptions.None;
		BuildPipeline.BuildPlayer(m_Scenes, m_sBuildPath, BuildTarget.StandaloneWindows, Options);
		
		RestoreWorkDLLs();
	}

	[MenuItem("Tools/Build Android")]
	private static void BuildAndroid()  {
		EditorUtilities.GenerateVersion();
	
		UseBuildDLLs();
		
		//build the file 
		BuildOptions Options = BuildOptions.None;		
		PlayerSettings.Android.keystoreName = Application.dataPath + "/../kinoastudios.keystore";
		PlayerSettings.Android.keystorePass = "123456";
		PlayerSettings.Android.keyaliasName = "kinoastudios";
		PlayerSettings.Android.keystorePass = "123456";
				
		PlayerSettings.keystorePass = "123456";
		PlayerSettings.keyaliasPass = "123456";
		
		BuildPipeline.BuildPlayer(m_Scenes, Application.dataPath + "/../Builds/CastleAttack.apk", BuildTarget.Android, Options);
		
		RestoreWorkDLLs();
	}	
	
	private static void UseBuildDLLs()
	{
		// Use working DLLs
		File.Copy(m_sDDLLocation + "NGUI_Build.dll", Application.dataPath + "/Externals/NGUI/NGUI.dll", true);
		
		//re-import the database
		AssetDatabase.Refresh();
	}
	
	private static void RestoreWorkDLLs()
	{
		//rename back
		File.Copy(m_sDDLLocation + "NGUI_Work.dll", Application.dataPath + "/Externals/NGUI/NGUI.dll", true);
		
		//re re-import the database
		AssetDatabase.Refresh();   
	}
}