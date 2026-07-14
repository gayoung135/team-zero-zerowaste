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

    //  오답 유무와 상관없이 서버 전송용 쓰레기통 이름 받아옴 (예: "재활용 쓰레기통", "일반 쓰레기통")
    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    //  게임 결과 한 번만 서버에 업로드되도록 방어해 주는 안전장치 변수
    private static bool isGameOverUploaded = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // 시간 끝나면 더 이상 점수 계산이 안 되도록 안전장치 추가
        if (limitTime <= 0) return;

        //  부딪힌 쓰레기 오브젝트의 진짜 이름 가져오기 (예: "찌그러진 페트병(Clone)" -> "찌그러진 페트병")
        string trashName = other.gameObject.name.Replace("(Clone)", "").Trim();

        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        bool isCorrect = other.CompareTag(correctTag);

        //  오답일 때 보여줄 메시지
        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";
        string tipMessage = isCorrect ? $"{trashName}은(는) [{correctBinKorean}]에 버려야 합니다." : "올바르게 분리배출 했습니다!";

        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        if (other.CompareTag(correctTag))
        {
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");

            //정답일 때 점수 처리
            score += 10; // 점수 10점 더하기
            scoreNumText.text = score.ToString(); // 화면의 Score_Num에 반영

            if (audioSource != null && correctSound != null)
            {
                audioSource.PlayOneShot(correctSound);
            }
        }
        else
        {
            Debug.Log($"오답!❌ 잘못된 쓰레기통에 넣었습니다! 점수 -5!");

            //오답일 때 점수 처리
            score -= 5; // 점수 5점 감점

            if (score < 0) score = 0; // 점수가 0 미만으로 떨어지면 강제로 0으로 고정

            scoreNumText.text = score.ToString(); // 화면의 Score_Num에 반영

            if (audioSource != null && incorrectSound != null)
            {
                audioSource.PlayOneShot(incorrectSound);
            }
        }

        //  정/오답 데이터를 FirebaseManager의 임시 리스트에 쌓아둠
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.AddTrashLog(trashName, this.trashCanName, correctBinKorean, isCorrect, tipMessage);
        }

        // 판정이 끝난 쓰레기는 화면에서 삭제
        Destroy(other.gameObject);
    }

    void Start()
    {
        //0으로 멈췄던 게임 시간을 다시 1배속으로 흐르게 합니다.
        Time.timeScale = 1f;

        //게임이 Replay로 다시 시작될 때 기존 점수를 0으로 깨끗하게 리셋합니다.
        score = 0;
        scoreNumText.text = score.ToString();

        //  다시 시작할 때 데이터 초기화 및 업로드 방어막 해제
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

            //  0초가 되면 보라색 프레임 안의 최종 점수 텍스트를 먼저 채워줍니다!
            if (totalScoreText != null)
            {
                totalScoreText.text = score.ToString();
                //totalScoreText.text = $"SCORE: {score}"; 
                // 숫자만 깔끔하게 띄우고 싶다면 totalScoreText.text = score.ToString(); 로 하셔도 됩니다.
            }

            if (logScoreText != null)
            {
                logScoreText.text = score.ToString(); // 로그창 점수도 똑같이 동기화!
            }


            if (logContentText != null && FirebaseManager.Instance != null)
            {
                logContentText.text = ""; // 1. 처음에 적혀있던 임의 테스트용 텍스트를 깨끗하게 비웁니다.

                // 2. Firebase 리스트 주머니를 열어서 쌓여있는 기록을 하나씩 순서대로 꺼냅니다.
                foreach (var log in FirebaseManager.Instance.currentSessionLogs)
                {
                    // 3. 맞았으면 [정답], 틀렸으면 [오답] 기호를 달아줍니다.
                    string resultIcon = log.isCorrect ? "[정답]" : "[오답]";
                    
                    // 4. 문장 모양을 이쁘게 만들어서 화면 텍스트에 한 줄씩 차곡차곡 더해줍니다 (\n은 줄바꿈)
                    logContentText.text += $"[{log.trashName}] {log.userAnswer} 선택 -> {resultIcon}\n";
                }
            }

            //0초가 되면 숨겨져 있던 결과창 캔버스를 화면에 탁 켭니다!
            if (gameOverCanvas != null)
            {
                gameOverCanvas.SetActive(true);
            }

            //  0초되는 순간 1번만 Firebase Manager에 이번 판 점수 넘겨 최종 업로드 실행
            if (!isGameOverUploaded)
            {
                isGameOverUploaded = true;
                if (FirebaseManager.Instance != null)
                {
                    FirebaseManager.Instance.gameScore = score; // 최종 점수 동기화
                    FirebaseManager.Instance.UploadGameResult(); // 서버로 전송!
                }
            }
            
            Time.timeScale = 0f; // 시간이 0초가 되면 게임 일시정지 시킵니다.
        }
    }
}