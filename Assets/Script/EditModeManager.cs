using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EditModeManager : MonoBehaviour
{
    // -------------------------------------------------------------
    // UI 변수 선언 영역
    // -------------------------------------------------------------
    [Header("--- UI Group Containers ---")]
    public GameObject editUIGroup;   // Terrarium_UI -> Edit_UI (Panel, Text)

    [Header("--- Buttons ---")] 
    public GameObject storeButton;       // STORE 버튼
    public GameObject editButton;        // EDIT 버튼
    public GameObject saveButtonObject;  // SAVE 버튼
    public GameObject cancelButtonObject;// CANCEL 버튼

    [Header("--- Top Right UI ---")] 
    public GameObject coinsUI;           // COINS UI (기본 화면)
    public GameObject previewButton;     // PREVIEW 버튼 (에디트 모드 화면)

    [Header("--- Edit UI Elements ---")]
    public GameObject itemBarPanel;   // 하단 아이템 바 패널
    public GameObject emptyText;      // "Empty. Purchase..." 텍스트

    private bool isPreviewActive = false;

    void Start()
    {
        // 게임 시작 시에는 일반 모드로 초기화!
        SetEditMode(false);
    }

    // -------------------------------------------------------------
    // 1. 편집 모드 스위칭 핵심 함수
    // -------------------------------------------------------------
    public void SetEditMode(bool isEdit)
    {
        // 1) 하단 에디트 패널(Panel, Text) 활성화/비활성화
        if (editUIGroup != null) editUIGroup.SetActive(isEdit);

        // 2) 하단 버튼 스위칭 (STORE/EDIT ↔ SAVE/CANCEL)
        if (storeButton != null) storeButton.SetActive(!isEdit);
        if (editButton != null) editButton.SetActive(!isEdit);
        if (saveButtonObject != null) saveButtonObject.SetActive(isEdit);
        if (cancelButtonObject != null) cancelButtonObject.SetActive(isEdit);

        // 3) 우측 상단 UI 스위칭 (COINS ↔ PREVIEW)
        if (coinsUI != null) coinsUI.SetActive(!isEdit);
        if (previewButton != null) previewButton.SetActive(isEdit);

        // 4) 편집 모드 진입 시 로직
        if (isEdit)
        {
            isPreviewActive = false;
            if (itemBarPanel != null) itemBarPanel.SetActive(true);
            if (emptyText != null) emptyText.SetActive(true); // 기본적으로 Empty 메시지 활성화
        }
    }

    // -------------------------------------------------------------
    // 2. 버튼 클릭 이벤트 매핑
    // -------------------------------------------------------------

    // [EDIT] 버튼 클릭
    public void OnClickEditButton()
    {
        SetEditMode(true);
    }

    // [SAVE] 버튼 클릭
    public void OnClickSaveButton()
    {
        Debug.Log("변경 사항이 저장되었습니다.");
        SetEditMode(false);     
    }

    // [CANCEL] 버튼 클릭
    public void OnClickCancelButton()
    {
        Debug.Log("편집을 취소합니다.");
        SetEditMode(false);     
    }

    // -------------------------------------------------------------
    // 3. PREVIEW 버튼 클릭 로직
    // -------------------------------------------------------------
    public void OnClickPreviewButton()
    {
        isPreviewActive = !isPreviewActive;

        // 1. 하단 에디트 패널/아이템 바 숨기기/켜기
        if (editUIGroup != null) editUIGroup.SetActive(!isPreviewActive);
        if (itemBarPanel != null) itemBarPanel.SetActive(!isPreviewActive);

        // 2. "Empty..." 경고 텍스트 숨기기/켜기
        if (emptyText != null) emptyText.SetActive(!isPreviewActive);

        // 3. SAVE / CANCEL 버튼 숨기기/켜기
        if (saveButtonObject != null) saveButtonObject.SetActive(!isPreviewActive);
        if (cancelButtonObject != null) cancelButtonObject.SetActive(!isPreviewActive);
    }
}