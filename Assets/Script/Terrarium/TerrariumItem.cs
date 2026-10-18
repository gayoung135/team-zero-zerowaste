using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class TerrariumItem : MonoBehaviour, IDragHandler
{
    public string itemId;
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
        EditUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "Edit_UI");
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
        manager.selectedChild = this.gameObject;
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

    public void ZoomIn() //확대 
    {
        if (rectTransform.localScale.x < 2f) rectTransform.localScale += new Vector3(0.2f, 0.2f, 0f);
    }

    public void ZoomOut() //축소
    {
        if (rectTransform.localScale.x > 0.6f) rectTransform.localScale += new Vector3(-0.2f, -0.2f, 0f);
    }
    
    public void Delete() //삭제
    {
        EditUI.SetActive(true);
        Destroy(gameObject);
        isItemEditmode = false;
    }
}