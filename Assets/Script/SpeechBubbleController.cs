using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("[UI Components]")]
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private ImageChange robotReaction;

    private Coroutine hideCoroutine;

    public void ShowBubble()
    {
        this.gameObject.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void ShowBubbleWithTip(string input)
    {
        if (tipText == null)
        {
            ShowBubble();
            return;
        }

        tipText.text = input;
        ShowBubble();
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        HideBubble();
        hideCoroutine = null;
    }

    public void HideBubble()
    {
        this.gameObject.SetActive(false);
        if (robotReaction != null) robotReaction.ResetRobot();
    }
}