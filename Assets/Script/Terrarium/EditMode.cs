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
        //나중에 아이템 불러오는 코드 이 아래에 추가하기
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
        Debug.Log("저장");
        //tempbox에 들어있는 아이템들의 종류와 좌표를 데이터베이스에 저장해야 합니다
        isEditMode = false;
    }

    public void cancel() //취소버튼
    {
        Debug.Log("취소");
        //데이터베이스에 저장된 아이템을 tempbox로 불러오는 기능을 구현해야 합니다
        isEditMode = false;
    }
}