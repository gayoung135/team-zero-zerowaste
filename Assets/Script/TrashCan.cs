using UnityEngine;
using TMPro;

public class TrashCan : MonoBehaviour
{
    public GameTimer gametimer;
    // 말풍선 제어 스크립트 연결 창
    [Header("말풍선 컨트롤러")]
    public SpeechBubbleController speechBubbleController;

    [Header("쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag;

    // 오답 유무와 상관없이 서버 전송용 쓰레기통 이름 받아옴
    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    void OnTriggerEnter2D(Collider2D other)
    {
        string trashName = other.gameObject.name.Replace("(Clone)", "").Trim();
        bool isCorrect = other.CompareTag(correctTag);

        // 쓰레기 투입 시 로봇 말풍선 활성화
        if (speechBubbleController != null) speechBubbleController.ShowBubble();

        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";
        string tipMessage = isCorrect ? $"{trashName}은(는) [{correctBinKorean}]에 버려야 합니다." : "올바르게 분리배출 했습니다!";

        TrashGuideData dbData = other.GetComponent<TrashObject>()?.GetMyData();  //추가
        if (speechBubbleController != null) speechBubbleController.ShowBubbleWithTip(dbData?.tip_ingame); //추가

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

        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.AddTrashLog(trashName, this.trashCanName, correctBinKorean, isCorrect, tipMessage);
        }

        Destroy(other.gameObject);
    }
}