using UnityEngine;
using System.Collections;

public class StateShop : GameState {

	public UIButton ButtonBuy;
	public UILabel ErrorLabel;
	public UILabel PleaseWaitLabel;
	public UILabel DescriptionLabel;
	
	public UIToggle[] ItemToggles;
	
	private string m_originalDescriptionText;
	private int m_currentSelectedItem;

	void Awake()
	{
		UIEventListener.Get (FindChild ("ButtonBack")).onClick += (obj) =>
		{
			StartCoroutine (OnBackButtonClick ());
		};
		
		UIEventListener.Get (ButtonBuy.gameObject).onClick += (obj) =>
		{
			StartCoroutine (OnBuyButtonClick ());
		};
		
		m_currentSelectedItem = -1;
	}
	
	private IEnumerator OnBackButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.25f));
		
		StateManager.Instance.PopState();
	}
	
	private IEnumerator OnBuyButtonClick()
	{
		yield return StartCoroutine(Utils.WaitForRealSeconds(0.1f));
		
		if( m_currentSelectedItem >= 0 && m_currentSelectedItem < ShopController.ITEM_NUM )
		{
			ShopController.Item item = ShopController.Instance.Items[m_currentSelectedItem];
			if( item.isBuyable )
			{			
				ShopController.Instance.Commit(item);
				m_currentSelectedItem = -1;
			}
		}
	}
	
	public override void OnEnter()
	{
		m_originalDescriptionText = DescriptionLabel.text;
		m_currentSelectedItem = -1;		
	}
	
	public override void OnUpdate()
	{
		if( ShopController.Instance.GotError )
		{
			// Error occured !
			PleaseWaitLabel.gameObject.SetActive(false);
			DescriptionLabel.gameObject.SetActive(false);
			ErrorLabel.gameObject.SetActive(true);
			ButtonBuy.gameObject.SetActive(false);
		}
		else if( ShopController.Instance.HasInfo )
		{
			// Everything is fine !
			DescriptionLabel.gameObject.SetActive(true);			
			PleaseWaitLabel.gameObject.SetActive(false);		
			ErrorLabel.gameObject.SetActive(false);
			
			int selectedItem = 0;
			foreach( UIToggle toggle in ItemToggles )
			{
				if( toggle.value )
				{
					break;
				}
				++selectedItem;
			}
			
			if( selectedItem != m_currentSelectedItem )
			{			
				m_currentSelectedItem = selectedItem;
				if( m_currentSelectedItem >= 0 && m_currentSelectedItem < ShopController.ITEM_NUM )
				{
					ShopController.Item item = ShopController.Instance.Items[m_currentSelectedItem];
					string desc = m_originalDescriptionText.Replace("{Description}", item.description);
					desc = desc.Replace("{Quantity}", item.purchased.ToString());
					desc = desc.Replace("{Price_tag}", item.pricetag);
					DescriptionLabel.text = desc;
					
					ButtonBuy.gameObject.SetActive(item.isBuyable);
				}
			}
		}
		else
		{
			// Waiting
			DescriptionLabel.gameObject.SetActive(false);
			ButtonBuy.gameObject.SetActive(false);
			PleaseWaitLabel.gameObject.SetActive(true);		
			ErrorLabel.gameObject.SetActive(false);
		}
	}
	
	public override void OnExit()
	{
		DescriptionLabel.text = m_originalDescriptionText;
	}
}
