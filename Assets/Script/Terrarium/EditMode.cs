using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class PlacedItemData
{
    public string itemId;
    public float posX;
    public float posY;
    public float rotZ;
}

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false;
    public GameObject itemPrefab;
    public GameObject itemParent;
    public GameObject itemButtonPrefab;
    public Transform itemButtonParent;
    public TextMeshProUGUI NothingText;
    private RectTransform rectTransform;
    bool hasItem = false;

    private bool isClicking = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        RefreshInventoryUI();

        if (UGSManager.Instance != null && UGSManager.Instance.currentLayout != null && UGSManager.Instance.currentLayout.Count > 0)
        {
            Debug.Log("[EditMode] 테라리움 화면 활성화 확인. 캐싱된 서버 배치 레이아웃 스폰 개시");
            SpawnLayout(UGSManager.Instance.currentLayout);
        }
    }

    // 인벤토리 UI 새로고침하는 로직 (로그인 완료 시에도 호출됨)
    public void RefreshInventoryUI()
    {
        if (itemButtonParent == null) return;

        foreach (Transform child in itemButtonParent)
        {
            if (child != null) Destroy(child.gameObject);
        }

        if (GameDBManager.Instance == null || GameDBManager.Instance.shopItemMasterDB == null)
        {
            Debug.LogWarning("[EditMode] GameDBManager 마스터 DB를 찾을 수 없습니다.");
            return;
        }

        hasItem = false;
        foreach (ShopItemData item in GameDBManager.Instance.shopItemMasterDB) 
        {
            if (item == null) continue;

            if (item.is_sold_out)
            {
                hasItem = true; 
                
                if (itemButtonPrefab == null) continue;
                GameObject Item = Instantiate(itemButtonPrefab, itemButtonParent);
                if (Item == null) continue;

                Image image = Item.GetComponentInChildren<Image>();
                if (image != null && item.item_image != null) 
                {
                    image.sprite = item.item_image;
                }
                else
                {
                    Image selfImage = Item.GetComponent<Image>();
                    if (selfImage != null && item.item_image != null)
                    {
                        selfImage.sprite = item.item_image;
                    }
                }

                Button btn = Item.GetComponent<Button>();
                if (btn == null) btn = Item.GetComponentInChildren<Button>();

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => itemClick(item.item_image));
                }
            }
        }

        if (NothingText != null)
        {
            NothingText.gameObject.SetActive(!hasItem);
        }
    }

    public void itemClick(Sprite itemSprite) 
    {
        if (!isEditMode) return;
        if (itemPrefab == null || itemParent == null) return;

        if (isClicking) return;
        isClicking = true;

        GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
        if (newItem == null)
        {
            isClicking = false;
            return;
        }

        RectTransform rect = newItem.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = new Vector2(0, 0);

        Image newImage = newItem.GetComponent<Image>();
        if (newImage != null) newImage.sprite = itemSprite;

        TerrariumItem item = newItem.GetComponent<TerrariumItem>();
        if (item != null)
        {
            if (GameDBManager.Instance != null && GameDBManager.Instance.shopItemMasterDB != null)
            {
                ShopItemData matchedData = GameDBManager.Instance.shopItemMasterDB.Find(x => x.item_image == itemSprite);
                if (matchedData != null)
                {
                    item.itemId = matchedData.item_id;
                }
            }
            
            item.EditItem();
        }

        Invoke("ResetClickFlag", 0.05f);
    }

    private void ResetClickFlag()
    {
        isClicking = false;
    }

    public void EditModeOn() 
    {
        isEditMode = true;
    }
    
    public void save() // 저장버튼
    {
        Debug.Log("테라리움 배치 데이터 서버 세이브 시작");
        List<PlacedItemData> layoutData = new List<PlacedItemData>();

        if (itemParent == null) return;

        foreach (Transform child in itemParent.transform)
        {
            if (child == null) continue;

            TerrariumItem itemScript = child.GetComponent<TerrariumItem>();
            RectTransform rect = child.GetComponent<RectTransform>();

            if (itemScript != null && rect != null)
            {
                PlacedItemData data = new PlacedItemData
                {
                    itemId = itemScript.itemId,
                    posX = rect.anchoredPosition.x,
                    posY = rect.anchoredPosition.y,
                    rotZ = itemScript.zRotation
                };
                layoutData.Add(data);
            }
        }

        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.SaveTerrariumLayout(layoutData);
        }

        isEditMode = false;
    }

    public void cancel() // 취소버튼
    {
        Debug.Log("배치 취소: 클라우드 백업 데이터 복구");
        
        if (itemParent == null) return;

        foreach (Transform child in itemParent.transform)
        {
            if (child != null) Destroy(child.gameObject);
        }

        if (UGSManager.Instance != null)
        {
            SpawnLayout(UGSManager.Instance.currentLayout);
        }

        isEditMode = false;
    }

    public void SpawnLayout(List<PlacedItemData> layout)
    {
        if (layout == null || itemPrefab == null || itemParent == null) return;

        foreach (Transform child in itemParent.transform)
        {
            if (child != null) Destroy(child.gameObject);
        }

        foreach (var data in layout)
        {
            if (GameDBManager.Instance == null) continue;
            ShopItemData shopItem = GameDBManager.Instance.GetShopItemData(data.itemId);
            if (shopItem == null) continue;

            GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
            if (newItem == null) continue;

            RectTransform rect = newItem.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(data.posX, data.posY);
            }

            Image newImage = newItem.GetComponent<Image>();
            if (newImage != null)
            {
                newImage.sprite = shopItem.item_image;
            }

            TerrariumItem itemScript = newItem.GetComponent<TerrariumItem>();
            if (itemScript != null)
            {
                itemScript.itemId = data.itemId;
                itemScript.SetRotation(data.rotZ);
            }
        }
    }
}
