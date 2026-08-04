using UnityEngine;
using UnityEngine.UI;

public enum RobotExpression
{
    Normal,         // 기본 표정
    Correct,        // 정답 (미소)
    ComboCorrect,   // 연속 정답 (감탄/반짝임)
    Wrong,          // 오답 (당황)
    ComboWrong      // 연속 오답 (화남)
}

public class RobotReactionController : MonoBehaviour
{
    [Header("로봇 랜더러")]
    [SerializeField] private Image robotImage;             // UI용
    [SerializeField] private SpriteRenderer robotRenderer; // 2D Sprite용

    [Header("로봇 표정 스프라이트 5종")]
    [SerializeField] private Sprite normalSprite;       // 기본
    [SerializeField] private Sprite correctSprite;      // 정답
    [SerializeField] private Sprite comboCorrectSprite; // 연속 정답
    [SerializeField] private Sprite wrongSprite;        // 오답
    [SerializeField] private Sprite comboWrongSprite;   // 연속 오답

    private int currentCombo = 0; // 양수: 정답 콤보, 음수: 오답 콤보

    private void Awake()
    {
        // 자동으로 컴포넌트 감지
        if (robotImage == null) robotImage = GetComponent<Image>();
        if (robotRenderer == null) robotRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnAnswerResult(bool isCorrect)
    {
        if (isCorrect)
        {
            currentCombo = (currentCombo < 0) ? 1 : currentCombo + 1;
            
            if (currentCombo >= 2)
                SetExpression(RobotExpression.ComboCorrect);
            else
                SetExpression(RobotExpression.Correct);
        }
        else
        {
            currentCombo = (currentCombo > 0) ? -1 : currentCombo - 1;

            if (currentCombo <= -2)
                SetExpression(RobotExpression.ComboWrong);
            else
                SetExpression(RobotExpression.Wrong);
        }
    }

    private void SetExpression(RobotExpression expression)
    {
        Sprite targetSprite = normalSprite;

        switch (expression)
        {
            case RobotExpression.Normal: targetSprite = normalSprite; break;
            case RobotExpression.Correct: targetSprite = correctSprite; break;
            case RobotExpression.ComboCorrect: targetSprite = comboCorrectSprite; break;
            case RobotExpression.Wrong: targetSprite = wrongSprite; break;
            case RobotExpression.ComboWrong: targetSprite = comboWrongSprite; break;
        }

        // UI Image와 SpriteRenderer 둘 다 지원
        if (robotImage != null) robotImage.sprite = targetSprite;
        if (robotRenderer != null) robotRenderer.sprite = targetSprite;
    }

    public void ResetReaction()
    {
        currentCombo = 0;
        SetExpression(RobotExpression.Normal);
    }
}