using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Receipt : MonoBehaviour
{
    int ItemPrice = 0; //가격 총합
    List<string> items = new List<string>(); //구매할 아이템 목록 저장
    public Transform Shop; //상점 아이템들 들어있는 객체

    public TextMeshProUGUI PriceText; //가격 텍스트
    public Transform content; //영수증 물품 텍스트 들어갈 위치
    public TextMeshProUGUI ReceiptText; //영수증 텍스트 프리팹
    private TextMeshProUGUI currentReceiptText; //생성한 텍스트 클론 저장용

    public void AddItem(string name, int price)
    {
        items.Add(name);
        ItemPrice += price;
        PriceText.text = ItemPrice.ToString()+" C";

        currentReceiptText = Instantiate(ReceiptText, content);
        currentReceiptText.text = name+" "+price+"C";
        currentReceiptText.name = name;

        if(items.Count == 1) gameObject.SetActive(true);
    }

    public void RemoveItem(string name, int price)
    {
        items.Remove(name);
        ItemPrice -= price;
        PriceText.text = ItemPrice.ToString()+" C";

        Transform obj = content.Find(name);
        Destroy(obj.gameObject);

        if(items.Count == 0) gameObject.SetActive(false);
    }

    public void Cancel() //취소버튼
    {
        items.Clear(); //리스트 초기화
        ItemPrice = 0; //총합 코인 초기화
        foreach (Transform child in content) //모든 텍스트 제거
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in Shop) //모든 선택상태 초기화
        {
            child.GetComponent<ShopItem>().isSelect = false;
        }
    }
}
