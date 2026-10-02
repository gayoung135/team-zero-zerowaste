using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditMode : MonoBehaviour
{
    public bool isEditMode = false; //에딧모드인지 확인용 (에딧 아니면 취소)
    public GameObject itemPrefab; //복제될 아이템 프리팹
    public GameObject itemParent; //복제된 아이템 배치할 위치
    public GameObject itemButtonPrefab; //아이템 버튼 프리팹
    public Transform itemButtonParent; //인벤토리
    public TextMeshProUGUI NothingText; //인벤토리 비었을때 띄울 텍스트
    private RectTransform rectTransform;
    bool hasItem = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        foreach (ShopItemData item in GameDBManager.Instance.shopItemMasterDB) //아이템 불러오기
        {
            if (item.is_sold_out)
            {
                hasItem = true; //아이템 보유중인지 판별
                GameObject Item = Instantiate(itemButtonPrefab, itemButtonParent);
                ItemListButton ItemScript = Item.GetComponent<ItemListButton>();
                if (ItemScript != null)
                {
                    Image image = Item.GetComponentInChildren<Image>();
                    if (image != null) image.sprite = item.item_image;
                }
            }
        }

        NothingText.gameObject.SetActive(!hasItem);
    }

    public void itemClick(Sprite itemSprite) //목록 아이템 클릭 시 복제 및 설정모드로
    {
        GameObject newItem = Instantiate(itemPrefab, itemParent.transform);
        RectTransform rect = newItem.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 0);

        //이미지 복사
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
        //tempbox에 들어있는 아이템들의 이미지와 좌표를 데이터베이스에 저장해야 합니다
        isEditMode = false;
    }

    public void cancel() //취소버튼
    {
        Debug.Log("취소");
        //데이터베이스에 저장된 아이템을 tempbox로 불러오는 기능을 구현해야 합니다
        isEditMode = false;
    }
}