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

    public List<PlacedItemData> currentLayout { get; private set; } = new List<PlacedItemData>();

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

    public async void SaveTrashUnlocks()
    {
        if (!IsUGSReady || !isLoggedIn || GameDBManager.Instance == null) return;

        try
        {
            List<string> unlockedIds = new List<string>();

            foreach (TrashGuideData data in GameDBManager.Instance.trashGuideMasterDB)
            {
                if (data.is_unlock) unlockedIds.Add(data.id);
            }

            var saveData = new Dictionary<string, object>
            {
                { "unlocked_trash_ids", unlockedIds }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);

            Debug.Log($"[UGS] 도감 해금 상태 저장 완료: {unlockedIds.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 도감 해금 상태 저장 실패: " + e.Message);
        }
    }

    public async void SaveTerrariumLayout(List<PlacedItemData> layout)
    {
        if (!IsUGSReady || !isLoggedIn || layout == null) return;
        try
        {
            currentLayout = layout;
            var targetData = new Dictionary<string, object> { { "terrarium_layout", layout } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(targetData);
            Debug.Log("[UGS] 테라리움 필드 배치 상태가 서버에 영구 저장되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError("[UGS] 테라리움 배치 저장 실패: " + e.Message);
        }
    }

    public async void LoadUserData()
    {
        if (!IsUGSReady || !isLoggedIn) return;
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "coins", "sold_out_items", "terrarium_layout", "unlocked_trash_ids" });
            
            if (savedData.TryGetValue("coins", out var coinItem))
            {
                currentCoins = coinItem.Value.GetAs<int>();
                Debug.Log($"서버에서 데이터 로드 완료! 현재 코인: {currentCoins}");
            }

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
                if (GameDBManager.Instance != null) GameDBManager.Instance.SyncInventoryFromServer(new List<string>());
            }

            if (GameDBManager.Instance != null)
            {


                if (savedData.TryGetValue("unlocked_trash_ids", out var trashLog))
                {
                    List<string> unlockedIds = trashLog.Value.GetAs<List<string>>();

                    if (unlockedIds != null)
                    {
                        foreach (string id in unlockedIds)
                        {
                            TrashGuideData data = GameDBManager.Instance.GetTrashData(id);
                            if (data != null) data.is_unlock = true;
                        }
                    }
                }

                Debug.Log("[UGS] 도감 해금 상태 불러오기 완료");
            }

            if (savedData.TryGetValue("terrarium_layout", out var layoutLog))
            {
                currentLayout = layoutLog.Value.GetAs<List<PlacedItemData>>();
                
                EditMode editManager = FindObjectOfType<EditMode>();
                if (editManager != null && currentLayout != null && currentLayout.Count > 0)
                {
                    editManager.SpawnLayout(currentLayout);
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
        currentLayout.Clear(); 
        Debug.Log("[UGS] 성공적으로 로그아웃되었으며 유저 데이터가 초기화되었습니다.");
    }
}
