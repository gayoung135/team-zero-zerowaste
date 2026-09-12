using UnityEngine;
using UnityEngine.UI;

public class EditModeManager : MonoBehaviour
{
    [Header("--- UI Panel Containers ---")]
    public GameObject editUIGroup;        // 하단 편집 UI 패널 (슬롯, Empty 텍스트 등)
    public GameObject gridObject;         // 테라리움 바닥/벽면 그리드 표시 오브젝트

    [Header("--- Bottom Mode Buttons ---")]
    public GameObject storeButton;        // STORE 버튼
    public GameObject editButton;         // EDIT 버튼 (메인/프리뷰 화면용)
    public GameObject previewButton;      // PREVIEW 버튼 (편집 화면용)
    public GameObject saveButtonObject;   // SAVE 버튼
    public GameObject cancelButtonObject; // CANCEL 버튼

    [Header("--- Top Right UI ---")]
    public GameObject coinsUI;            // COINS UI (기본 메인 화면)

    [Header("--- Edit UI Elements ---")]
    public GameObject itemBarPanel;      // 하단 아이템 슬롯 패널
    public GameObject emptyText;         // "Empty..." 경고 텍스트

    private bool isEditMode = false;

    void Start()
    {
        // 게임 시작 시 메인 화면 상태로 초기화
        SetEditMode(false);
    }

    // -------------------------------------------------------------
    // 하단바 Empty 문구 자동 갱신
    // -------------------------------------------------------------
    public void UpdateEmptyMessage()
    {
        if (emptyText != null && itemBarPanel != null)
        {
            InventorySlotDrag[] activeSlots = itemBarPanel.GetComponentsInChildren<InventorySlotDrag>(false);
            bool isEmpty = (activeSlots.Length == 0);
            emptyText.SetActive(isEmpty);
        }
    }

    // -------------------------------------------------------------
    // 1. 모드 상태 변경 핵심 로직
    // -------------------------------------------------------------
    public void SetEditMode(bool edit)
    {
        isEditMode = edit;

        // 1) 그리드 및 편집 UI 패널 제어
        if (gridObject != null) gridObject.SetActive(isEditMode);
        if (editUIGroup != null) editUIGroup.SetActive(isEditMode);
        if (itemBarPanel != null) itemBarPanel.SetActive(isEditMode);

        // 2) 버튼 토글 (기획 요청 반영)
        // 편집 중: PREVIEW, SAVE, CANCEL 표시 / STORE, EDIT 숨김
        // 편집 완료(메인/프리뷰): EDIT, STORE 표시 / PREVIEW, SAVE, CANCEL 숨김
        if (editButton != null) editButton.SetActive(!isEditMode);
        if (storeButton != null) storeButton.SetActive(!isEditMode);

        if (previewButton != null) previewButton.SetActive(isEditMode);
        if (saveButtonObject != null) saveButtonObject.SetActive(isEditMode);
        if (cancelButtonObject != null) cancelButtonObject.SetActive(isEditMode);

        // 3) 상단 COINS UI 제어
        if (coinsUI != null) coinsUI.SetActive(!isEditMode);

        // 4) 편집 모드 진입 시 슬롯 확인
        if (isEditMode)
        {
            UpdateEmptyMessage();
        }
    }

    // -------------------------------------------------------------
    // 2. 버튼 클릭 이벤트
    // -------------------------------------------------------------

    // [EDIT] 버튼 클릭 (편집 시작 or 프리뷰 해제 후 재편집)
    public void OnClickEditButton()
    {
        SetEditMode(true);
    }

    // [PREVIEW] 버튼 클릭 (배치 확인: 그리드 사라짐 & PREVIEW 위치가 EDIT 버튼으로 변경)
    public void OnClickPreviewButton()
    {
        Debug.Log("[EditModeManager] PREVIEW 모드: 그리드 해제 및 배치 완료 상태 확인");

        // 개구리 선택 조작 중이었다면 해제
        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.DeselectItem();
        }

        // 그리드 및 편집 UI만 끌 뿐, 메인 화면(EDIT/STORE 버튼 재활성화) 상태로 전환
        SetEditMode(false);
    }

    // [SAVE] 버튼 클릭 (배치 저장 후 메인으로)
    public void OnClickSaveButton()
    {
        Debug.Log("[EditModeManager] SAVE 클릭: 현재 테라리움 배치 저장 완료.");

        // 1. 선택 해제
        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.DeselectItem();
        }

        // 2. 테라리움 배치 상태 저장 (TerrariumManager에 위치 데이터 저장 요청)
        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.SaveTerrariumLayout();
        }

        // 3. 편집 모드 종료 ➔ 테라리움 메인으로 이동
        SetEditMode(false);
    }

    // [CANCEL] 버튼 클릭 (편집 취소 후 메인으로)
    public void OnClickCancelButton()
    {
        Debug.Log("[EditModeManager] CANCEL 클릭: 편집 취소 및 메인으로 복귀.");

        // 1. 선택 해제
        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.DeselectItem();
        }

        // 2. 편집 취소 (저장되지 않은 변경사항을 원래대로 복구)
        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.CancelTerrariumLayout();
        }

        // 3. 편집 모드 종료 ➔ 테라리움 메인으로 이동
        SetEditMode(false);
    }
}