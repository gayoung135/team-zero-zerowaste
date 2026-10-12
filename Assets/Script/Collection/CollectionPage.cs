using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CollectionPage : MonoBehaviour
{
    [Header("[UI Components]")]
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI methodText;
    public TextMeshProUGUI tipText;

    public void UpdatePage(TrashGuideData trashData, Sprite trashSprite)
    {
        // 데이터 없는 페이지면 UI 비활성화
        if (trashData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (CollectionManager.Instance != null && CollectionManager.Instance.IsUnlocked(trashData.trash_name))
        {
            // [Unlocked State]
            if (trashSprite != null)
            {
                itemImage.sprite = trashSprite;
                itemImage.color = Color.white;
            }

            // DB 영어 텍스트 출력
            nameText.text = "Name: " + trashData.trash_name;
            methodText.text = "How to recycle: " + trashData.log_correct;
            tipText.text = "Tip: " + trashData.tip_ingame;
        }
        else
        {
            // [Locked State]
            if (trashSprite != null)
            {
                itemImage.sprite = trashSprite;
                itemImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Silhouette
            }

            nameText.text = "Name: ???";
            methodText.text = "How to recycle: ???";
            tipText.text = "Tip: You haven't recycled this item yet.";
        }
    }
}
