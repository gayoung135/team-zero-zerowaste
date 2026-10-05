using UnityEngine;
using TMPro;

public class TrashCan : MonoBehaviour
{
    public GameTimer gametimer;
    [Header("말풍선 컨트롤러")]
    public SpeechBubbleController speechBubbleController;

    [Header("쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag;

    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    void OnTriggerEnter2D(Collider2D other)
    {
        string trashName = other.gameObject.name.Replace("(Clone)", "").Trim();
        bool isCorrect = other.CompareTag(correctTag);

        if (speechBubbleController != null) speechBubbleController.ShowBubble();

        // 1. 해당 쓰레기 오브젝트에서 DB 데이터 가져옴
        TrashGuideData dbData = other.GetComponent<TrashObject>()?.GetMyData();

        // 2. 기본값 설정해 두고, DB 데이터 있다면 해당 데이터로 로그 교체
        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";
        string tipMessage = "";

        if (dbData != null)
        {
            // DB에 저장된 정답/오답 로그 가져옴
            tipMessage = isCorrect ? dbData.log_correct : dbData.log_wrong;
        }
        else
        {
            // 혹시라도 DB 데이터를 찾지 못했을 때를 대비한 예외 처리 문구
            tipMessage = isCorrect ? "올바르게 분리배출 했습니다!" : $"{trashName}은(는) [{correctBinKorean}]에 버려야 합니다.";
            Debug.LogWarning($"{trashName} 오브젝트에서 TrashGuideData를 가져오지 못했습니다. 기본 문구를 사용합니다.");
        }

        // 인게임 로봇 말풍선에는 팁 메시지 출력
        if (speechBubbleController != null) speechBubbleController.ShowBubbleWithTip(dbData?.tip_ingame);

        // 정답 및 오답 점수/타이머 처리
        if (isCorrect) 
        { 
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");
            gametimer.Correct();
        }
        else 
        {
            Debug.Log ($"오답!❌ 잘못된 쓰레기통에 넣었습니다! 점수 -5!");
            gametimer.Incorrect();
        }

        // 서버(UGS)로 최종 업데이트된 tipMessage 전송
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.AddTrashLog(trashName, this.trashCanName, correctBinKorean, isCorrect, tipMessage);
        }

        Destroy(other.gameObject);
    }
}
