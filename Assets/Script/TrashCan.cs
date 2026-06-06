using UnityEngine;
using TMPro;

public class TrashCan : MonoBehaviour
{

    // [1] 유니티 에디터의 Score_Num과 Timer_Num을 드래그앤드롭 할 구멍입니다.
    public TextMeshProUGUI scoreNumText; 
    public TextMeshProUGUI timerNumText;

    // [2] 실제 점수와 남은 시간을 계산해서 저장할 데이터 주머니입니다.
    private static int score = 0;
    private float limitTime = 60f; // 1분 = 60초

    [Header("이 쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag; 

    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        if (other.CompareTag(correctTag))
        {
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");

            //정답일 때 점수 처리
            score += 10; // 점수 10점 더하기
            scoreNumText.text = score.ToString(); // 화면의 Score_Num에 반영
        }
        else
        {
            Debug.Log($"오답!❌ 잘못된 쓰레기통에 넣었습니다! 점수 -5!");

            //오답일 때 점수 처리
            score -= 5; // 점수 5점 감점
            scoreNumText.text = score.ToString(); // 화면의 Score_Num에 반영
        }

        // 판정이 끝난 쓰레기는 화면에서 삭제
        Destroy(other.gameObject);
    }

    void Start()
    {
        scoreNumText.text = score.ToString();
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

            Time.timeScale = 0f; // 시간이 0초가 되면 게임 일시정지 시킵니다.
        }
    }
}