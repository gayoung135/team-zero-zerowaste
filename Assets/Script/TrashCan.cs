using UnityEngine;
using TMPro;

public class TrashCan : MonoBehaviour
{
    // [1] 유니티 에디터의 Score_Num과 Timer_Num을 드래그앤드롭 할 구멍입니다.
    public TextMeshProUGUI scoreNumText;
    public TextMeshProUGUI timerNumText;

    // 보라색 프레임(HandFrame) 안에 넣은 텍스트를 연결할 구멍입니다.
    [Header("게임 오버 창의 최종 점수 텍스트")]
    public TextMeshProUGUI totalScoreText;

    [Header("게임 오버 창의 얻은 재화(코인) 텍스트")]
    public TextMeshProUGUI totalCoinText;

    [Header("상세 로그 창의 점수 텍스트")]
    public TextMeshProUGUI logScoreText;

    [Header("상세 로그 창의 실제 기록(내용) 텍스트")]
    public TextMeshProUGUI logContentText;

    // 로봇 표정 제어 스크립트 연결 창
    [Header("로봇 표정 컨트롤러")]
    public RobotReactionController robotReaction;

    // 유니티 에디터에서 GameOver_Canvas를 연결할 구멍입니다.
    public GameObject gameOverCanvas;

    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    // [2] 실제 점수와 남은 시간을 계산해서 저장할 데이터 주머니입니다.
    private static int score = 0;
    private float limitTime = 30f; // 30초

    [Header("이 쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag;

    // 오답 유무와 상관없이 서버 전송용 쓰레기통 이름 받아옴
    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    // 게임 결과 한 번만 서버에 업로드되도록 방어해 주는 안전장치 변수
    private static bool isGameOverUploaded = false;

    // [팀장님 요구사항] 점수 기반 코인 계산 함수 (점수 x 0.5, 0점 이하 시 참여금 5코인)
    private int CalculateCoins(int finalScore)
    {
        if (finalScore <= 0)
        {
            return 5; // 0점 이하 참여금 5코인
        }

        return (int)(finalScore * 0.5f); // 소수점 버림 자동 적용
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (limitTime <= 0) return;

        string trashName = other.gameObject.name.Replace("(Clone)", "").Trim();
        bool isCorrect = other.CompareTag(correctTag);

        // 로봇 표정 변경 알림
        if (robotReaction != null)
        {
            robotReaction.OnAnswerResult(isCorrect);
        }

        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";
        string tipMessage = isCorrect ? $"{trashName}은(는) [{correctBinKorean}]에 버려야 합니다." : "올바르게 분리배출 했습니다!";

        if (isCorrect)
        {
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");
            score += 10;
            scoreNumText.text = score.ToString();

            if (audioSource != null && correctSound != null)
            {
                audioSource.PlayOneShot(correctSound);
            }
        }
        else
        {
            Debug.Log($"오답!❌ 잘못된 쓰레기통에 넣었습니다! 점수 -5!");
            score -= 5;
            if (score < 0) score = 0;

            scoreNumText.text = score.ToString();

            if (audioSource != null && incorrectSound != null)
            {
                audioSource.PlayOneShot(incorrectSound);
            }
        }

        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.AddTrashLog(trashName, this.trashCanName, correctBinKorean, isCorrect, tipMessage);
        }

        Destroy(other.gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

        score = 0;
        scoreNumText.text = score.ToString();

        if (robotReaction != null)
        {
            robotReaction.ResetReaction();
        }

        isGameOverUploaded = false;
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.currentSessionLogs.Clear();
            UGSManager.Instance.gameScore = 0;
        }
    }

    void Update()
    {
        if (limitTime > 0)
        {
            limitTime -= Time.deltaTime;
            timerNumText.text = Mathf.Max(0, Mathf.FloorToInt(limitTime)).ToString();
        }
        else
        {
            timerNumText.text = "0";

            if (totalScoreText != null) totalScoreText.text = score.ToString();
            if (logScoreText != null) logScoreText.text = score.ToString();

            if (logContentText != null && UGSManager.Instance != null)
            {
                logContentText.text = "";
                foreach (var log in UGSManager.Instance.currentSessionLogs)
                {
                    string resultIcon = log.isCorrect ? "[정답]" : "[오답]";
                    logContentText.text += $"[{log.trashName}] {log.userAnswer} 선택 -> {resultIcon}\n";
                }
            }

            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(true);
            }

            // 게임 결과 및 코인 데이터 처리
            ProcessGameOverData();

            Time.timeScale = 0f;
        }
    }

    // 설정창에서 '그만하기' 눌렀을 때 호출될 함수
    public void QuitGameAndSaveScore()
    {
        if (totalScoreText != null) totalScoreText.text = score.ToString();
        if (logScoreText != null) logScoreText.text = score.ToString();

        if (logContentText != null && UGSManager.Instance != null)
        {
            logContentText.text = "";
            foreach (var log in UGSManager.Instance.currentSessionLogs)
            {
                string resultIcon = log.isCorrect ? "[정답]" : "[오답]";
                logContentText.text += $"[{log.trashName}] {log.userAnswer} 선택 -> {resultIcon}\n";
            }
        }

        // 게임 결과 및 코인 데이터 처리
        ProcessGameOverData();

        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        Time.timeScale = 0f;
    }

    // 게임오버 시 코인 계산, UI 반영 및 UGS 업로드를 통합 처리하는 전용 함수
    private void ProcessGameOverData()
    {
        if (!isGameOverUploaded)
        {
            isGameOverUploaded = true;

            // 1. 점수 기반 획득 코인 계산
            int earnedCoins = CalculateCoins(score);

            // 2. UI 표시 (얻은재화: 숫자 형식)
            if (totalCoinText != null)
            {
                totalCoinText.text = $"얻은재화: {earnedCoins}";
            }

            // 3. UGS 서버에 코인 누적 세이브 및 플레이 결과 업로드
            if (UGSManager.Instance != null)
            {
                UGSManager.Instance.gameScore = score;
                UGSManager.Instance.EarnCoins(earnedCoins);
                UGSManager.Instance.UploadGameResult();
            }
        }
    }
}