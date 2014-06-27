using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OnePF;

public class ShopController : MonoBehaviour {

	public const int ITEM_CROSSBOW = 0;
	public const int ITEM_AXE = 1;
	public const int ITEM_SNOW = 2;
	public const int ITEM_FIRE = 3;
	public const int ITEM_NUM = 4;

	private static ShopController g_instance = null;
	
	public static ShopController Instance
	{
		get { return g_instance; }
	}
	
	void Awake()
	{
		g_instance = this;
		
		Items = new Item[ITEM_NUM];
		
		// TODO - add more items to be done here
		Items[ITEM_CROSSBOW] = new Item(ITEM_CROSSBOW, "crossbow", "Crossbow", "The ultimate range weapon crossbow can shoot 3 arrows at a time!", false, 1);
		Items[ITEM_CROSSBOW].skuGooglePlay = "crossbow";
		Items[ITEM_CROSSBOW].skuNokia = "1254489";
		Items[ITEM_AXE] = new Item(ITEM_AXE, "axe", "Axe", "The axe wielder can do attack on both sides.", false, 1);
		Items[ITEM_AXE].skuGooglePlay = "axe";
		Items[ITEM_AXE].skuNokia = "1254488";
		Items[ITEM_SNOW] = new Item(ITEM_SNOW, "blizzard", "Blizzard spells", "Buy a combo of [00ff00]10[-] blizzard spells. The blizzard can freeze up all enemies for a long while.", true, 10);
		Items[ITEM_SNOW].skuGooglePlay = "blizzard";
		Items[ITEM_SNOW].skuNokia = "1254492";
		Items[ITEM_FIRE] = new Item(ITEM_FIRE, "apocalypse", "Apocalypse spells", "Buy a combo of [00ff00]5[-] apocalypse spells. The spell can virtually hurt all enemies on sight.", true, 5);
		Items[ITEM_FIRE].skuGooglePlay = "apocalypse";
		Items[ITEM_FIRE].skuNokia = "1254491";
	}
	
	public class Item	
	{
		public int id;
		public string title;
		public string description;
		public string pricetag;
		public int purchased;	
		public int quantity;
		public bool isBuyable;	
		public string sku;
		public string skuGooglePlay;
		public string skuNokia;
		public bool isConsumable;
		
		public Item(int nid, string nsku, string ntitle, string desc, bool isCon, int nquantity)
		{
			id = nid;
			title = ntitle;
			description = desc;
			pricetag = "";
			purchased = 0;
			isBuyable = true;
			sku = nsku;
			isConsumable = isCon;		
			quantity = nquantity;	
		}
	}
	
	public Item[] Items;
	
	public string ErrorMessage = "";
	public bool GotError = false;
	public bool HasInfo = false;		
	
	private int m_initStep = 0;

	// Use this for initialization
	void Start () {		
		// Map sku for different stores
		foreach( Item item in Items )
		{
			OpenIAB.mapSku(item.sku, OpenIAB_Android.STORE_GOOGLE, item.skuGooglePlay);
			OpenIAB.mapSku(item.sku, OpenIAB_Android.STORE_NOKIA, item.skuNokia);
		}
		
		m_initStep = -1;
		GotError = false;
		HasInfo = false;	
	}
	
	void Update()
	{
		switch( m_initStep )
		{
		case 0:
			InitIAP();
			++m_initStep;
		break;
		
		case 2:
			QueryInventory();
			++m_initStep;
		break;		
		}		
	}
		
	// Update is called once per frame
	void LateUpdate () {
	
		// TODO - add more items to be done here
		Items[ITEM_CROSSBOW].purchased = PlayerStash.Instance.PurchasedCrossbow;
		Items[ITEM_CROSSBOW].isBuyable = Items[ITEM_CROSSBOW].purchased < 1;
		
		Items[ITEM_AXE].purchased = PlayerStash.Instance.PurchasedAxe;
		Items[ITEM_AXE].isBuyable = Items[ITEM_AXE].purchased < 1;
		
		Items[ITEM_SNOW].purchased = PlayerStash.Instance.PurchasedSnow;
		Items[ITEM_FIRE].purchased = PlayerStash.Instance.PurchasedFire;				
	}
	
	public void Retry()
	{
		if( m_initStep < 0 && !HasInfo )
		{
			m_initStep = (-m_initStep) - 1;
		}
		
		GotError = false;
	}
	
	public void Request(Item item)
	{
		OpenIAB.purchaseProduct(item.sku, GetPayload());		
	}
	
	private void Commit(Item item, bool reversed = false)
	{
		// TODO - this is only a simulation
		switch( item.id )
		{
		case ITEM_CROSSBOW:
			PlayerStash.Instance.PurchasedCrossbow = reversed ? 0 : item.quantity;
			break;
		case ITEM_AXE:
			PlayerStash.Instance.PurchasedAxe = reversed ? 0 : item.quantity;
			break;
		case ITEM_SNOW:
			PlayerStash.Instance.PurchasedSnow += item.quantity;
			break;
		case ITEM_FIRE:
			PlayerStash.Instance.PurchasedFire += item.quantity;
			break;
		}
		
		PlayerPrefs.Save();
	}
	
	private void QueryInventory()	
	{
		string[] skus = new string[Items.Length];
		for( int i=0; i<Items.Length; ++i )
		{
			skus[i] = Items[i].sku;
		}
		OpenIAB.queryInventory(skus);
	}
	
