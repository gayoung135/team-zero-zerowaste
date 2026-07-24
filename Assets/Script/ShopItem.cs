using UnityEngine;
using TMPro;

public class ShopItem : MonoBehaviour
{
    //상점 아이템마다 해당 스크립트가 들어가야 합니다

    public int ItemPrice; //아이템별 가격 입력부분
    public TextMeshProUGUI ItemPriceText; //말풍선 텍스트

    public void ChangeText() //아이템에 마우스 올리면 가격 출력하는 함수
    {
        string Price = ItemPrice.ToString();
        ItemPriceText.text = "This item is "+Price+" coin";
    }
}
