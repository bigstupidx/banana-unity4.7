function ORMMAReady11()
{
VservAdControllerInterface.showMsg('*********ORMMAReady11 Called');
if(typeof(vservImpressionNotifyURL)!='undefined' && typeof(vservImpressionNotifyURL!=null) && vservImpressionNotifyURL instanceof Array)
{
	VservAdControllerInterface.showMsg('inside recordImpression  Array--notifyOnce');
	for(var i in vservImpressionNotifyURL)
	{
		VservAdControllerInterface.addNotifyUrlsToHashTable(vservImpressionNotifyURL[i]);
	}
	VservAdControllerInterface.notifyOnce();
}
else if(typeof(vservImpressionNotifyURL)!='undefined' && typeof(vservImpressionNotifyURL!=null))
{
	VservAdControllerInterface.showMsg('inside recordImpression Single--notifyOnce');
	VservAdControllerInterface.addNotifyUrlsToHashTable(vservImpressionNotifyURL);
	VservAdControllerInterface.notifyOnce();
}
VservAdControllerInterface.showSkipPage();
}