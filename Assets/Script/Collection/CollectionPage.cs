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
        // 1. 데이터 검증 방어선
        if (trashData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 2. UI 컴포넌트 유실 방어선
        if (itemImage == null || nameText == null || methodText == null || tipText == null)
        {
            Debug.LogError($"[{gameObject.name}] 인스펙터에 TMP 텍스트나 이미지 컴포넌트 연결이 유실되었습니다!");
            return;
        }

        // 3. 현재 선택된 언어 코드 추출
        string currentLanguage = "en";
        try
        {
            if (LocalizationSettings.Instance != null && LocalizationSettings.SelectedLocale != null)
            {
                currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
            }
        }
        catch
        {
            currentLanguage = "ko"; 
        }

        // 4. 해금 여부 판정
        bool isUnlocked = false;
        if (CollectionManager.Instance != null)
        {
            isUnlocked = CollectionManager.Instance.IsUnlocked(trashData.trash_name);
        }

        if (isUnlocked)
        {
            // [Unlocked State] 해금 완료 상태
            if (trashSprite != null)
            {
                itemImage.sprite = trashSprite;
                itemImage.color = Color.white;
            }

            if (currentLanguage == "ko")
            {
                // 로컬라이징 테이블 안전 우회, 주입
                nameText.text = "이름: " + GetSafeLocalizedString("TrashTable", trashData.trash_name + "_NAME", trashData.trash_name);
                methodText.text = "분류 방법: " + GetSafeLocalizedString("TrashTable", trashData.trash_name + "_METHOD", trashData.log_correct);
                tipText.text = "팁: " + GetSafeLocalizedString("TrashTable", trashData.trash_name + "_TIP", trashData.tip_ingame);
            }
            else
            {
                nameText.text = "Name: " + trashData.trash_name;
                methodText.text = "How to recycle: " + trashData.log_correct;
                tipText.text = "Tip: " + trashData.tip_ingame;
            }
        }
        else
        {
            // [Locked State] 잠긴 상태
            if (trashSprite != null)
            {
                itemImage.sprite = trashSprite;
                itemImage.color = new Color(0.1f, 0.1f, 0.1f, 1.0f); // 어두운 실루엣 처리
            }

            if (currentLanguage == "ko")
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

    // 크래시, 런타임 마비 우회하는 동기식 락 프리 함수
    private string GetSafeLocalizedString(string tableName, string key, string fallback)
    {
        try
        {
            // 테이블, 인스턴스 준비되지 않았다면 기획 데이터(fallback) 반환
            if (LocalizationSettings.Instance == null || 
                LocalizationSettings.StringDatabase == null || 
                !LocalizationSettings.InitializationOperation.IsDone)
            {
                return fallback;
            }

            // 비동기 연동 무결성 검증 후 안전하게 문자열 추출
            var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);
            if (operation.IsDone)
            {
                return operation.Result;
            }
            
            return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        }
        catch
        {
            return fallback;
        }
    }
}
