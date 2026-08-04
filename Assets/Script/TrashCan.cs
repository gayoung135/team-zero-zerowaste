using UnityEngine;
using TMPro;

public class TrashCan : MonoBehaviour
{
    // [1] 유니티 에디터의 Score_Num과 Timer_Num을 드래그앤드롭 할 구멍입니다.
    public TextMeshProUGUI scoreNumText;
    public TextMeshProUGUI timerNumText;

    // 보라색 프레임(HandFrame) 안에 넣은 텍스트를 연결할 구멍을 새로 만듭니다!
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

    // 유니티 에디터에서 GameOver_Canvas를 연결할 구멍입니다. (스펠링 정확히 매칭)
    public GameObject gameOverCanvas;

    [Header("사운드 설정")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    // [2] 실제 점수와 남은 시간을 계산해서 저장할 데이터 주머니입니다.
    private static int score = 0;
    private float limitTime = 30f; // 1분 = 60초 -> 60f

    [Header("이 쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag;

    // 오답 유무와 상관없이 서버 전송용 쓰레기통 이름 받아옴 (예: "재활용 쓰레기통", "일반 쓰레기통")
    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    // 게임 결과 한 번만 서버에 업로드되도록 방어해 주는 안전장치 변수
    private static bool isGameOverUploaded = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 시간 끝나면 더 이상 점수 계산이 안 되도록 안전장치 추가
        if (limitTime <= 0) return;

        // 부딪힌 쓰레기 오브젝트의 진짜 이름 가져오기
        string trashName = other.gameObject.name.Replace("(Clone)", "").Trim();

        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        bool isCorrect = other.CompareTag(correctTag);

        // 로봇 표정 변경 알림!
        if (robotReaction != null)
        {
            robotReaction.OnAnswerResult(isCorrect);
        }

        // 오답일 때 보여줄 메시지
        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";
        string tipMessage = isCorrect ? $"{trashName}은(는) [{correctBinKorean}]에 버려야 합니다." : "올바르게 분리배출 했습니다!";

        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        if (isCorrect)
        {
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");

            // 정답일 때 점수 처리
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

            // 오답일 때 점수 처리
            score -= 5;
            if (score < 0) score = 0;

            scoreNumText.text = score.ToString();

            if (audioSource != null && incorrectSound != null)
            {
                audioSource.PlayOneShot(incorrectSound);
            }
        }

        // 정/오답 데이터를 FirebaseManager의 임시 리스트에 쌓아둠
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.AddTrashLog(trashName, this.trashCanName, correctBinKorean, isCorrect, tipMessage);
        }

        // 판정이 끝난 쓰레기는 화면에서 삭제
        Destroy(other.gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;

        score = 0;
        scoreNumText.text = score.ToString();

        // 게임 시작 시 로봇 표정 초기화
        if (robotReaction != null)
        {
            robotReaction.ResetReaction();
        }

        isGameOverUploaded = false;
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.currentSessionLogs.Clear();
            FirebaseManager.Instance.gameScore = 0;
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

            if (totalScoreText != null)
            {
                totalScoreText.text = score.ToString();
            }

            if (logScoreText != null)
            {
                logScoreText.text = score.ToString();
            }

            if (logContentText != null && FirebaseManager.Instance != null)
            {
                logContentText.text = "";

                foreach (var log in FirebaseManager.Instance.currentSessionLogs)
                {
                    string resultIcon = log.isCorrect ? "[정답]" : "[오답]";
                    logContentText.text += $"[{log.trashName}] {log.userAnswer} 선택 -> {resultIcon}\n";
                }
            }

            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(true);
            }

            if (!isGameOverUploaded)
            {
                isGameOverUploaded = true;
                if (FirebaseManager.Instance != null)
                {
                    FirebaseManager.Instance.gameScore = score;
                    FirebaseManager.Instance.UploadGameResult();
                }
            }
            
            Time.timeScale = 0f;
        }
    }

    // [추가] 설정창에서 '그만하기' 눌렀을 때 호출될 점수 저장 및 게임오버 처리 함수
    public void QuitGameAndSaveScore()
    {
        // 1. 현재 점수를 최종 점수 및 상세 로그 점수 텍스트에 전달
        if (totalScoreText != null) totalScoreText.text = score.ToString();
        if (logScoreText != null) logScoreText.text = score.ToString();

        // 2. 상세 로그 내용 갱신
        if (logContentText != null && FirebaseManager.Instance != null)
        {
            logContentText.text = "";
            foreach (var log in FirebaseManager.Instance.currentSessionLogs)
            {
                string resultIcon = log.isCorrect ? "[정답]" : "[오답]";
                logContentText.text += $"[{log.trashName}] {log.userAnswer} 선택 -> {resultIcon}\n";
            }
        }

        // 3. 서버/파이어베이스 업로드
        if (!isGameOverUploaded)
        {
            isGameOverUploaded = true;
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.gameScore = score;
                FirebaseManager.Instance.UploadGameResult();
            }
        }

        // 4. 게임 오버 창을 켜고 시간 정지
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        Time.timeScale = 0f;
    }
}