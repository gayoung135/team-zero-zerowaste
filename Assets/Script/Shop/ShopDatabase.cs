using UnityEngine;
using TMPro;

public class ShopDatabase : MonoBehaviour
{
    public TextMeshProUGUI CoinText; //코인 텍스트
    void Start()
    {
        CoinText.text = UGSManager.Instance.currentCoins.ToString();
    }
}
