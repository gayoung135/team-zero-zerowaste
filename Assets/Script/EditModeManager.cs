using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditModeManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemState
    {
        public GameObject itemObject;
        public Vector3 savedPosition;
        public bool savedActiveSelf;
    }

    [Header("--- UI Group Containers ---")]
    public GameObject normalUIGroup; // Normal_UI 오브젝트
    public GameObject editUIGroup;   // Edit_UI 오브젝트

    [Header("--- Edit UI Elements ---")]
    public GameObject itemBarPanel;   // 하단 아이템 바 패널
    public GameObject emptyText;      // "Empty. Purchase..." 텍스트

    [Header("--- Preview Toggle Elements ---")]
    public GameObject saveButtonObject;   // SAVE 버튼 오브젝트
    public GameObject cancelButtonObject; // CANCEL 버튼 오브젝트
    
    [Header("--- Target Placement Group ---")]
    public Transform terrariumContainer; // 테라리움 내부의 배치 아이템들의 부모 오브젝트

    // [연결 인터페이스] 다른 스크립트에서 참조할 수 있게 열어둡니다.
    [Header("--- Item Container Reference ---")]
    public GameObject itemContainer;     // 하이어라키의 Items 오브젝트

    private List<ItemState> originalStates = new List<ItemState>();
    private bool isPreviewActive = false;

    // 가상의 보유 아이템 여부를 판별하기 위한 리스트 (상태 제어용으로 유지)
    private List<string> ownedItems = new List<string>(); 

    void Start()
    {
        // 게임 시작 시에는 일반 모드로 초기화!
        SetEditMode(false);
    }

    // --- 1. 편집 모드 스위칭 핵심 함수 ---
    public void SetEditMode(bool isEdit)
    {
        normalUIGroup.SetActive(!isEdit);
        editUIGroup.SetActive(isEdit);

        // 편집 모드 진입 시 세팅
        if (isEdit)
        {
            isPreviewActive = false;
            itemBarPanel.SetActive(true); // 격자선이 빠졌으므로 하단 바만 켜줍니다.
            
            // 1) 현재 배치된 아이템들의 상태(위치 등) 백업 (CANCEL 대비)
            BackupCurrentStates();

            // 2) 소유한 아이템이 없으면 Empty 텍스트 활성화
            UpdateItemBarStatus();
        }
    }

    // --- 2. 백업 & 복원 시스템 (CANCEL 핵심 로직) ---
    private void BackupCurrentStates()
    {
        originalStates.Clear();
        if (terrariumContainer == null) return;

        foreach (Transform child in terrariumContainer)
        {
            ItemState state = new ItemState
            {
                itemObject = child.gameObject,
                savedPosition = child.localPosition,
                savedActiveSelf = child.gameObject.activeSelf
            };
            originalStates.Add(state);
        }
    }

    private void RestoreOriginalStates()
    {
        foreach (var state in originalStates)
        {
            if (state.itemObject != null)
            {
                state.itemObject.transform.localPosition = state.savedPosition;
                state.itemObject.SetActive(state.savedActiveSelf);
            }
        }
        originalStates.Clear();
    }

    // 진열장 생성 루프를 완전히 걷어내고, Items 오브젝트의 활성화 상태와 Empty 메시지만 단순 제어합니다.
    private void UpdateItemBarStatus()
    {
        if (ownedItems.Count == 0)
        {
            if (emptyText != null) emptyText.SetActive(true);
            if (itemContainer != null) itemContainer.SetActive(false); // 아이템이 없으면 진열장 영역을 숨깁니다.
        }
        else
        {
            if (emptyText != null) emptyText.SetActive(false);
            if (itemContainer != null) itemContainer.SetActive(true);  // 아이템이 있으면 진열장 영역을 켭니다.
        }
    }

    // --- 3. 각 버튼 클릭 시 매핑될 함수들 ---

    // [EDIT] 버튼 클릭
    public void OnClickEditButton()
    {
        SetEditMode(true);
    }

    // [SAVE] 버튼 클릭
    public void OnClickSaveButton()
    {
        Debug.Log("변경 사항이 성공적으로 저장되었습니다!");
        originalStates.Clear(); 
        SetEditMode(false);     
    }

    // [CANCEL] 버튼 클릭
    public void OnClickCancelButton()
    {
        Debug.Log("편집을 취소하고 이전 상태로 되돌립니다.");
        RestoreOriginalStates();
        SetEditMode(false);     
    }

    // [PREVIEW] 버튼 클릭 시 호출되는 함수
    public void OnClickPreviewButton()
    {
        // 프리뷰 상태 토글 (true ↔ false)
        isPreviewActive = !isPreviewActive;

        // 1. 하단 아이템 바 숨기기/켜기
        if (itemBarPanel != null)
            itemBarPanel.SetActive(!isPreviewActive);

        // 2. "Empty..." 경고 텍스트 숨기기/켜기
        if (emptyText != null)
        {
            if (isPreviewActive)
            {
                emptyText.SetActive(false); // 프리뷰 중에는 무조건 숨김
            }
            else
            {
                UpdateItemBarStatus(); // 프리뷰 종료 시, 현재 상태에 맞춰서 복원
            }
        }

        // [추가 연동] 프리뷰 시 가로로 나열된 아이템들(itemContainer) 영역도 함께 숨김/복원 처리합니다.
        if (itemContainer != null)
        {
            if (isPreviewActive) itemContainer.SetActive(false);
            else UpdateItemBarStatus();
        }

        // 3. SAVE 버튼 숨기기/켜기
        if (saveButtonObject != null) 
            saveButtonObject.SetActive(!isPreviewActive);

        // 4. CANCEL 버튼 숨기기/켜기
        if (cancelButtonObject != null) 
            cancelButtonObject.SetActive(!isPreviewActive);
    }
}