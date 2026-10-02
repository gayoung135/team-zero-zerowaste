using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Receipt : MonoBehaviour
{
    int ItemPrice = 0; //가격 총합
    List<string> items = new List<string>(); //구매할 아이템 목록 저장

    public Transform Shop; //상점 아이템들 들어있는 객체
    public TextMeshProUGUI CoinText; //코인 텍스트
    public TextMeshProUGUI PriceText; //가격 텍스트
    public Transform content; //영수증 물품 텍스트 들어갈 위치
    public TextMeshProUGUI ReceiptText; //영수증 텍스트 프리팹
    public GameObject Message; //메시지 객체
    public TextMeshProUGUI MessageText; //메시지 텍스트
    private TextMeshProUGUI currentReceiptText; //생성한 텍스트 클론 저장용

    public ShopDatabase shopDatabase; //상점 재설정용
    public void AddItem(ShopItemData item)
    {
        items.Add(item.item_id);
        ItemPrice += item.price;
        PriceText.text = ItemPrice.ToString()+" C";

        currentReceiptText = Instantiate(ReceiptText, content);
        currentReceiptText.text = item.item_name+" "+item.price+"C";
        currentReceiptText.name = item.item_id;

        if(items.Count == 1) gameObject.SetActive(true);
    }

    public void RemoveItem(ShopItemData item)
    {
        items.Remove(item.item_id);
        ItemPrice -= item.price;
        PriceText.text = ItemPrice.ToString()+" C";

        Transform obj = content.Find(item.item_id);
        if (obj != null) Destroy(obj.gameObject);

        if(items.Count == 0) gameObject.SetActive(false);
    }

    public void Cancel() //취소버튼
    {
        items.Clear(); //리스트 초기화
        ItemPrice = 0; //총합 코인 초기화
        foreach (Transform child in content) Destroy(child.gameObject); //모든 텍스트 제거
        foreach (Transform child in Shop) //모든 선택상태 초기화
        {
            ShopItem shopItem = child.GetComponent<ShopItem>();
            if (shopItem != null) shopItem.isSelect = false;
        }
    }

    public void BuyItem() //구매버튼
    {
        if(UGSManager.Instance != null)
        {
            if(ItemPrice <= UGSManager.Instance.currentCoins) //돈이 충분
            {
                UGSManager.Instance.EarnCoins(-ItemPrice); //코인 정산

                foreach (string itemID in items) //데이터베이스에 반영
                {
                    ShopItemData data = GameDBManager.Instance.GetShopItemData(itemID);
                    if (data != null) data.is_sold_out = true;
                }
                shopDatabase.Setting(); //상점 재설정

                //구매 완료 창 띄우기
                MessageText.text = "Purchase complete!";
                Message.SetActive(true);
                StartCoroutine(HideMessage());

                Cancel(); //리스트 제거
            } 
            else //돈이 없음
            {
                MessageText.text = "Isn't enough coin.";
                Message.SetActive(true);
                StartCoroutine(HideMessage());
            }
        }
        else Debug.Log("비로그인 상태");
    }

    IEnumerator HideMessage() //메세지 숨기기용
    {
        yield return new WaitForSeconds(3f);
        Message.SetActive(false);
    }
}
