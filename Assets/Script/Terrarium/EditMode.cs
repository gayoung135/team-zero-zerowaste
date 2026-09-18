using UnityEngine;
using UnityEngine.UI;

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false; //에딧모드인지 확인용 (에딧 아니면 취소)
    public GameObject itemPrefab; //복제될 아이템 프리팹
    public GameObject itemParent; //복제된 아이템 배치할 위치
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void itemClick(Sprite itemSprite, string itemName) //목록 아이템 클릭 시 복제 및 설정모드로
    {
        GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
        RectTransform rect = newItem.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 0);

        // 이름/이미지 복사
        newItem.name = itemName;
        Image newImage = newItem.GetComponent<Image>();
        newImage.sprite = itemSprite;

        TerrariumItem item = newItem.GetComponent<TerrariumItem>();
        item.EditItem();
    }

    public void EditModeOn() //에딧버튼
    {
        isEditMode = true;
    }
    
    public void save() //저장버튼
    {
        Debug.Log("테라리움 저장 (나중에 수정하기)");
        isEditMode = false;
    }

    public void cancel() //취소버튼
    {
        Debug.Log("저장 취소 (나중에 수정하기)");
        isEditMode = false;
    }
}