	private void InitIAP()
	{		
		// Application public key
		string public_key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApMbAFbkq2Fgpom0l/MXvSySmDqGLI8br4Puw3LXHuYuygRYN++5hWIBiRc704HVB7DZrLhxSNQ52EuYtGuDWBXpAP7ExIk9HzMT3BhoXI1WU8UtK+TQwqvTkNhJUKKgXRXf9hpAxLGZo7KaAsF3X6MwWYcZygCwbWEZzh9O9csSIM2ISosmODkJN1/T6lYU1yiYTO2SKSv9SWJYxt14E4+g4HsVE09z8zb95NODnD4MZsoz5qYS+7PnFvQby32W+z+pz5WIIQYhA/IDfkv44fljmKT9i/QAGubjW44UyArqX3iqhtOWZr3z2NVTwwnJ+kPZGcFA8uUt+MQuAp7SAUwIDAQAB";
		
		Options options = new Options();
		options.storeKeys = new Dictionary<string, string> {
			{OpenIAB_Android.STORE_GOOGLE, public_key}
		};
		options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
		
		// Transmit options and start the service
		OpenIAB.init(options);		
	}
	
	void OnApplicationQuit()
	{
		OpenIAB.unbindService();
	}	
	
	private void OnEnable() {
		// Listen to all events for illustration purposes
		OpenIABEventManager.billingSupportedEvent += billingSupportedEvent;
		OpenIABEventManager.billingNotSupportedEvent += billingNotSupportedEvent;
		OpenIABEventManager.queryInventorySucceededEvent += queryInventorySucceededEvent;
		OpenIABEventManager.queryInventoryFailedEvent += queryInventoryFailedEvent;
		OpenIABEventManager.purchaseSucceededEvent += purchaseSucceededEvent;
		OpenIABEventManager.purchaseFailedEvent += purchaseFailedEvent;
		OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceededEvent;
		OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailedEvent;		
	}
	
	private void OnDisable() {
		// Remove all event handlers
		OpenIABEventManager.billingSupportedEvent -= billingSupportedEvent;
		OpenIABEventManager.billingNotSupportedEvent -= billingNotSupportedEvent;
		OpenIABEventManager.queryInventorySucceededEvent -= queryInventorySucceededEvent;
		OpenIABEventManager.queryInventoryFailedEvent -= queryInventoryFailedEvent;
		OpenIABEventManager.purchaseSucceededEvent -= purchaseSucceededEvent;
		OpenIABEventManager.purchaseFailedEvent -= purchaseFailedEvent;
		OpenIABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceededEvent;
		OpenIABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailedEvent;
	}
	
	private void billingSupportedEvent() {
		Debug.Log("billingSupportedEvent");		
		++m_initStep;
	}
	
	private void billingNotSupportedEvent(string error) {
		Debug.Log("billingNotSupportedEvent: " + error);
		
		ErrorMessage = error;
		GotError = true;		
		m_initStep = -1;
	}
	
	private void queryInventorySucceededEvent(Inventory inventory) {
		Debug.Log("queryInventorySucceededEvent: " + inventory);
		
		foreach( Item item in Items )
		{
			Purchase purchase = inventory.GetPurchase(item.sku);
			bool verified = (purchase != null && VerifyDeveloperPayload(purchase.DeveloperPayload));
			
			if( !item.isConsumable )
			{				
				if( verified)
				{
					Commit(item);
				}			
				else
				{	
					Commit(item, true);
				}
			}
			else
			{
				if( verified )
				{
					OpenIAB.consumeProduct(purchase);
				}
			}
			
			SkuDetails details = inventory.GetSkuDetails(item.sku);
			if( details != null )
			{
				item.pricetag = details.Price;
				
				if( details.Title != null && details.Title.Length > 0 )
				{
					item.title = details.Title;
				}
				
				//if( details.Description != null && details.Description.Length > 0 )
				//{
					//item.description = details.Description;					
				//}
			}
			else
			{
				item.pricetag = "???";
			}
		}		
		
		HasInfo = true;
	}
	
	private void queryInventoryFailedEvent(string error) {
		Debug.Log("queryInventoryFailedEvent: " + error);
		
		ErrorMessage = error;
		GotError = true;
		m_initStep = -3;
	}
	
	private void purchaseSucceededEvent(Purchase purchase) {
		Debug.Log("purchaseSucceededEvent: " + purchase);
		
		if( !VerifyDeveloperPayload(GetPayload()) )
		{
			return;
		}
		
		foreach( Item item in Items )
		{
			if( purchase.Sku.Equals(item.sku) )
			{
				Commit(item);
				
				if( item.isConsumable )
				{
					OpenIAB.consumeProduct(purchase);
				}
				break;
			}
		}
	}
	
	private void purchaseFailedEvent(string error) {
		Debug.Log("purchaseFailedEvent: " + error);
		
		ErrorMessage = error;
		GotError = true;
		m_initStep = -5;
	}
	
	private void consumePurchaseSucceededEvent(Purchase purchase) {
		Debug.Log("consumePurchaseSucceededEvent: " + purchase);		
	}
	
	private void consumePurchaseFailedEvent(string error) 
	{
		Debug.Log("consumePurchaseFailedEvent: " + error);
	}
	
	// Verifies the developer payload of a purchase.
	bool VerifyDeveloperPayload(string developerPayload) {	
		if( !developerPayload.Equals(GetPayload()) )
		{
			return false;
		}
			
		return true;
	}
	
	private string GetPayload()
	{
		return "A very secret text!";
	}
}
