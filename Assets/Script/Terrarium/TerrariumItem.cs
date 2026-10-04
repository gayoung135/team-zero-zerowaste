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

        manager = FindObjectOfType<EditMode>(true);
        if (manager != null)
        {
            editUIObject = manager.gameObject;
        }
    }

    public void EditItem() 
    {
        if (manager == null) manager = FindObjectOfType<EditMode>(true);
        if (manager == null)
        {
            Debug.LogError("[에러] 씬 내부에서 EditMode 스크립트(SettingsManager)를 찾을 수 없습니다.");
            return;
        }

        if (!manager.isEditMode) return;
        
        if (UI != null)
        {
            UI.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[경고] {gameObject.name} 프리팹 내부에 자식 오브젝트(조작 UI)가 단 하나도 존재하지 않습니다.");
        }
        
        if (editUIObject != null) editUIObject.SetActive(false);

        PreviewUI = GameObject.Find("Preview");
        if(PreviewUI != null) PreviewUI.SetActive(false);
        
        isItemEditmode = true;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        if (manager == null) manager = FindObjectOfType<EditMode>(true);
        if (manager != null && manager.isEditMode && isItemEditmode == true)
        {
            if (canvas != null && rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }
    }

    public void check() 
    {
        if (editUIObject != null) editUIObject.SetActive(true);

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

    public void Delete() 
    {
        if (editUIObject != null) editUIObject.SetActive(true);
        Destroy(gameObject);
        isItemEditmode = false;
    }
}
