using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // 유니티 에디터의 Score_Num과 Timer_Num을 드래그앤드롭 할 구멍입니다.
    public TextMeshProUGUI scoreNumText;
    public TextMeshProUGUI timerNumText;

    public GameObject gameOverCanvas;
    public AudioSource audioSource;
    public AudioClip GameoverSound;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    

    // 보라색 프레임(HandFrame) 안에 넣은 텍스트를 연결할 구멍입니다.
    [Header("게임 오버 창의 텍스트")]
    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI totalCoinText;

    [Header("상세 로그 창의 텍스트")]
    public TextMeshProUGUI logScoreText;
    public TextMeshProUGUI logContentText;

    [Header("로봇 표정 컨트롤러")]
    public ImageChange robotReaction;

    // 실제 점수와 남은 시간을 계산해서 저장할 데이터 주머니입니다.
    private static int score = 0;
    private float limitTime = 30f; // 30초
    int combo = 0; //콤보계산용

    // 게임 결과 한 번만 서버에 업로드되도록 방어해 주는 안전장치 변수
    private static bool isGameOverUploaded = false;

    void Start()
    {
        Time.timeScale = 1f;

        score = 0;
        combo = 0;
        scoreNumText.text = score.ToString();

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
                    logContentText.text += $"{log.reason}\n\n";
                }
            }

            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(true);
                audioSource.Stop();
                audioSource.PlayOneShot(GameoverSound); //게임오버소리 켜기
            }

            // 게임 결과 및 코인 데이터 처리
            ProcessGameOverData();

            Time.timeScale = 0f;
        }
    }

    public void Correct() //정답 신호
    {
        score += 10;
        scoreNumText.text = score.ToString();
        audioSource.PlayOneShot(correctSound);

        if(combo<0) combo = 0;
        combo+=1;
        robotReaction.RobotChange(combo);
    }

    public void Incorrect()
    {
        score -= 5;
        if (score < 0) score = 0;
        scoreNumText.text = score.ToString();
        audioSource.PlayOneShot(incorrectSound);

        if(combo>0) combo = 0;
        combo-=1;
        robotReaction.RobotChange(combo);
    }

    // 설정창에서 '그만하기' 눌렀을 때 호출될 함수
    public void QuitGameAndSaveScore()
    {
        if (totalScoreText != null)
            totalScoreText.text = score.ToString();

        if (logScoreText != null)
            logScoreText.text = score.ToString();

    // 로그 내용 표시
        if (logContentText != null && UGSManager.Instance != null)
        {
            logContentText.text = "";

            foreach (var log in UGSManager.Instance.currentSessionLogs)
            {
                // DB에서 받아온 최신 로그 내용 사용
                logContentText.text += $"{log.reason}\n\n";
            }
        }

        audioSource.Stop();
        audioSource.PlayOneShot(GameoverSound);

    // 게임 중단이므로 기본 재화 5 지급
        ProcessGameOverData(true);

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Time.timeScale = 0f;
    }

    // 게임오버 시 코인 계산, UI 반영 및 UGS 업로드를 통합 처리하는 전용 함수
    private void ProcessGameOverData(bool isQuit = false)
    {
        if (!isGameOverUploaded)
        {
            isGameOverUploaded = true;

            int earnedCoins;

            // 게임 중단 시에는 무조건 기본 재화 5
            if (isQuit)
            {
                earnedCoins = 5;
            }
            // 정상 게임오버
            else
            {
                // 점수 기반 획득 코인 계산
                if (score <= 0)
                    earnedCoins = 5;
                else
                    earnedCoins = (int)(score * 0.5f);
            }

        // UI 표시
            if (totalCoinText != null)
                totalCoinText.text = $"얻은재화: {earnedCoins}";

        // UGS 서버에 코인 누적 세이브 및 플레이 결과 업로드
            if (UGSManager.Instance != null)
            {
                UGSManager.Instance.gameScore = score;
                UGSManager.Instance.EarnCoins(earnedCoins);
                UGSManager.Instance.UploadGameResult();
            }
        }
    }
}
