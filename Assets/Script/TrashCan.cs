using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class TrashCan : MonoBehaviour
{
    public GameTimer gametimer;
    [Header("쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag;

    [Header("이 쓰레기통의 이름 입력")]
    public string trashCanName;

    string BubbleMessage;

    void OnTriggerEnter2D(Collider2D other)
    {
        bool isCorrect = other.CompareTag(correctTag);

        // 1. 해당 쓰레기 오브젝트에서 DB 가져옴
        TrashGuideData ItemDB = other.GetComponent<TrashObject>().item;

        // 2. 기본값 설정해 두고, DB 데이터 있다면 해당 데이터로 로그 교체
        string correctBinKorean = (correctTag == "Recyclable") ? "재활용 쓰레기통" : "일반 쓰레기통";

        // 언어별 DB에 저장된 정답/오답 로그 가져옴
        if (LocalizationSettings.SelectedLocale.Identifier.Code == "ko") BubbleMessage = isCorrect ? ItemDB.correct_KR : ItemDB.wrong_KR;
        else BubbleMessage = isCorrect ? ItemDB.correct : ItemDB.wrong;

        // 정답 및 오답 점수/타이머 처리
        if (isCorrect) 
        { 
            Debug.Log($"정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +10!");
            gametimer.Correct(BubbleMessage);

            // 도감 해금 로직
            TrashGuideData data = GameDBManager.Instance.GetTrashData(ItemDB.id);
            if (data != null) data.is_unlock = true;
            if (UGSManager.Instance != null) UGSManager.Instance.SaveTrashUnlocks();
        }
        else 
        {
            Debug.Log ($"오답!❌ 잘못된 쓰레기통에 넣었습니다! 점수 -5!");
            gametimer.Incorrect(BubbleMessage);
        }

        // 서버(UGS)로 최종 업데이트된 로그 전송
        if (UGSManager.Instance != null) UGSManager.Instance.AddTrashLog(other.name, this.trashCanName, correctBinKorean, isCorrect, BubbleMessage);

        Destroy(other.gameObject);
    }
}
