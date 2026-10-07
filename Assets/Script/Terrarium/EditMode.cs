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
    [Header("UI 그룹")]
    public GameObject editUI;              // -> [Edit_UI] 부모 오브젝트
    public GameObject basicUI;             // -> [Basic_UI] 부모 오브젝트

    [Header("프리팹 (Project 창에서 드래그)")]
    public GameObject itemPrefab;          // TerrariumItem 프리팹
    public GameObject itemButtonPrefab;    // 패널 버튼 프리팹

    [Header("하이어라키 필드 & 인벤토리 연결")]
    public GameObject itemParent;          // -> [TempBox]
    public Transform itemButtonParent;     // -> [Edit_UI > Panel] 내부 Content
    public TextMeshProUGUI NothingText;    // -> [Edit_UI > Panel] 내부 안내 텍스트

    [Header("Edit_UI 내부 오브젝트")]
    public GameObject editContentPanel;    // -> [Edit_UI > Panel]
    public GameObject saveButton;          // -> [Edit_UI > SaveButton]
    public GameObject cancelButton;        // -> [Edit_UI > CancelButton]
    public GameObject previewButton;       // -> [Edit_UI > PreviewButton]

    [Header("전체화면 Preview 오브젝트 (하이어라키의 Preview)")]
    public GameObject previewScreenObject; // -> 하이어라키의 [Preview] 오브젝트 연결 (있으면 연결, 없어도 자동 감지)

    [HideInInspector]
    public bool isEditMode = false;
    private bool isClicking = false;
    private bool isPreviewMode = false;     // 현재 미리보기 감상 중인지 여부
    private float previewEnterTime = 0f;    // 버튼 누르자마자 바로 풀리는 현상 방지 쿨타임

    // 편집 시작 시점의 위치와 회전을 보관하는 스냅샷 리스트
    private List<PlacedItemData> editSessionSnapshot = new List<PlacedItemData>();

    void Awake()
    {
        // 서버에 저장된 이전 배치 데이터 복원
        if (UGSManager.Instance != null && UGSManager.Instance.currentLayout != null && UGSManager.Instance.currentLayout.Count > 0)
        {
            SpawnLayout(UGSManager.Instance.currentLayout);
        }

        CaptureCurrentSnapshot();

        // PreviewButton 클릭 리스너 연결
        if (previewButton != null)
        {
            Button pBtn = previewButton.GetComponent<Button>();
            if (pBtn == null) pBtn = previewButton.GetComponentInChildren<Button>();
            if (pBtn != null)
            {
                pBtn.onClick.RemoveAllListeners();
                pBtn.onClick.AddListener(EnterPreviewMode);
            }
        }

        // Preview 오브젝트에 Button이 붙어있을 경우 자동 리스너 연결
        if (previewScreenObject != null)
        {
            Button sBtn = previewScreenObject.GetComponent<Button>();
            if (sBtn != null)
            {
                sBtn.onClick.RemoveAllListeners();
                sBtn.onClick.AddListener(ExitPreviewMode);
            }
        }
    }

    void Update()
    {
        // 미리보기 감상 모드일 때 화면 아무 데나 터치/클릭하면 복귀!
        if (isPreviewMode)
        {
            // 누르자마자 풀리지 않도록 0.2초 후부터 터치 감지
            if (Time.time - previewEnterTime > 0.2f)
            {
                // 마우스 클릭 또는 모바일 터치 감지
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
                {
                    ExitPreviewMode();
                }
            }
        }
    }

    // ==========================================
    // [1] 미리보기 모드 진입 & 화면 터치 복귀
    // ==========================================

    // [Preview 버튼 클릭 시] -> 모든 UI 숨기고 전체화면 감상 모드 진입
    public void EnterPreviewMode()
    {
        Debug.Log("[EditMode] 미리보기 모드 시작 (화면 아무 곳이나 누르면 복귀합니다)");
        isPreviewMode = true;
        previewEnterTime = Time.time;

        // 1. Edit_UI 전체 숨김 (패널, Save, Cancel, Preview 버튼 싹 사라짐)
        if (editUI != null) editUI.SetActive(false);

        // 2. 전체화면 Preview 오브젝트가 있다면 활성화
        if (previewScreenObject != null) previewScreenObject.SetActive(true);

        // 3. 필드 아이템 조작창 닫기
        CloseAllItemEditUI();
    }

    // [화면 아무 데나 터치 시] -> 다시 편집 화면으로 복귀!
    public void ExitPreviewMode()
    {
        if (!isPreviewMode) return;
        Debug.Log("[EditMode] 미리보기 종료 -> 편집 모드로 복귀");

        isPreviewMode = false;

        // 1. Edit_UI 전체 다시 표시 (패널, 아이템, Save, Cancel, Preview 버튼 복구)
        if (editUI != null) editUI.SetActive(true);

        // 2. 전체화면 Preview 오브젝트 비활성화
        if (previewScreenObject != null) previewScreenObject.SetActive(false);

        // 3. 인벤토리 목록 갱신
        RefreshInventoryUI();
    }

    // ==========================================
    // [2] 메인화면 -> 편집 모드 진입
    // ==========================================
    public void OpenEditModeFromMain()
    {
        isEditMode = true;
        isPreviewMode = false;

        CaptureCurrentSnapshot();

        if (basicUI != null) basicUI.SetActive(false);
        if (editUI != null) editUI.SetActive(true);

        if (editContentPanel != null) editContentPanel.SetActive(true);
        if (saveButton != null) saveButton.SetActive(true);
        if (cancelButton != null) cancelButton.SetActive(true);
        if (previewButton != null) previewButton.SetActive(true);
        if (previewScreenObject != null) previewScreenObject.SetActive(false);

        RefreshInventoryUI();
    }

    public void CaptureCurrentSnapshot()
    {
        editSessionSnapshot.Clear();
        if (itemParent == null) return;

        foreach (Transform child in itemParent.transform)
        {
            if (child == null) continue;
            TerrariumItem item = child.GetComponent<TerrariumItem>();
            RectTransform rect = child.GetComponent<RectTransform>();

            if (item != null && rect != null)
            {
                editSessionSnapshot.Add(new PlacedItemData
                {
                    itemId = item.itemId,
                    posX = rect.anchoredPosition.x,
                    posY = rect.anchoredPosition.y,
                    rotZ = item.zRotation
                });
            }
        }
    }

    public void CloseAllItemEditUI()
    {
        if (itemParent == null) return;
        foreach (Transform child in itemParent.transform)
        {
            if (child == null) continue;
            TerrariumItem item = child.GetComponent<TerrariumItem>();
            if (item != null) item.CloseEditUI();
        }
    }

    public void SetItemEditingUI(bool isEditing)
    {
        bool showUI = !isEditing;
        if (editContentPanel != null) editContentPanel.SetActive(showUI);
        if (saveButton != null) saveButton.SetActive(showUI);
        if (cancelButton != null) cancelButton.SetActive(showUI);
        if (previewButton != null) previewButton.SetActive(showUI);
    }

    // ==========================================
    // [3] 인벤토리 UI 갱신 & Empty 텍스트
    // ==========================================
    public void RefreshInventoryUI()
    {
        if (itemButtonParent == null) return;

        foreach (Transform child in itemButtonParent)
        {
            if (child != null) Destroy(child.gameObject);
        }

        if (GameDBManager.Instance == null || GameDBManager.Instance.shopItemMasterDB == null)
            return;

        List<string> placedItemIds = new List<string>();
        if (itemParent != null)
        {
            foreach (Transform child in itemParent.transform)
            {
                if (child == null) continue;
                TerrariumItem tItem = child.GetComponent<TerrariumItem>();
                if (tItem != null && !string.IsNullOrEmpty(tItem.itemId))
                {
                    placedItemIds.Add(tItem.itemId);
                }
            }
        }

        int availableCount = 0;
        foreach (ShopItemData item in GameDBManager.Instance.shopItemMasterDB)
        {
            if (item == null || !item.is_sold_out) continue;

            if (placedItemIds.Contains(item.item_id))
            {
                placedItemIds.Remove(item.item_id);
                continue;
            }

            availableCount++;

            if (itemButtonPrefab == null) continue;
            GameObject itemBtnObj = Instantiate(itemButtonPrefab, itemButtonParent);
            if (itemBtnObj == null) continue;

            Image image = itemBtnObj.GetComponentInChildren<Image>();
            if (image == null) image = itemBtnObj.GetComponent<Image>();
            if (image != null && item.item_image != null) image.sprite = item.item_image;

            Button btn = itemBtnObj.GetComponent<Button>();
            if (btn == null) btn = itemBtnObj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => itemClick(item.item_image, item.item_id));
            }
        }

        if (NothingText != null)
        {
            NothingText.text = "Empty Purchase items from the shop!";
            NothingText.gameObject.SetActive(availableCount == 0);
        }
    }

    public void itemClick(Sprite itemSprite, string targetItemId = "")
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
        if (rect != null) rect.anchoredPosition = Vector2.zero;

        Image newImage = newItem.GetComponent<Image>();
        if (newImage != null) newImage.sprite = itemSprite;

        TerrariumItem item = newItem.GetComponent<TerrariumItem>();
        if (item != null)
        {
            if (!string.IsNullOrEmpty(targetItemId))
            {
                item.itemId = targetItemId;
            }
            else if (GameDBManager.Instance != null && GameDBManager.Instance.shopItemMasterDB != null)
            {
                ShopItemData matchedData = GameDBManager.Instance.shopItemMasterDB.Find(x => x.item_image == itemSprite);
                if (matchedData != null) item.itemId = matchedData.item_id;
            }

            RefreshInventoryUI();
            item.EditItem();
        }

        Invoke(nameof(ResetClickFlag), 0.05f);
    }

    public void itemClick(Sprite itemSprite)
    {
        itemClick(itemSprite, "");
    }

    private void ResetClickFlag()
    {
        isClicking = false;
    }

    // ==========================================
    // [4] Save & Cancel (스냅샷 복구)
    // ==========================================
    public void save()
    {
        Debug.Log("[EditMode] 현재 배치 데이터 세이브");
        List<PlacedItemData> layoutData = new List<PlacedItemData>();

        if (itemParent != null)
        {
            foreach (Transform child in itemParent.transform)
            {
                if (child == null) continue;
                TerrariumItem itemScript = child.GetComponent<TerrariumItem>();
                RectTransform rect = child.GetComponent<RectTransform>();

                if (itemScript != null && rect != null)
                {
                    layoutData.Add(new PlacedItemData
                    {
                        itemId = itemScript.itemId,
                        posX = rect.anchoredPosition.x,
                        posY = rect.anchoredPosition.y,
                        rotZ = itemScript.zRotation
                    });
                }
            }
        }

        CaptureCurrentSnapshot();

        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.SaveTerrariumLayout(layoutData);
        }
    }

    public void cancel()
    {
        Debug.Log("[EditMode] 배치 취소: 회전 및 위치를 스냅샷 원래대로 롤백");

        ClearFieldItems();

        if (editSessionSnapshot != null && editSessionSnapshot.Count > 0)
        {
            SpawnLayout(editSessionSnapshot);
        }
        else if (UGSManager.Instance != null && UGSManager.Instance.currentLayout != null)
        {
            SpawnLayout(UGSManager.Instance.currentLayout);
        }
        else
        {
            RefreshInventoryUI();
        }

        if (isPreviewMode) ExitPreviewMode();
    }

    private void ClearFieldItems()
    {
        if (itemParent == null) return;

        List<Transform> children = new List<Transform>();
        foreach (Transform child in itemParent.transform)
        {
            if (child != null) children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    public void SpawnLayout(List<PlacedItemData> layout)
    {
        if (layout == null || itemPrefab == null || itemParent == null) return;

        ClearFieldItems();

        foreach (var data in layout)
        {
            if (GameDBManager.Instance == null) continue;
            ShopItemData shopItem = GameDBManager.Instance.GetShopItemData(data.itemId);
            if (shopItem == null) continue;

            GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
            if (newItem == null) continue;

            RectTransform rect = newItem.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = new Vector2(data.posX, data.posY);

            Image newImage = newItem.GetComponent<Image>();
            if (newImage != null) newImage.sprite = shopItem.item_image;

            TerrariumItem itemScript = newItem.GetComponent<TerrariumItem>();
            if (itemScript != null)
            {
                itemScript.itemId = data.itemId;
                itemScript.SetRotation(data.rotZ);
            }
        }

        RefreshInventoryUI();
    }
}