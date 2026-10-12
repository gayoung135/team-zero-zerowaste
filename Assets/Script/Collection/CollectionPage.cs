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

            // 언어 코드(en, ko) 체크
            string currentLanguage = LocalizationSettings.SelectedLocale != null 
                ? LocalizationSettings.SelectedLocale.Identifier.Code 
                : "en";

            if (currentLanguage == "ko")
            {
                // 1. 한국어 모드일 때
                nameText.text = "이름: " + LocalizationSettings.StringDatabase.GetLocalizedString("TrashTable", trashData.trash_name + "_NAME");
                methodText.text = "분류 방법: " + LocalizationSettings.StringDatabase.GetLocalizedString("TrashTable", trashData.trash_name + "_METHOD");
                tipText.text = "팁: " + LocalizationSettings.StringDatabase.GetLocalizedString("TrashTable", trashData.trash_name + "_TIP");
            }
            else
            {
                // 2. 영어 모드일 때 (원본 유지)
                nameText.text = "Name: " + trashData.trash_name;
                methodText.text = "How to recycle: " + trashData.log_correct;
                tipText.text = "Tip: " + trashData.tip_ingame;
            }
        }
        else
        {
            // [Locked State]
            if (trashSprite != null)
            {
                itemImage.sprite = trashSprite;
                itemImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Silhouette
            }

            // 잠긴 물음표 언어 대응되도록 처리
            if (LocalizationSettings.SelectedLocale != null && LocalizationSettings.SelectedLocale.Identifier.Code == "ko")
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
