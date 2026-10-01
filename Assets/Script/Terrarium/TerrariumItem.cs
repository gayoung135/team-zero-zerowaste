using UnityEngine;
using UnityEngine.EventSystems;

public class TerrariumItem : MonoBehaviour, IDragHandler
{
    public GameObject UI;
    [HideInInspector] public GameObject originSlot; // 생성 시 사용한 하단바 슬롯 참조

    private EditMode manager;
    private GameObject EditUI;
    private GameObject PreviewUI;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isItemEditmode = false;

    void Awake()
    {
        GameObject settingsObj = GameObject.Find("SettingsManager");
        if (settingsObj != null)
        {
            manager = settingsObj.GetComponent<EditMode>();
            if (manager.editUI != null) EditUI = manager.editUI;
        }

        if (EditUI == null) EditUI = GameObject.Find("Edit_UI");

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void EditItem()
    {
        if (!manager.isEditMode) return;
        UI.SetActive(true);
        EditUI.SetActive(false);
        PreviewUI = GameObject.Find("Preview");
        if(PreviewUI!=null) PreviewUI.SetActive(false);
        isItemEditmode = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager != null && manager.isEditMode && isItemEditmode)
        {
            float scale = (canvas != null) ? canvas.scaleFactor : 1f;
            rectTransform.anchoredPosition += eventData.delta / scale;
        }
    }

    public void check() // 설치 완료 (V 버튼)
    {
        if (EditUI != null) EditUI.SetActive(true);
        if (UI != null) UI.SetActive(false);
        isItemEditmode = false;

        // if (manager != null) manager.UpdateEmptyMessage();
    }

    public void Rotation()
    {
        rectTransform.Rotate(0, 0, 30);
        if (UI != null) UI.transform.Rotate(0, 0, -30);
    }

    // x 버튼(삭제/취소) 누를 때: 슬롯 복구 및 Empty 잔상 방지
    public void Delete()
    {
        if (EditUI != null) EditUI.SetActive(true);

        // 1. 원본 인벤토리 슬롯 다시 활성화
        if (originSlot != null)
        {
            originSlot.SetActive(true);
        }

        // 2. UI 레이아웃 상태 즉시 동기화
        Canvas.ForceUpdateCanvases();

        // 3. Empty 문구 즉시 갱신 (슬롯이 들어왔으므로 문구 비활성화)
        if (manager != null)
        {
            //manager.UpdateEmptyMessage();
        }

        // 4. 오브젝트 파괴
        isItemEditmode = false;
        Destroy(gameObject);
    }
}