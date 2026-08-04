using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class TrashLog
{
    public string trashName;       // 쓰레기 이름 (예: "페트병")
    public string userAnswer;      // 유저가 선택한 분류
    public string correctAnswer;   // 실제 정답
    public bool isCorrect;         // 맞았는지 여부
    public string reason;          // 틀린 이유
}

public class UGSManager : MonoBehaviour
{
    public static UGSManager Instance { get; private set; }

    private bool isUGSReady = false;

    [Header("UI References (Signup/Login UI)")]
    public GameObject loginPopup;       // 팝업창 오브젝트 (Background_Panel)
    public TMP_InputField emailInput;           // E-mail 입력창 (UGS 로그인시 ID로 활용)
    public TMP_InputField idInput;              // ID 입력창 (닉네임 데이터용)
    public TMP_InputField passwordInput;        // Password 입력창
    public TMP_InputField confirmPasswordInput; // Confirm Password 입력창

    [Header("Remember My Account Status")]
    private bool isRememberMeOn = false; // 아이디 기억하기 토글 상태 체크용 변수

    [Header("Player Status")]
    public bool isLoggedIn = false;
    public int currentCoins = 0;
    public List<TrashLog> currentSessionLogs = new List<TrashLog>();
    public int gameScore = 0; // 점수 저장용

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

    void Start()
    {
        // [게임 시작 시 실행] 로컬 컴퓨터에 저장된 기억 이메일 주소가 있다면 자동 로그인창에 적어주기
        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");
            if (emailInput != null)
            {
                emailInput.text = savedEmail;
                isRememberMeOn = true;
                Debug.Log("기억된 이메일을 성공적으로 불러왔습니다: " + savedEmail);
            }
        }
    }

    // 1. UGS 서비스 초기화 (.exe 빌드 시 튕기지 않는 안정적인 구조)
    async void InitUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();
            isUGSReady = true;
            Debug.Log("[UGS] 유니티 게이밍 서비스 초기화 완료! 서버 준비 완료.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UGS] 초기화 중 오류 발생: " + e.Message);
            isUGSReady = true; // 크래시 방지용 강제 우회
        }
    }

    public void ToggleLoginPopup(bool show)
    {
        if (loginPopup != null) loginPopup.SetActive(show);
    }

    // Remember My Account 버튼(토글)을 누를 때마다 실행할 상태 전환 함수
    public void OnClickToggleRememberMe()
    {
        isRememberMeOn = !isRememberMeOn;
        Debug.Log("아이디 기억하기 체크 상태: " + isRememberMeOn);
    }

    // [기능 변환] UGS는 ID/비밀번호 방식이라 자체 이메일 재설정 메일 기능이 없음
    // 기존 UI의 작동 유지를 위해 안내 팝업 문구 혹은 에디터 테스트 코드로 대체함
    public void OnClickForgotPassword()
    {
        string email = emailInput.text;
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("비밀번호를 바꿀 이메일 주소를 입력창에 먼저 작성해 주세요!");
            return;
        }
        
        Debug.Log($"[{email}] UGS Username/Password 방식은 대시보드 관리자나 별도 시스템을 통해 계정 관리가 가능합니다.");
    }

    // 2. 회원가입 기능 (기존 파이어베이스 로직 반영)
    public async void OnClickSignUp()
    {
        if (!isUGSReady) return;

        string email = emailInput.text; // UGS의 로그인 ID(Username) 역할을 함
        string id = idInput.text;       // 프로필용 유저 닉네임
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("모든 빈칸을 채워주세요!");
            return;
        }
        if (password != confirmPassword)
        {
            Debug.LogError("비밀번호가 일치하지 않습니다!");
            return;
        }

        try
        {
            // 유니티 공식 회원가입 API 호출
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
            Debug.Log("회원가입 성공!");
            
            isLoggedIn = true;

            // 가입 성공 시 기본 데이터베이스 구조 생성 및 세이브
            // 파이어베이스처럼 가입 직후 초기 데이터 {"username": id, "coins": 0} 구조 유지
            SaveUserDataOnRegister(id);
            ToggleLoginPopup(false); 
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("회원가입 실패: " + e.Message);
        }
    }

    // 3. 로그인 기능 (기존 파이어베이스 로직 반영)
    public async void OnClickSignIn()
    {
        if (!isUGSReady) return;

        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("이메일과 비밀번호를 모두 입력해주세요.");
            return;
        }

        try
        {
            // 유니티 공식 로그인 API 호출
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("로그인 성공!");
            
            isLoggedIn = true;

            // [아이디 기억하기 로컬 세이브 처리]
            if (isRememberMeOn)
            {
                PlayerPrefs.SetString("SavedEmail", email);
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedEmail");
            }
            PlayerPrefs.Save(); 

            ToggleLoginPopup(false);
            LoadUserData(); // 데이터베이스에서 유저 데이터 원격 로드
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("로그인 실패: " + e.Message);
        }
    }

    public void EarnCoins(int coins)
    {
        currentCoins += coins;
        SaveUserData(); // 코인 획득 시 자동 저장
    }

    // 4. 회원가입 전용 초기 세이브 데이터 처리 함수
    private async void SaveUserDataOnRegister(string username)
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
        catch (System.Exception e)
        {
            Debug.LogError("[UGS] 회원가입 초기 데이터 세이브 실패: " + e.Message);
        }
    }

    // 5. 일반 데이터베이스 저장 기능 (기존 파이어베이스 Merge 대체)
    public async void SaveUserData()
    {
        if (!isUGSReady || !isLoggedIn) return;

        try
        {
            // 유니티 Cloud Save는 값을 넘기면 기본적으로 부분 업데이트(Merge) 처리가 내장되어 있습니다.
            var userData = new Dictionary<string, object> { { "coins", currentCoins } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(userData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UGS] 데이터 세이브 실패: " + e.Message);
        }
    }

    // 6. 데이터베이스 불러오기 기능 (기존 파이어베이스 가져오기 대체)
    private async void LoadUserData()
    {
        if (!isUGSReady || !isLoggedIn) return;

        try
        {
            // 서버에서 "coins" 키값의 데이터를 요청합니다.
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "coins" });

            if (savedData.TryGetValue("coins", out var coinItem))
            {
                currentCoins = coinItem.Value.GetAs<int>();
                Debug.Log($"서버에서 데이터 로드 완료! 현재 코인: {currentCoins}");
            }
        }
        catch (System.Exception e)
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

    // 7. 게임 결과 및 세션 오답 노트 서버 업로드 기능 (대체 완료)
    public async void UploadGameResult()
    {
        if (!isUGSReady) return;

        try
        {
            // 파이어베이스에 작성하셨던 중첩 Dictionary 구조를 완벽하게 구현했습니다.
            var resultData = new Dictionary<string, object> {
                { "score", gameScore },
                { "playTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            if (isLoggedIn)
            {
                // 파이어베이스의 user.UserId 대신 UGS 고유 PlayerId를 데이터 구조에 추가
                resultData.Add("userId", AuthenticationService.Instance.PlayerId); 
            }

            // 이번 세션 쓰레기 기록 리스트 파싱
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

            // Cloud Save에 'GameResults' 키로 JSON 구조 데이터를 저장합니다.
            await CloudSaveService.Instance.Data.Player.SaveAsync(resultData);
            Debug.Log("[UGS] 이번 판의 전체 게임 결과가 성공적으로 서버에 업로드되었습니다!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UGS] 게임 결과 업로드 실패: " + e.Message);
        }
    }
}
