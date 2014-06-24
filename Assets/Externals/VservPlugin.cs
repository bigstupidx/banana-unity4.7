			using UnityEngine;
			using System.Collections;
			
	public class VservPlugin {
	
		AndroidJavaObject aObj = null;
	
		// Use this for initialization
		public VservPlugin(){
			aObj = new AndroidJavaObject("mobi.vserv.android.plugin.UnityBridge");
		}
		
	 	public void RenderAd(string zoneId,int position)
	    {
	        aObj.Call("createBannerView", position);
	        aObj.Call("renderAd", zoneId);
	
	    }
	
		public void ShowAd(int position) {
				aObj.Call("createBannerView", position);
				aObj.Call("show");
		}

		public void DisplayAd(string zoneId){
			//AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			//AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity"); 
			//aObj = new AndroidJavaObject("mobi.vserv.android.plugin.UnityBridge");
			//ia.CallStatic("displayAd", new object[2] {currentActivity, zoneId}); 
			aObj.Call("displayAd", zoneId);
		}

		public void DisplayAd(string zoneId, string pAdType){
			aObj.Call("displayAd", zoneId, pAdType);
		}

		public void DisplayAd(string zoneId, int pAdOrientation){
			aObj.Call("displayAd", zoneId, pAdOrientation);
		}

		public void GetAd(string zoneId, int pOrientation) {
			aObj.Call("getAd", zoneId, pOrientation);
		}
		
		public void GetAd(string zoneId) {
			aObj.Call("getAd", zoneId);
		}
		
		public void OverlayAd() {
			aObj.Call("overlay");
		}

		public void OverlayAd(string pAdType) {
			aObj.Call("overlay", pAdType);
		}

		public void SetCacheNextAd(bool isCacheNextAd) {
			aObj.Call("setCacheNextAd", isCacheNextAd);
		}
		
		public void SetShowAt(string pShowAt) {
			aObj.Call("setShowAt", pShowAt);
		}

		// Set the name of the callback handler so the right component gets ad callbacks.
	    public void SetCallbackHandlerName(string callbackHandlerName)
	    {
			aObj.Call("setCallbackHandlerName", callbackHandlerName);
	    }
		
		public void DestroyApp() {
			aObj.Call("destroy");
		}

		public void StopRefreshAd() {
			if(aObj != null) {
			aObj.Call("stopRefresh");
			}
		}

		public void PauseRefreshAd() {
			if(aObj != null) {
				aObj.Call("pauseRefresh");
			}
		}

		public void ResumeRefreshAd() {
			if(aObj != null) {
				aObj.Call("resumeRefresh");
			}
		}

		public void HideBanner() {
			if(aObj != null) {
				aObj.Call("hideBanner");
			}
		}

		public void ShowBanner() {
			if(aObj != null) {
				aObj.Call("showBanner");
			}
		}
	}
