using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

public class Receipt : MonoBehaviour
{
    int ItemPrice = 0;
    List<string> items = new List<string>();

    public Transform Shop;
    public TextMeshProUGUI CoinText;
    public TextMeshProUGUI PriceText;
    public Transform content;
    public TextMeshProUGUI ReceiptText;
    public GameObject Message;
    public TextMeshProUGUI MessageText;
    private TextMeshProUGUI currentReceiptText;

    public ShopDatabase shopDatabase;

    public void AddItem(ShopItemData item)
    {
        items.Add(item.item_id);
        ItemPrice += item.price;
        PriceText.text = ItemPrice.ToString() + " C";

        currentReceiptText = Instantiate(ReceiptText, content);
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko") currentReceiptText.text = item.item_name_KR + " " + item.price + "C";
        else currentReceiptText.text = item.item_name + " " + item.price + "C";
        currentReceiptText.name = item.item_id;

        if (items.Count == 1) gameObject.SetActive(true);
    }

    public void RemoveItem(ShopItemData item)
    {
        items.Remove(item.item_id);
        ItemPrice -= item.price;
        PriceText.text = ItemPrice.ToString() + " C";

        Transform obj = content.Find(item.item_id);
        if (obj != null) Destroy(obj.gameObject);

        if (items.Count == 0) gameObject.SetActive(false);
    }

    public void Cancel()
    {
        items.Clear();
        ItemPrice = 0;
        foreach (Transform child in content) Destroy(child.gameObject);
        foreach (Transform child in Shop)
        {
            ShopItem shopItem = child.GetComponent<ShopItem>();
            if (shopItem != null) shopItem.isSelect = false;
        }
    }

    public void BuyItem()
    {
        if (UGSManager.Instance != null)
        {
            if (ItemPrice <= UGSManager.Instance.currentCoins)
            {
                // 1. 코인 차감 정산 (자동 내부 서버 세이브 발생)
                UGSManager.Instance.EarnCoins(-ItemPrice);

                // 2. 장바구니에 담긴 아이템 로컬 DB 반영
                foreach (string itemID in items)
                {
                    ShopItemData data = GameDBManager.Instance.GetShopItemData(itemID);
                    if (data != null) data.is_sold_out = true;
                }

                // 3. 현재 DB 상에서 품절 상태인 아이템 ID 목록 취합하여 서버 전송
                if (GameDBManager.Instance != null)
                {
                    List<string> soldOutList = new List<string>();
                    foreach (var shopItem in GameDBManager.Instance.shopItemMasterDB)
                    {
                        if (shopItem.is_sold_out)
                        {
                            soldOutList.Add(shopItem.item_id);
                        }
                    }

                    UGSManager.Instance.SaveSoldOutItems(soldOutList);
                }

                // 4. 상점 UI 재설정
                shopDatabase.Setting();
                if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko") shopDatabase.Message("구매가 완료되었습니다!");
                else shopDatabase.Message("Purchase complete!");
                Cancel();
                gameObject.SetActive(false);
            }
            else {
                if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko") shopDatabase.Message("돈이 부족합니다.");
                else shopDatabase.Message("Isn't enough coin.");
            }
        }
        else Debug.Log("비로그인 상태");
    }
}
