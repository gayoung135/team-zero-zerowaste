using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    //상점 아이템마다 해당 스크립트가 들어가야 합니다
    public string ItemID; //아이템 아이디 입력부분
    public bool isSelect = false; //이미 선택되었는지 확인용

    public GameObject soldOut; //soldout 아이콘
    public Image itemImage; //아이템 이미지
    public Button buyButton; //아이템 버튼 상호작용
    GameObject SpeechBubble; //말풍선
    TextMeshProUGUI ItemPriceText; //말풍선 텍스트
    Receipt receipt; //영수증 코드

    ShopItemData item;
    void Start()
    {
        item = GameDBManager.Instance.GetShopItemData(ItemID); //아이템 정보 불러오기
        SpeechBubble = System.Array.Find(Resources.FindObjectsOfTypeAll<GameObject>(), x => x.name == "SpeechBubble"); //말풍선 찾기
        ItemPriceText = System.Array.Find(Resources.FindObjectsOfTypeAll<TextMeshProUGUI>(), x => x.name == "SpeechBubbleText"); //말풍선 텍스트 찾기
        receipt = System.Array.Find(Resources.FindObjectsOfTypeAll<Receipt>(), x => x.name == "Receipt"); //영수증 찾기
    }

    public void OnItem() //아이템에 마우스 올리면 가격 출력하는 함수
    {
        if(item.is_sold_out == false) ItemPriceText.text = "This item is "+item.price.ToString()+" coin";
        else ItemPriceText.text = "This item is sold out.";
        SpeechBubble.SetActive(true);
    }

    public void LeftItem() //마우스 탈출
    {
        SpeechBubble.SetActive(false);
    }

    public void ClickItem()
    {
        if(!isSelect)
        {
            isSelect = true;
            receipt.AddItem(item);
        }
        else
        {
            isSelect = false;
            receipt.RemoveItem(item);
        }
    }
}
