using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class EditorUtilities
{
	[MenuItem("Tools/Clear Player Prefs")]
	private static void BuildStandAlone()  {
		PlayerPrefs.DeleteAll();
	}
}