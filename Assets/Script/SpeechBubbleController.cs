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

    private Coroutine hideCoroutine;

    // 이현 님이 지정하신 정확한 텍스트 데이터 딕셔너리
    private static readonly Dictionary<string, (string ko, string en)> localizedDict = new Dictionary<string, (string ko, string en)>()
    {
        { "Trash_PetBottle_Wrong", ("페트병은 일반 쓰레기가 아니며, 재활용 플라스틱으로 분류됩니다.", "PET bottles are not general waste, they are classified as recyclable plastics.") },
        { "Trash_PetBottle_Correct", ("페트병은 재활용(플라스틱) 쓰레기가 맞습니다.", "PET bottles are recyclable waste.") },
        { "Trash_Apple_Wrong", ("사과는 재활용 쓰레기가 아니며, 음식물 쓰레기로 분류됩니다.", "Apples are not recyclable waste, they are classified as food waste.") },
        { "Trash_Apple_Correct", ("사과는 음식물 쓰레기가 맞습니다.", "Apples are classified as food waste.") },
        { "Trash_Bone_Wrong", ("뼈는 재활용할 수 없으며 일반쓰레기로 분류됩니다.", "Bones cannot be recycled and are classified as general waste.") },
        { "Trash_Bone_Correct", ("뼈다귀는 일반쓰레기가 맞습니다.", "Bones are classified as general waste.") },
        { "Trash_Banana_Wrong", ("바나나 껍질은 재활용 쓰레기가 아니라 음식물 쓰레기입니다.", "Banana peels are food waste, not recyclable waste.") },
        { "Trash_Banana_Correct", ("바나나 껍질은 음식물 쓰레기가 맞습니다.", "Banana peels are classified as food waste.") },
        { "Trash_BlueCan_Wrong", ("캔은 일반쓰레기가 아니며,  재활용 과정에서 캔류로 분류됩니다.", "Cans are not general waste, they are separated into a distinct category for cans during the recycling process.") },
        { "Trash_BlueCan_Correct", ("캔은 재활용(캔류) 쓰레기가 맞습니다.", "Cans are classified as 'can waste' during the recycling process.") },
        { "Trash_Bread_Wrong", ("빵 조각은 재활용 쓰레기가 아니며, 음식물 쓰레기로 분류됩니다.", "Bread scraps are not recyclable waste, they are classified as food waste.") },
        { "Trash_Bread_Correct", ("빵 조각은 음식물 쓰레기가 맞습니다.", "Breadcrumbs are classified as food waste.") },
        { "Trash_BrownGlassBottle_Wrong", ("유리병은 일반쓰레기가 아니며, 유리류 재활용으로 분류됩니다.", "Glass bottles are not general waste, they are classified as glass for recycling.") },
        { "Trash_BrownGlassBottle_Correct", ("빈 맥주병은 재활용(유리병) 쓰레기가 맞습니다.", "Empty beer bottles are classified as glass waste for recycling.") },
        { "Trash_Egg_Wrong", ("달걀 껍데기는 재활용할 수 없으며 일반쓰레기로 분류됩니다.", "Eggshells cannot be recycled and are classified as general waste.") },
        { "Trash_Egg_Correct", ("달걀 껍질은 일반쓰레기가 맞습니다.", "Eggshells are classified as general waste.") },
        { "Trash_Tissue_Wrong", ("휴지는 재활용할 수 없으며 일반쓰레기로 분류됩니다.", "Tissues cannot be recycled and are classified as general waste.") },
        { "Trash_Tissue_Correct", ("사용한 휴지는 일반쓰레기가 맞습니다.", "Used tissues are classified as general waste.") }
    };

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

    public void ShowBubbleWithTip(string exactKeyOrMessage)
    {
        ProcessAndShow(exactKeyOrMessage);
    }

    public void ShowBubbleWithKey(string exactKey)
    {
        ProcessAndShow(exactKey);
    }

    private void ProcessAndShow(string input)
    {
        if (tipText == null)
        {
            ShowBubble();
            return;
        }

        // 1. 현재 선택된 언어 확인 (기본값 "ko")
        string currentLang = "ko";
        try
        {
            if (LocalizationSettings.Instance != null && LocalizationSettings.SelectedLocale != null)
            {
                currentLang = LocalizationSettings.SelectedLocale.Identifier.Code;
            }
        }
        catch { }

        // 2. 외부 문장(로그에 찍히는 메시지)을 키값으로 파싱
        string parsedKey = ParseInputToKey(input);
        string finalDisplayString = input;

        // 3. 파싱된 키로 딕셔너리에서 한국어/영어 문장 가져오기
        if (!string.IsNullOrEmpty(parsedKey) && localizedDict.ContainsKey(parsedKey))
        {
            var textPair = localizedDict[parsedKey];
            finalDisplayString = (currentLang == "en") ? textPair.en : textPair.ko;
        }

        tipText.text = finalDisplayString;
        ShowBubble();
    }

    /// <summary>
    /// 외부에서 들어온 문장이나 키를 딕셔너리 키로 변환해주는 파서
    /// </summary>
    private string ParseInputToKey(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        // 이미 완벽한 키 형태라면 그대로 반환
        if (localizedDict.ContainsKey(input)) return input;

        string lower = input.ToLower();

        // 쓰레기 종류 특정
        string itemPrefix = "";
        if (lower.Contains("glass") || lower.Contains("beer") || lower.Contains("유리") || lower.Contains("맥주")) itemPrefix = "Trash_BrownGlassBottle";
        else if (lower.Contains("pet") || lower.Contains("plastic") || lower.Contains("페트") || lower.Contains("플라스틱")) itemPrefix = "Trash_PetBottle";
        else if (lower.Contains("banana") || lower.Contains("바나나")) itemPrefix = "Trash_Banana";
        else if (lower.Contains("apple") || lower.Contains("사과")) itemPrefix = "Trash_Apple";
        else if (lower.Contains("bone") || lower.Contains("뼈")) itemPrefix = "Trash_Bone";
        else if (lower.Contains("bread") || lower.Contains("crumb") || lower.Contains("빵")) itemPrefix = "Trash_Bread";
        else if (lower.Contains("egg") || lower.Contains("달걀") || lower.Contains("계란")) itemPrefix = "Trash_Egg";
        else if (lower.Contains("tissue") || lower.Contains("휴지") || lower.Contains("티슈")) itemPrefix = "Trash_Tissue";
        else if (lower.Contains("can") || lower.Contains("cans") || lower.Contains("캔")) itemPrefix = "Trash_BlueCan";

        if (string.IsNullOrEmpty(itemPrefix)) return "";

        // 정답 / 오답 구분 (콘솔 로그에 찍힌 -5, 오답, 잘못된 등 기준)
        bool isWrong = lower.Contains("-5") || lower.Contains("오답") || lower.Contains("잘못된") || lower.Contains("not") || lower.Contains("cannot");
        
        return itemPrefix + (isWrong ? "_Wrong" : "_Correct");
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