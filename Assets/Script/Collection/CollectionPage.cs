using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings; 

public class CollectionPage : MonoBehaviour
{
    [Header("[UI Components]")]
    public Image itemImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI methodText;
    public TextMeshProUGUI tipText;

    public void UpdatePage(TrashGuideData trashData)
    {
        // 데이터 검증 방어선
        if (trashData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        itemImage.sprite = trashData.item_image;

        // UI 컴포넌트 유실 방어선
        if (itemImage == null || nameText == null || methodText == null || tipText == null)
        {
            Debug.LogError($"[{gameObject.name}] 인스펙터에 TMP 텍스트나 이미지 컴포넌트 연결이 유실되었습니다!");
            return;
        }

        if (trashData.is_unlock)
        {
            // [Unlocked State] 해금 완료 상태
            itemImage.color = Color.white;

            if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko")
            {
                nameText.text = "이름: " + trashData.name_KR;
                methodText.text = "분류 방법: " + trashData.method_KR;
                tipText.text = "팁: " + trashData.tip_KR;
            }
            else
            {
                nameText.text = "Name: " + trashData.name;
                methodText.text = "How to recycle: " + trashData.method;
                tipText.text = "Tip: " + trashData.tip;
            }
        }
        else
        {
            // [Locked State] 잠긴 상태
            itemImage.color = new Color(0.1f, 0.1f, 0.1f, 1.0f); // 어두운 실루엣 처리

            if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko")
            {
                nameText.text = "이름: ???";
                methodText.text = "분류 방법: ???";
                tipText.text = "팁: 아직 이 쓰레기를 분리배출하지 못했습니다.";
            }
            else
            {
                nameText.text = "Name: ???";
                methodText.text = "How to recycle: ???";
                tipText.text = "Tip: You haven't recycled this item yet.";
            }
        }
    }
}
