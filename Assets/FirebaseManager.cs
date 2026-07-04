using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;

[System.Serializable]
public class TrashLog
{
    public string trashName;       // 쓰레기 이름 (예: "페트병")
    public string userAnswer;      // 유저가 선택한 분류
    public string correctAnswer;   // 실제 정답
    public bool isCorrect;         // 맞았는지 여부
    public string reason;          // 틀린 이유
}

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    private FirebaseFirestore db;
    private bool isFirebaseReady = false;

    // 이번 판(세션) 동안 쌓인 정/오답 데이터를 모아두는 리스트
    public List<TrashLog> currentSessionLogs = new List<TrashLog>();
    public int gameScore = 0; // 점수 저장용

    void Awake()
    {
        // 씬이 바뀌어도 파이어베이스 매니저가 파괴되지 않고 유지 (싱글톤)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                isFirebaseReady = true;
                Debug.Log("FirebaseManager: 서버 준비 완료!");
            }
            else
            {
                Debug.LogError("Firebase 초기화 실패");
            }
        });
    }

    // 씬에서 정/오답이 발생할 때마다 호출할 함수
    public void AddTrashLog(string name, string userAns, string correctAns, bool isCorrect, string tip)
    {
        TrashLog log = new TrashLog
        {
            trashName = name,
            userAnswer = userAns,
            correctAnswer = correctAns,
            isCorrect = isCorrect,
            reason = tip
        };
        currentSessionLogs.Add(log);
    }

    // 게임오버시 모든 데이터 서버에 통째로 전송
    public void UploadGameResult()
    {
        if (!isFirebaseReady) return;

        // 파이어베이스에 보낼 묶음 데이터 생성
        Dictionary<string, object> resultData = new Dictionary<string, object>
        {
            { "score", gameScore },
            { "playTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        // 오답/정답 리스트를 파이어베이스가 읽을 수 있는 형태로 변환
        List<Dictionary<string, object>> logList = new List<Dictionary<string, object>>();
        foreach (var log in currentSessionLogs)
        {
            var doc = new Dictionary<string, object>
            {
                { "trashName", log.trashName },
                { "userAnswer", log.userAnswer },
                { "correctAnswer", log.correctAnswer },
                { "isCorrect", log.isCorrect },
                { "reason", log.reason }
            };
            logList.Add(doc);
        }
        resultData.Add("logs", logList);

        // 'GameResults' 컬렉션에 데이터 업로드
        db.Collection("GameResults").AddAsync(resultData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("[Firebase] 이번 판의 전체 게임 결과가 성공적으로 서버에 업로드되었습니다!");
            }
        });
    }
}
