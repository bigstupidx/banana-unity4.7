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
		Items[ITEM_CROSSBOW] = new Item(ITEM_CROSSBOW, "crossbow", "The ultimate range weapon crossbow can shoot 3 arrows at a time!");
		Items[ITEM_AXE] = new Item(ITEM_AXE, "axe", "The axe wielder can do attack on both sides.");
		Items[ITEM_SNOW] = new Item(ITEM_SNOW, "blizzard", "Buy a combo of [00ff00]10[-] blizzard spells. The blizzard can freeze up all enemies for a long while.");
		Items[ITEM_FIRE] = new Item(ITEM_FIRE, "apocalypse", "Buy a combo of [00ff00]5[-] apocalypse spells. The spell can virtually hurt all enemies on sight.");
	}
	
	public class Item	
	{
		public int id;
		public string description;
		public string pricetag;
		public int purchased;	
		public bool isBuyable;	
		public string sku;
		
		public Item(int nid, string nsku, string desc)
		{
			id = nid;
			description = desc;
			pricetag = "";
			purchased = 0;
			isBuyable = true;
			sku = nsku;
		}
	}
	
	public Item[] Items;
	
	public bool GotError = false;
	public bool HasInfo = false;		
	private bool m_isRequesting = false;
	private bool m_isWorking = false;

	// Use this for initialization
	void Start () {
		m_isWorking = false;
		m_isRequesting = false;
		RequestInfo();
		
		// Map sku for different stores
		foreach( Item item in Items )
		{
			OpenIAB.mapSku(item.sku, OpenIAB_Android.STORE_GOOGLE, item.sku);
		}
	}
	
	public void RequestInfo()
	{
		if( m_isWorking )
		{
			return;
		}
		
		m_isRequesting = true;
	}
	
	private void Refresh()
	{
		m_isWorking = true;
		
		GotError = false;
		HasInfo = false;
		
		// TODO - this is only a simulation
		StartCoroutine(Connect());
	}
	
	public IEnumerator Connect()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(5.0f));
	
		// TODO - add more items to be done here	
		Items[0].pricetag = "VND50,000";
		Items[1].pricetag = "VND30,000";
		Items[2].pricetag = "VND10,000";
		Items[3].pricetag = "VND10,000";
		
		HasInfo = true;
		
		m_isWorking = false;
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
		
		if( m_isRequesting )
		{
			m_isRequesting = false;
			Refresh();
		}
	}
	
	public void Commit(Item item)
	{
		// TODO - this is only a simulation
		switch( item.id )
		{
		case ITEM_CROSSBOW:
			++PlayerStash.Instance.PurchasedCrossbow;
			break;
		case ITEM_AXE:
			++PlayerStash.Instance.PurchasedAxe;
			break;
		case ITEM_SNOW:
			++PlayerStash.Instance.PurchasedSnow;
			break;
		case ITEM_FIRE:
			++PlayerStash.Instance.PurchasedFire;
			break;
		}
		
		PlayerPrefs.Save();
	}
	
	void InitIAP()
	{
		// Application public key
		string public_key = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApMbAFbkq2Fgpom0l/MXvSySmDqGLI8br4Puw3LXHuYuygRYN++5hWIBiRc704HVB7DZrLhxSNQ52EuYtGuDWBXpAP7ExIk9HzMT3BhoXI1WU8UtK+TQwqvTkNhJUKKgXRXf9hpAxLGZo7KaAsF3X6MwWYcZygCwbWEZzh9O9csSIM2ISosmODkJN1/T6lYU1yiYTO2SKSv9SWJYxt14E4+g4HsVE09z8zb95NODnD4MZsoz5qYS+7PnFvQby32W+z+pz5WIIQYhA/IDfkv44fljmKT9i/QAGubjW44UyArqX3iqhtOWZr3z2NVTwwnJ+kPZGcFA8uUt+MQuAp7SAUwIDAQAB";
		
		Options options = new Options();
		options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
		options.storeKeys = new Dictionary<string, string> {
			{OpenIAB_Android.STORE_GOOGLE, public_key}
		};
		
		// Transmit options and start the service
		OpenIAB.init(options);
	}
	
	void OnApplicationQuit()
	{
	}
	
	void OnApplicationPause()
	{
	}
}
