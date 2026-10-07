using UnityEngine;
using System.Collections;
using TMPro;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("말풍선 루트 오브젝트 (껐다 켤 대상)")]
    [SerializeField] private GameObject bubbleObject;

    [Header("말풍선 텍스트 (추후 배출법 문구 표시용)")]
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("말풍선 유지 시간(초)")]
    [SerializeField] private float displayDuration = 2f;

    // 로봇 표정 제어 스크립트 참조
    [Header("로봇 표정 컨트롤러")]
    [SerializeField] private ImageChange robotReaction;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (bubbleObject == null)
        {
            bubbleObject = this.gameObject;
        }
    }

    /// <summary>
    /// 쓰레기가 쓰레기통에 들어갈 때 호출되는 함수
    /// </summary>
    public void ShowBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        // 이미 돌고 있던 2초 타이머가 있다면 취소하고 새로 2초를 잽니다.
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// (추후 팁 문구까지 띄울 때 사용할 확장 함수)
    /// </summary>
    public void ShowBubbleWithTip(string message)
    {
        if (tipText != null)
        {
            tipText.text = message;
        }

        ShowBubble();
    }

    /// <summary>
    /// 정해진 시간(2초) 뒤에 말풍선을 숨기는 코루틴
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        HideBubble();
        hideCoroutine = null;
    }

    /// <summary>
    /// 말풍선을 숨기는 함수
    /// </summary>
    public void HideBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }

        //  말풍선이 꺼질 때 로봇 표정을 기본 표정으로 복구
        if (robotReaction != null)
        {
            robotReaction.ResetRobot();
        }
    }
}