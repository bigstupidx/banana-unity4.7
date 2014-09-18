using UnityEngine;
using System.Collections;

public class ScreenshotCapture : MonoBehaviour {

	public static bool CaptureRequested = false;
	
	private int screenshotCount = 0;
	private string screenshotFilename;
		
	void Start()
	{
		if( Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor )
		{
			FindScreenshotCount();
		}
	}
	
	void Update()
	{
		if( (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && Input.GetKeyUp(KeyCode.F9) )
		{
			ScreenshotCapture.CaptureRequested = true;
		}
	}
		
	void OnPostRender()
	{	
		if( (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) && CaptureRequested )
		{
			Application.CaptureScreenshot(Application.dataPath + "/../" + screenshotFilename);
			FindScreenshotCount();
		
			CaptureRequested = false;
		}		
	}
	
	private void FindScreenshotCount()
	{
		do
		{
			screenshotCount++;
			screenshotFilename = "screenshot" + screenshotCount + ".png";
			
		} while (System.IO.File.Exists(screenshotFilename));
	}
}
