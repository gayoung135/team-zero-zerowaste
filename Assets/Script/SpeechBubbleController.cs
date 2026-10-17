using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("[UI Components]")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private ImageChange robotReaction;

    [Header("[인게임 쓰레기 프리팹 등록 리스트]")]
    [SerializeField] private List<GameObject> trashPrefabs = new List<GameObject>();

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (bubbleObject == null)
        {
            bubbleObject = this.gameObject;
        }
    }

    public void ShowBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// 도감 페이지(CollectionPage)의 안전한 로컬라이징 방어선을 이식한 말풍선 출력 함수
    /// </summary>
    public void ShowBubbleWithTip(string message)
    {
        if (tipText == null)
        {
            ShowBubble();
            return;
        }

        string localizedText = message;

        try
        {
            // 1. 현재 선택된 언어 코드 추출 (도감 페이지와 동일한 방식)
            string currentLanguage = "en";
            if (LocalizationSettings.Instance != null && LocalizationSettings.SelectedLocale != null)
            {
                currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;
            }

            if (currentLanguage == "ko")
            {
                // 2. 전달받은 메시지가 이미 정확한 테이블 키인 경우 직공격
                localizedText = GetSafeLocalizedString("TrashTable", message, message);

                // 3. 키를 못 찾았다면 등록된 프리팹 리스트를 대조하여 정확한 키 추출 후 안전 주입
                if (localizedText == message || localizedText.Contains("No translation found"))
                {
                    string targetKey = FindKeyFromPrefabs(message);
                    if (!string.IsNullOrEmpty(targetKey))
                    {
                        localizedText = GetSafeLocalizedString("TrashTable", targetKey, message);
                    }
                }
            }
        }
        catch
        {
            localizedText = message;
        }

        tipText.text = localizedText;
        ShowBubble();
    }

    /// <summary>
    /// 프리팹 리스트 순회 오류(캔이 다른 아이템을 뺏어먹는 현상)를 방지하기 위해 
    /// 글자 수가 길고 고유한 이름부터 정확히 판별하도록 수정한 함수
    /// </summary>
    private string FindKeyFromPrefabs(string message)
    {
        if (string.IsNullOrEmpty(message)) return "";
        string lowerMsg = message.ToLower();

        // 정답/오답 판정 
        bool isWrong = lowerMsg.Contains("not") || lowerMsg.Contains("cannot") || lowerMsg.Contains("aren't") || lowerMsg.Contains("should be emptied");
        string suffix = isWrong ? "_Wrong" : "_Correct";

        // [중요] 캔("can") 같은 짧은 단어가 다른 아이템보다 먼저 걸리는 걸 막기 위해 
        // 이름이 길고 고유한 아이템부터 확실하게 단독 검사합니다.
        
        string targetPrefabName = "";

        if (lowerMsg.Contains("glass") || lowerMsg.Contains("beer"))
        {
            targetPrefabName = "Trash_BrownGlassBottle";
        }
        else if (lowerMsg.Contains("pet") || lowerMsg.Contains("plastic"))
        {
            targetPrefabName = "Trash_PetBottle";
        }
        else if (lowerMsg.Contains("banana"))
        {
            targetPrefabName = "Trash_Banana";
        }
        else if (lowerMsg.Contains("apple"))
        {
            targetPrefabName = "Trash_Apple";
        }
        else if (lowerMsg.Contains("bone"))
        {
            targetPrefabName = "Trash_Bone";
        }
        else if (lowerMsg.Contains("bread") || lowerMsg.Contains("crumb"))
        {
            targetPrefabName = "Trash_Bread";
        }
        else if (lowerMsg.Contains("egg"))
        {
            targetPrefabName = "Trash_Egg";
        }
        else if (lowerMsg.Contains("tissue"))
        {
            targetPrefabName = "Trash_Tissue";
        }
        else if (lowerMsg.Contains("can") || lowerMsg.Contains("cans"))
        {
            targetPrefabName = "Trash_BlueCan";
        }

        // 찾은 고유 이름이 있다면 접미사(_Correct / _Wrong)를 붙여서 반환
        if (!string.IsNullOrEmpty(targetPrefabName))
        {
            return targetPrefabName + suffix;
        }

        return "";
    }

    /// <summary>
    /// 도감 페이지(CollectionPage)에서 검증된 크래시/런타임 마비 우회 함수
    /// </summary>
    private string GetSafeLocalizedString(string tableName, string key, string fallback)
    {
        try
        {
            if (LocalizationSettings.Instance == null || 
                LocalizationSettings.StringDatabase == null || 
                !LocalizationSettings.InitializationOperation.IsDone)
            {
                return fallback;
            }

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

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        HideBubble();
        hideCoroutine = null;
    }

    public void HideBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }

        if (robotReaction != null)
        {
            robotReaction.ResetRobot();
        }
    }
}