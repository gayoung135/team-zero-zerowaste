using System;
using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 1. 쓰레기 분류 목록 DB 구조체
// ==========================================
[System.Serializable]
public class TrashGuideData
{
    public string id;
    public int correct_bin_id;
    public string name;
    public string name_KR;
    public Sprite item_image;
    public string category;
    public bool is_unlock;
    [TextArea(2, 4)] public string wrong;
    [TextArea(2, 4)] public string wrong_KR;
    [TextArea(2, 4)] public string correct;
    [TextArea(2, 4)] public string correct_KR;
    [TextArea(2, 4)] public string method;
    [TextArea(2, 4)] public string method_KR;
    [TextArea(2, 4)] public string tip;
    [TextArea(2, 4)] public string tip_KR;
}

// ==========================================
// 2. 상품 목록 DB 구조체
// ==========================================
[System.Serializable]
public class ShopItemData
{
    public string item_id; 
    public string item_name; 
    public string item_name_KR;
    public int price; 
    public Sprite item_image; 
    public bool is_sold_out;
    public AnimationClip animation;
}

public class GameDBManager : MonoBehaviour
{
    public static GameDBManager Instance { get; private set; }

    [Header("[마스터 DB 소스]")]
    public List<TrashGuideData> trashGuideMasterDB = new List<TrashGuideData>();
    public List<ShopItemData> shopItemMasterDB = new List<ShopItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TrashGuideData GetTrashData(string Id)
    {
        TrashGuideData Tdata = trashGuideMasterDB.Find(i => i.id == Id);
        return Tdata;
    }

    public ShopItemData GetShopItemData(string itemId)
    {
        ShopItemData Sdata = shopItemMasterDB.Find(i => i.item_id == itemId);
        return Sdata;
    }

    // 품절 아이템 상태 동기화 처리
    public void SyncInventoryFromServer(List<string> itemIdsFromServer)
    {
        // 모든 상품의 품절 상태 false로 초기화
        foreach (var item in shopItemMasterDB)
        {
            item.is_sold_out = false;
        }

        if (itemIdsFromServer == null) return;
        Debug.Log("서버로부터 상점 품절 아이템 목록 연동 완료");

        // 서버에서 받아온 아이템 ID 배열 순회하며 DB값 true로 갱신
        foreach (string id in itemIdsFromServer)
        {
            ShopItemData item = GetShopItemData(id);
            if (item != null)
            {
                item.is_sold_out = true;
            }
        }
    }

    // 아이템 구매 시 품절 상태 업데이트
    public void AddItemToInventory(string itemId)
    {
        ShopItemData item = GetShopItemData(itemId);
        if (item != null)
        {
            item.is_sold_out = true;
        }
    }
}
