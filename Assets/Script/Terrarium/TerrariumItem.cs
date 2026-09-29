using UnityEngine;
using UnityEngine.EventSystems;

public class TerrariumItem : MonoBehaviour, IDragHandler
{
    public GameObject UI;
    private EditMode manager;
    private GameObject EditUI;
    private GameObject PreviewUI;
    private RectTransform rectTransform;
    private Canvas canvas;
    private bool isItemEditmode = false;

    void Awake()
    {
        manager = GameObject.Find("SettingsManager").GetComponent<EditMode>();
        EditUI = GameObject.Find("Edit_UI");
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void EditItem() //설정모드
    {
        if (!manager.isEditMode) return;
        UI.SetActive(true);
        EditUI.SetActive(false);
        PreviewUI = GameObject.Find("Preview");
        if(PreviewUI!=null) PreviewUI.SetActive(false);
        isItemEditmode = true;
    }

    public void OnDrag(PointerEventData eventData) //아이템 이동
    {
        if(manager.isEditMode && isItemEditmode==true) rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void check() //설치 완료
    {
        EditUI.SetActive(true);
        UI.SetActive(false);
        isItemEditmode = false;
    } 

    public void Rotation() //회전
    {
        rectTransform.Rotate(0, 0, 30);
        UI.transform.Rotate(0, 0, -30);
    }

    public void Delete() //삭제
    {
        EditUI.SetActive(true);
        Destroy(gameObject);
        isItemEditmode = false;
    }
}