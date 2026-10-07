using UnityEngine;
using UnityEngine.EventSystems;

public class TerrariumItem : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [Header("아이템 정보")]
    public string itemId;
    public float zRotation = 0f;

    [Header("아이템 자체 조작 UI")]
    public GameObject UI;

    private EditMode manager;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isItemEditmode = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        manager = FindObjectOfType<EditMode>(true);

        FindUIObject();
    }

    private void FindUIObject()
    {
        if (UI != null) return;

        Transform uiTransform = transform.Find("UI") ?? transform.Find("ui") ?? transform.Find("EditUI");
        if (uiTransform != null)
        {
            UI = uiTransform.gameObject;
            return;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponentInChildren<UnityEngine.UI.Button>() != null)
            {
                UI = child.gameObject;
                return;
            }
        }

        if (transform.childCount > 0)
        {
            UI = transform.GetChild(0).gameObject;
        }
    }

    // [이미 배치된 아이템 터치 시 다시 조작창 열기]
    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null) manager = FindObjectOfType<EditMode>(true);
        if (manager != null && manager.isEditMode && !isItemEditmode)
        {
            EditItem();
        }
    }

    public void EditItem() 
    {
        if (manager == null) manager = FindObjectOfType<EditMode>(true);
        if (manager != null && !manager.isEditMode) return;
        
        FindUIObject();

        if (UI != null)
        {
            UI.SetActive(true);
        }

        if (manager != null)
        {
            manager.SetItemEditingUI(true);
        }
        
        isItemEditmode = true;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        if (manager == null) manager = FindObjectOfType<EditMode>(true);
        if (manager != null && manager.isEditMode && isItemEditmode)
        {
            if (canvas != null && rectTransform != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
        }
    }

    public void check() 
    {
        if (UI != null) UI.SetActive(false);
        isItemEditmode = false;

        if (manager != null)
        {
            manager.SetItemEditingUI(false);
            manager.RefreshInventoryUI();
        }
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
        transform.SetParent(null);
        Destroy(gameObject);

        isItemEditmode = false;

        if (manager != null)
        {
            manager.SetItemEditingUI(false);
            manager.RefreshInventoryUI();
        }
    }

    public void CloseEditUI()
    {
        if (UI != null) UI.SetActive(false);
        isItemEditmode = false;
    }
}