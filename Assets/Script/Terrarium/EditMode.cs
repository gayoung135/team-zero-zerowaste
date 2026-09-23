using UnityEngine;
using UnityEngine.UI;

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false;

    [Header("--- UI Group Containers ---")]
    public GameObject editUI;            // Terrarium_UI -> Edit_UI
    public GameObject basicUI;           // Terrarium_UI -> Basic_UI
    
    [Header("--- Preview UI ---")]
    public GameObject previewEditButton; // Terrarium_UI -> PreviewEditButton (우측 상단 독립 버튼)

    [Header("--- Item Spawn Settings ---")]
    public GameObject itemPrefab;   
    public GameObject itemParent;   

    [Header("--- Edit UI Elements ---")]
    public GameObject itemBarPanel; // Edit_UI -> Panel
    public GameObject emptyText;    // Empty... 텍스트

    void Start()
    {
        ShowBasicUI();
    }

    // 1. 메인 화면
    public void ShowBasicUI()
    {
        isEditMode = false;
        
        if (basicUI != null) basicUI.SetActive(true);
        if (editUI != null) editUI.SetActive(false);
        if (previewEditButton != null) previewEditButton.SetActive(false);
    }

    // 2. 편집 모드 진입 ([EDIT] 버튼 누를 때)
    public void EditModeOn()
    {
        isEditMode = true;

        if (basicUI != null) basicUI.SetActive(false);
        if (editUI != null) editUI.SetActive(true);
        if (previewEditButton != null) previewEditButton.SetActive(false);

        UpdateEmptyMessage();
    }

    // 3. [PREVIEW] 버튼 클릭 시 (관상 모드)
    public void OnClickPreview()
    {
        TerrariumItem currentItem = FindObjectOfType<TerrariumItem>();
        if (currentItem != null) currentItem.check();

        isEditMode = false;

        if (basicUI != null) basicUI.SetActive(false);
        if (editUI != null) editUI.SetActive(false);
        if (previewEditButton != null) previewEditButton.SetActive(true); 
    }

    // [SAVE] 저장 버튼
    public void save()
    {
        Debug.Log("테라리움 저장 완료");

        TerrariumItem[] items = itemParent.GetComponentsInChildren<TerrariumItem>();
        foreach (TerrariumItem item in items)
        {
            item.check();
        }

        ShowBasicUI();
    }

    // [CANCEL] 취소 버튼 (저장 안 하고 롤백)
    public void cancel()
    {
        Debug.Log("저장 취소: 임시 아이템 파괴 및 슬롯 복구");

        TerrariumItem[] currentItems = itemParent.GetComponentsInChildren<TerrariumItem>();
        foreach (TerrariumItem item in currentItems)
        {
            if (item.originSlot != null)
            {
                item.originSlot.SetActive(true);
            }
            Destroy(item.gameObject);
        }

        ShowBasicUI();
    }

    // 하단바 아이템 클릭 시 소환
    public TerrariumItem itemClick(Sprite itemSprite, string itemName)
    {
        if (itemPrefab == null || itemParent == null) return null;

        GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
        RectTransform rect = newItem.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = Vector2.zero;

        newItem.name = itemName;
        Image newImage = newItem.GetComponent<Image>();
        if (newImage != null) newImage.sprite = itemSprite;

        TerrariumItem item = newItem.GetComponent<TerrariumItem>();
        if (item != null)
        {
            item.EditItem();
        }

        return item;
    }

    // Empty 문구 스마트 갱신
    public void UpdateEmptyMessage()
    {
        if (emptyText != null && itemBarPanel != null)
        {
            Canvas.ForceUpdateCanvases();
            ItemListButton[] activeSlots = itemBarPanel.GetComponentsInChildren<ItemListButton>(false);
            bool isEmpty = (activeSlots.Length == 0);
            emptyText.SetActive(isEmpty);
        }
    }
}