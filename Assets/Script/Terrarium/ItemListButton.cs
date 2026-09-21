using UnityEngine;
using UnityEngine.UI;

public class ItemListButton : MonoBehaviour
{
    public string itemName;
    public Sprite image;

    public void ItemClick()
    {
        GameObject settingsObj = GameObject.Find("SettingsManager");
        if (settingsObj != null)
        {
            EditMode manager = settingsObj.GetComponent<EditMode>();
            if (manager != null)
            {
                // 1. 아이템 생성 및 참조 가져오기
                TerrariumItem createdItem = manager.itemClick(image, itemName);

                // 2. 생성된 아이템에 본인 슬롯 연결
                if (createdItem != null)
                {
                    createdItem.originSlot = this.gameObject;
                }

                // 3. 하단바 슬롯 숨기기
                gameObject.SetActive(false);

                // 4. Empty 문구 갱신
                manager.UpdateEmptyMessage();
            }
        }
    }
}