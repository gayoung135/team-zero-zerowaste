using System;
using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 1. 쓰레기 분류 목록 DB 구조체
// ==========================================
[System.Serializable]
public class TrashGuideData
{
    [Tooltip("쓰레기 고유 고유식별 Key")]
    public string id;                  // 예) apple_core, pet_bottle
    
    [Tooltip("올바른 쓰레기통 ID (1: 초록, 2: 파란 등)")]
    public int correct_bin_id;         // 예) 1, 2
    
    [Tooltip("쓰레기 이름")]
    public string trash_name;          // 예) 사과, 페트병
    
    [Tooltip("대분류")]
    public string category;            // 예) Food waste, Plastic
    
    [Tooltip("소분류/상세")]
    public string subcategory;         // 예) 과일류, 페트류
    
    [Tooltip("오답인 경우 출력할 메시지")]
    [TextArea(2, 4)]
    public string log_wrong;           // 예) 사과는 재활용 쓰레기가 아닙니다. 음식물 쓰레기입니다.
    
    [Tooltip("정답인 경우 출력할 메시지")]
    [TextArea(2, 4)]
    public string log_correct;         // 예) 사과는 음식물 쓰레기가 맞습니다.
    
    [Tooltip("인게임 올바른 분류 팁")]
    [TextArea(2, 4)]
    public string tip_ingame;          // 예) 수분을 최대한 제거하고 버려주세요.
}

// ==========================================
// 2. 상품 목록 DB 구조체
// ==========================================
[System.Serializable]
public class ShopItemData
{
    [Tooltip("아이템 고유 ID")]
    public string item_id;             // 예) ITEM_1
    
    [Tooltip("아이템 영문명")]
    public string item_name;           // 예) Bottle
    
    [Tooltip("아이템 가격 (코인)")]
    public int price;                  // 예) 70
    
    [Tooltip("상점 말풍선 안내 문구")]
    public string tooltip_text;        // 예) This item is 70 coin
    
    [Tooltip("품절 여부 (단일 해금 아이템일 경우 사용)")]
    public bool is_sold_out;           // false / true
}

public class GameDBManager : MonoBehaviour
{
    public static GameDBManager Instance { get; private set; }

    [Header("[마스터 DB 소스]")]
    [Tooltip("유니티 인스펙터에서 전체 쓰레기 리스트를 작성합니다.")]
    public List<TrashGuideData> trashGuideMasterDB = new List<TrashGuideData>();
    
    [Tooltip("유니티 인스펙터에서 전체 상점 물품 리스트를 작성합니다.")]
    public List<ShopItemData> shopItemMasterDB = new List<ShopItemData>();

    [Header("[로그인 유저 실시간 인벤토리 DB]")]
    // Key: item_id, Value: 보유 개수 (서버 저장용 리스트와 연동 예정)
    private Dictionary<string, int> userOwnedInventory = new Dictionary<string, int>();

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

    // ==========================================
    // 💡 팀원들이 데이터를 꺼내 쓰는 활용 API 함수들
    // ==========================================

    /// <summary>
    /// 쓰레기 Key(id)를 넘겨주면 매칭되는 모든 가이드 정보를 반환합니다. (미니게임용)
    /// </summary>
    public TrashGuideData GetTrashData(string trashId)
    {
        TrashGuideData data = trashGuideMasterDB.Find(t => t.id == trashId);
        if (data == null) Debug.LogWarning($"[GameDB] 해당 쓰레기 ID가 DB에 없습니다: {trashId}");
        return data;
    }

    /// <summary>
    /// 아이템 ID(item_id)를 넘겨주면 상점 상품 정보를 반환합니다. (상점용)
    /// </summary>
    public ShopItemData GetShopItemData(string itemId)
    {
        ShopItemData data = shopItemMasterDB.Find(i => i.item_id == itemId);
        if (data == null) Debug.LogWarning($"[GameDB] 해당 상품 ID가 DB에 없습니다: {itemId}");
        return data;
    }

    /// <summary>
    /// 서버(UGS)에서 계정 정보를 로드해왔을 때 이 로컬 DB에 인벤토리를 주입/동기화합니다.
    /// </summary>
    public void SyncInventoryFromServer(List<string> itemIdsFromServer)
    {
        userOwnedInventory.Clear();
        if (itemIdsFromServer == null) return;

        foreach (string id in itemIdsFromServer)
        {
            if (userOwnedInventory.ContainsKey(id))
                userOwnedInventory[id]++;
            else
                userOwnedInventory.Add(id, 1);
        }
    }

    /// <summary>
    /// 아이템을 획득/구매했을 때 인벤토리 딕셔너리에 개수를 추가합니다.
    /// </summary>
    public void AddItemToInventory(string itemId)
    {
        if (userOwnedInventory.ContainsKey(itemId)) userOwnedInventory[itemId]++;
        else userOwnedInventory.Add(itemId, 1);
    }
}
