using UnityEngine;
using TMPro;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("말풍선 루트 오브젝트 (껐다 켤 대상)")]
    [SerializeField] private GameObject bubbleObject;

    [Header("말풍선 텍스트 (추후 배출법 문구 표시용)")]
    [SerializeField] private TextMeshProUGUI tipText;

    private void Awake()
    {
        // 대상이 지정 안 되어 있다면 자기 자신을 기본으로 사용
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
    /// 말풍선을 숨기는 함수
    /// </summary>
    public void HideBubble()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }
    }
}