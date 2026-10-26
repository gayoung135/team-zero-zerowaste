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
    public float scale;
}

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false; //에딧모드인지 확인용 (에딧 아니면 취소)
    public GameObject selectedChild = null; //다른 아이템 변경중인지 확인용
    public GameObject itemPrefab; //복제될 아이템 프리팹
    public GameObject itemParent; //복제된 아이템 배치할 위치
    public GameObject itemButtonPrefab; //아이템 버튼 프리팹
    public Transform itemButtonParent; //인벤토리
    public TextMeshProUGUI NothingText; //인벤토리 비었을때 띄울 텍스트
    public Transform tempbox; //아이템 배치위치
    public Transform ground; //땅 
    private RectTransform rectTransform;
    bool hasItem = false;

    void Awake()
    {
        Time.timeScale = 1f;
        rectTransform = GetComponent<RectTransform>();
        foreach (ShopItemData item in GameDBManager.Instance.shopItemMasterDB) //아이템 불러오기
        {
            if (item.is_sold_out)
            {
                hasItem = true; //아이템 보유중인지 판별
                GameObject Item = Instantiate(itemButtonPrefab, itemButtonParent);
                ItemListButton ItemScript = Item.GetComponent<ItemListButton>();
                if (ItemScript != null)
                {
                    Image image = Item.GetComponentInChildren<Image>(); //이미지 배분
                    if (image != null) image.sprite = item.item_image;
                    ItemScript.itemId = item.item_id; //아이디 배분
                }
            }
        }

        if (UGSManager.Instance != null && UGSManager.Instance.currentLayout != null && UGSManager.Instance.currentLayout.Count > 0)
        {
            Debug.Log("[EditMode] 테라리움 화면 활성화 확인. 캐싱된 서버 배치 레이아웃 스폰 개시");
            SpawnLayout(UGSManager.Instance.currentLayout);
        }

        NothingText.gameObject.SetActive(!hasItem);
    }

    public void itemClick(Sprite itemSprite, string ID) //목록 아이템 클릭 시 복제 및 설정모드로
    {
        GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
        RectTransform rect = newItem.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 0);

        //이미지 복사
        Image newImage = newItem.GetComponent<Image>();
        newImage.sprite = itemSprite;

        TerrariumItem item = newItem.GetComponent<TerrariumItem>();
        item.itemId = ID;
        item.ApplyAnimation();
        item.EditItem();
    }

    public void EditModeOn() //에딧버튼
    {
        isEditMode = true;
        tempbox.localScale = Vector3.one;
        ground.localScale = Vector3.one;
    }
    
    public void save() // 저장버튼
    {
        Debug.Log("테라리움 배치 데이터 서버 세이브 시작");
        List<PlacedItemData> layoutData = new List<PlacedItemData>();
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
                    rotZ = rect.localEulerAngles.z,
                    scale = rect.localScale.x
                };
                layoutData.Add(data);
            }
        }

        if (UGSManager.Instance != null) UGSManager.Instance.SaveTerrariumLayout(layoutData);
        isEditMode = false;
    }

    public void cancel() // 취소버튼
    {
        Debug.Log("배치 취소: 클라우드 백업 데이터 복구");
        foreach (Transform child in itemParent.transform) if (child != null) Destroy(child.gameObject);

        if (UGSManager.Instance != null) SpawnLayout(UGSManager.Instance.currentLayout);
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
                rect.localEulerAngles = new Vector3(0f,0f,data.rotZ);
                rect.GetChild(0).localEulerAngles = new Vector3(0f, 0f, -data.rotZ);
                rect.localScale = new Vector3(data.scale, data.scale, 1f);
            }

            Image newImage = newItem.GetComponent<Image>();
            if (newImage != null) newImage.sprite = shopItem.item_image;

            TerrariumItem itemScript = newItem.GetComponent<TerrariumItem>();
            if (itemScript != null) itemScript.itemId = data.itemId;
        }
    }

    void Update() //스크롤로 확대/축소
    {
        if(isEditMode) return;
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;

        float scale = tempbox.transform.localScale.x + scroll * 0.1f;

        if (scale < 0.5f) scale = 0.5f;
        if (scale > 2f) scale = 2f;
        tempbox.localScale = new Vector3(scale, scale, 1f);
        ground.localScale = new Vector3(scale, scale, 1f);
    }
}