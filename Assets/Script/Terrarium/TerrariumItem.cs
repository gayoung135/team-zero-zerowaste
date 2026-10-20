using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TerrariumItem : MonoBehaviour, IDragHandler
{
    public string itemId;
    public GameObject UI;
    private EditMode manager;
    private GameObject EditUI;
    private GameObject PreviewUI;
    private RectTransform rectTransform;
    private Canvas canvas;
    private PlayableGraph graph;
    private bool isItemEditmode = false;

    void Awake()
    {
        manager = GameObject.Find("SettingsManager").GetComponent<EditMode>();
        EditUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "Edit_UI");
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        ApplyAnimation();
    }

    public void EditItem() //설정모드
    {
        if (!manager.isEditMode || manager.selectedChild != null) return;
        UI.SetActive(true);
        EditUI.SetActive(false);
        PreviewUI = GameObject.Find("Preview");
        if(PreviewUI!=null) PreviewUI.SetActive(false);
        isItemEditmode = true;
        manager.selectedChild = this.gameObject;
    }

    public void ApplyAnimation() //애니메이션 존재 시 불러오기
    {
        ShopItemData shopItem = GameDBManager.Instance.GetShopItemData(itemId);

        if (shopItem.animation == null) return;

        //여기부터
        Animator animator = GetComponent<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();

        AnimationPlayableUtilities.PlayClip(animator, shopItem.animation, out graph);
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
        manager.selectedChild = null;
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
        isItemEditmode = false;
        manager.selectedChild = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }
}