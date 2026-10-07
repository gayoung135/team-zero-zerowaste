using UnityEngine;
using UnityEngine.EventSystems;

public class TerrariumItem : MonoBehaviour, IDragHandler
{
    public string itemId;
    public float zRotation = 0f;

    private GameObject UI; 

    private EditMode manager;
    private GameObject editUIObject;
    private GameObject PreviewUI;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isItemEditmode = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (transform.childCount > 0)
        {
            UI = transform.GetChild(0).gameObject;
        }
        else
        {
            Transform uiTransform = transform.Find("UI") ?? transform.Find("ui");
            if (uiTransform != null) UI = uiTransform.gameObject;
        }

        manager = EditMode.Instance != null ? EditMode.Instance : FindObjectOfType<EditMode>(true);
        if (manager != null)
        {
            editUIObject = manager.editUIPanel != null ? manager.editUIPanel : manager.gameObject;
        }
    }

    public void EditItem() 
    {
        if (manager == null) manager = EditMode.Instance != null ? EditMode.Instance : FindObjectOfType<EditMode>(true);
        if (manager == null)
        {
            Debug.LogError("[에러] 씬 내부에서 EditMode 스크립트를 찾을 수 없습니다.");
            return;
        }

        if (!manager.isEditMode) return;
        
        // 1. 아이템 자체의 조작 기즈모 UI(체크, 회전, 삭제) 켜기
        if (UI != null)
        {
            UI.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[경고] {gameObject.name} 프리팹 내부에 자식 오브젝트(조작 UI)가 단 하나도 존재하지 않습니다.");
        }
        
        // 2. 편집 UI 패널 (아이템 바, Save, Cancel 버튼 등) 숨기기
        if (editUIObject == null && manager != null)
        {
            editUIObject = manager.editUIPanel != null ? manager.editUIPanel : manager.gameObject;
        }

        if (editUIObject != null)
        {
            editUIObject.SetActive(false);
        }

        PreviewUI = GameObject.Find("Preview");
        if (PreviewUI != null) PreviewUI.SetActive(false);
        
        isItemEditmode = true;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        if (manager == null) manager = EditMode.Instance != null ? EditMode.Instance : FindObjectOfType<EditMode>(true);
        if (manager != null && manager.isEditMode && isItemEditmode)
        {
            if (canvas != null && rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }
    }

    /// <summary>
    /// 초록색 Check (완료) 버튼 클릭 시 실행
    /// </summary>
    public void check() 
    {
        // 1. 숨겨두었던 편집 UI 패널 (아이템 바, Save, Cancel) 다시 나타나게 만들기
        if (editUIObject != null)
        {
            editUIObject.SetActive(true);
        }
        else if (manager != null)
        {
            manager.SetEditUIVisibility(true);
        }

        // 2. 아이템 자체의 조작 UI 끄기
        if (UI != null) UI.SetActive(false);
        isItemEditmode = false;
    } 

    public void Rotation() 
    {
        zRotation = (zRotation + 30f) % 360f;
        if (rectTransform != null) rectTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
        if (UI != null) UI.transform.localRotation = Quaternion.Euler(0, 0, -zRotation);
    }

    public void SetRotation(float angle)
    {
        zRotation = angle;
        if (rectTransform != null) rectTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
        if (UI != null) UI.transform.localRotation = Quaternion.Euler(0, 0, -zRotation);
    }

    /// <summary>
    /// 빨간색 Delete (삭제) 버튼 클릭 시 실행
    /// </summary>
    public void Delete() 
    {
        // 삭제 시에도 편집 UI 패널 다시 나타나도록 복구
        if (editUIObject != null)
        {
            editUIObject.SetActive(true);
        }
        else if (manager != null)
        {
            manager.SetEditUIVisibility(true);
        }

        Destroy(gameObject);
        isItemEditmode = false;
    }
}