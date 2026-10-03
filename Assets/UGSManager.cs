using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

[System.Serializable]
public class TrashLog
{
    public string trashName;
    public string userAnswer;
    public string correctAnswer;
    public bool isCorrect;
    public string reason;
}

public class UGSManager : MonoBehaviour
{
    public static UGSManager Instance { get; private set; }
    
    public bool IsUGSReady { get; private set; } = false;

    [Header("Player Status")]
    public bool isLoggedIn = false;
    public int currentCoins = 0;
    public int gameScore = 0;
    public List<TrashLog> currentSessionLogs = new List<TrashLog>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitUGS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void InitUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();
            IsUGSReady = true;
            Debug.Log("[UGS] 유니티 게이밍 서비스 초기화 완료! 서버 준비 완료.");
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 초기화 중 오류 발생: " + e.Message);
        }
    }

    public void SetLoggedIn(bool state)
    {
        isLoggedIn = state;
    }

    public void EarnCoins(int coins)
    {
        currentCoins += coins;
        SaveUserData();
    }

    public async void SaveUserDataOnRegister(string username)
    {
        try
        {
            var userData = new Dictionary<string, object>
            {
                { "username", username },
                { "coins", 0 }
            };
            await CloudSaveService.Instance.Data.Player.SaveAsync(userData);
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 회원가입 초기 데이터 세이브 실패: " + e.Message);
        }
    }

    public async void SaveUserData()
    {
        if (!IsUGSReady || !isLoggedIn) return;
        try
        {
            var userData = new Dictionary<string, object> { { "coins", currentCoins } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(userData);
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 데이터 세이브 실패: " + e.Message);
        }
    }

    public async void SaveSoldOutItems(List<string> soldOutItemIds)
    {
        if (!IsUGSReady || !isLoggedIn || soldOutItemIds == null) return;
        try
        {
            var targetData = new Dictionary<string, object> { { "sold_out_items", soldOutItemIds } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(targetData);
            Debug.Log("[UGS] 품절 아이템 데이터가 서버에 안전하게 세이브되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 품절 아이템 데이터 세이브 실패: " + e.Message);
        }
    }

    public async void LoadUserData()
    {
        if (!IsUGSReady || !isLoggedIn) return;
        try
        {
            // 한 번에 두 가지 방(coins, sold_out_items) 데이터 요청
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "coins", "sold_out_items" });
            
            // 1. 코인 복원
            if (savedData.TryGetValue("coins", out var coinItem))
            {
                currentCoins = coinItem.Value.GetAs<int>();
                Debug.Log($"서버에서 데이터 로드 완료! 현재 코인: {currentCoins}");
            }

            // 2. 품절 아이템 데이터 복원 및 GameDBManager 동기화 연동
            if (savedData.TryGetValue("sold_out_items", out var itemLog))
            {
                List<string> serverSoldOutList = itemLog.Value.GetAs<List<string>>();
                if (GameDBManager.Instance != null && serverSoldOutList != null)
                {
                    GameDBManager.Instance.SyncInventoryFromServer(serverSoldOutList);
                }
            }
            else
            {
                // 저장된 품절 기록이 아예 없는 신규 유저의 경우 초기화 처리 보장
                if (GameDBManager.Instance != null)
                {
                    GameDBManager.Instance.SyncInventoryFromServer(new List<string>());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("서버 데이터 로드 실패: " + e.Message);
        }
    }

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

    public async void UploadGameResult()
    {
        if (!IsUGSReady) return;
        try
        {
            var resultData = new Dictionary<string, object>
            {
                { "score", gameScore },
                { "playTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            if (isLoggedIn)
            {
                resultData.Add("userId", AuthenticationService.Instance.PlayerId);
            }

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

            await CloudSaveService.Instance.Data.Player.SaveAsync(resultData);
            Debug.Log("[UGS] 이번 판의 전체 게임 결과가 성공적으로 서버에 업로드되었습니다!");
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 게임 결과 업로드 실패: " + e.Message);
        }
    }

    public void SignOut()
    {
        try
        {
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
                AuthenticationService.Instance.ClearSessionToken();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[UGS] SignOut 처리 중 예외 발생: " + e.Message);
        }

        isLoggedIn = false;
        currentCoins = 0;
        gameScore = 0;
        currentSessionLogs.Clear();
        Debug.Log("[UGS] 성공적으로 로그아웃되었으며 유저 데이터가 초기화되었습니다.");
    }
}
