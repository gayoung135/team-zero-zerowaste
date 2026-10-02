using UnityEngine;
using TMPro;

public class ShopDatabase : MonoBehaviour
{
    public TextMeshProUGUI CoinText; //코인 텍스트
    public Transform Shop; //상점 아이템들 들어있는 객체

    void Start()
    {
        Setting();
    }

    public void Setting()
    {
        CoinText.text = UGSManager.Instance.currentCoins.ToString(); //텍스트 연동
        foreach (Transform child in Shop) //soldout 반영
        {
            ShopItem shopItem = child.GetComponent<ShopItem>();
            if (shopItem == null) continue;

            ShopItemData data = GameDBManager.Instance.GetShopItemData(shopItem.ItemID); // ID로 DB 검색

            if (data == null) continue;
            if (data.is_sold_out)
            {
                shopItem.soldOut.SetActive(true);
                shopItem.itemImage.color = new Color(1, 1, 1, 0.5f);
                shopItem.buyButton.interactable = false;
            }
            else
            {
                shopItem.soldOut.SetActive(false);
                shopItem.itemImage.color = new Color(1, 1, 1, 1f);
                shopItem.buyButton.interactable = true;
            }
        }
    }
}
