using UnityEngine;
using Firebase;
using Firebase.Auth; // 인증(로그인/회원가입) 기능을 위해 추가
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro; // UI 구성에 따라 InputField 종류가 다를 경우를 대비

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

    private FirebaseAuth auth; // 로그인용 추가
    private FirebaseFirestore db;
    private FirebaseUser user; // 현재 로그인한 유저 정보용 추가
    private bool isFirebaseReady = false;

    [Header("UI References (Signup/Login UI)")]
    public GameObject loginPopup;       // 팝업창 오브젝트 (Background_Panel)
    public TMPro.TMP_InputField emailInput;           // E-mail 입력창
    public TMPro.TMP_InputField idInput;              // ID 입력창 (닉네임)
    public TMPro.TMP_InputField passwordInput;        // Password 입력창
    public TMPro.TMP_InputField confirmPasswordInput; // Confirm Password 입력창

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
            InitFirebase();
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

    void InitFirebase()
    {
        // 💡 유니티 에디터 안에서 플레이 버튼을 누를 때는 서버 연결을 건너뜁니다.
#if UNITY_EDITOR
        Debug.Log("Firebase: 유니티 에디터 환경이므로 안전을 위해 서버 통신을 건너뜁니다.");
        isFirebaseReady = true;
        return;
#endif

        // 💡 [빌드 전용 수정] 빌드된 .exe 파일이 켜질 때 파이어베이스가 메모리를 즉시 건드리지 못하도록
        // 유니티 6 엔진의 첫 프레임 로딩이 완전히 끝난 0.5초 뒤에 동적으로 실행되도록 안전 버퍼를 둡니다.
        Invoke("StartFirebaseSafely", 0.5f);
    }

    void StartFirebaseSafely()
    {
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                // [안전 검사] 의존성 체크 태스크 자체에 에러가 있다면 즉시 차단하여 크래시 방지
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Firebase 의존성 체크 실패 (태스크 에러): " + task.Exception);
                    isFirebaseReady = true; // 무한 로딩 방지를 위한 강제 우회
                    return;
                }

                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    db = FirebaseFirestore.DefaultInstance;
                    isFirebaseReady = true;
                    Debug.Log("FirebaseManager: 독립 실행 파일에서 서버 및 인증 준비 완료!");
                    auth.StateChanged += AuthStateChanged;
                }
                else
                {
                    Debug.LogError($"Firebase 초기화 실패 (상태 코드: {task.Result})");
                    isFirebaseReady = true;
                }
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError("파이어베이스 초기화 중 예외 발생 (우회 처리): " + e.Message);
            isFirebaseReady = true; // 게임이 아예 꺼지는 크래시를 막기 위해 강제 통과
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

    // Forget Password 버튼을 눌렀을 때 작동할 비밀번호 재설정 이메일 전송 함수
    public void OnClickForgotPassword()
    {
#if UNITY_EDITOR
        Debug.Log($"[에디터 테스트] {emailInput.text} 주소로 비밀번호 재설정 링크 발송 시도!");
        return;
#endif

        if (!isFirebaseReady || auth == null) return;

        string email = emailInput.text;

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("비밀번호를 바꿀 이메일 주소를 입력창에 먼저 작성해 주세요!");
            return;
        }

        // 파이어베이스에게 입력된 이메일로 비밀번호 재설정 메일을 발송하라고 요청
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("비밀번호 재설정 이메일 발송 실패: " + task.Exception);
                return;
            }
            Debug.Log($"[{email}] 메일함으로 비밀번호 재설정 링크가 무사히 발송되었습니다! 메일함을 확인해 보세요.");
        });
    }

    public void OnClickSignUp()
    {
#if UNITY_EDITOR
        Debug.Log($"[에디터 테스트] 회원가입 시도 - 이메일: {emailInput.text}, ID: {idInput.text}");
        ToggleLoginPopup(false);
        return;
#endif

        if (!isFirebaseReady || auth == null || db == null) return;
        string email = emailInput.text;
        string id = idInput.text;
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

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // [안전 검사] 회원가입 실패 시 task.Result 접근을 완벽히 차단
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("회원가입 실패: " + task.Exception);
                return;
            }
            Debug.Log("회원가입 성공!");
            user = task.Result.User;
            DocumentReference docRef = db.Collection("Users").Document(user.UserId);
            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "username", id },
                { "coins", 0 }
            };
            docRef.SetAsync(userData);
            ToggleLoginPopup(false); // 가입 성공 시 창 닫기
        });
    }

    public void OnClickSignIn()
    {
#if UNITY_EDITOR
        Debug.Log($"[에디터 테스트] 로그인 시도 - 이메일: {emailInput.text}");
        isLoggedIn = true;
        ToggleLoginPopup(false);
        return;
#endif

        if (!isFirebaseReady || auth == null) return;
        string email = emailInput.text;
        string password = passwordInput.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            // [안전 검사] 로그인 실패 시 task.Result 접근을 완벽히 차단
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("로그인 실패: " + task.Exception);
                return;
            }
            Debug.Log("로그인 성공!");
            user = task.Result.User;
            isLoggedIn = true;

            // 💡 [추가] 로그인 성공 구역에 아이디 기억하기 로컬 세이브 처리 연동
            if (isRememberMeOn)
            {
                PlayerPrefs.SetString("SavedEmail", email); // 토글 켜져 있으면 PC 로컬 메모리에 이메일 주소 저장
            }
            else
            {
                PlayerPrefs.DeleteKey("SavedEmail"); // 토글 꺼져 있으면 저장되어 있던 이메일 정보 강제 삭제
            }
            PlayerPrefs.Save(); // 로컬 메모리 즉시 저장 반영

            ToggleLoginPopup(false);
            LoadUserData(user.UserId); // 데이터베이스에서 코인 불러오기
        });
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth != null && auth.CurrentUser != user)
        {
            user = auth.CurrentUser;
            if (user != null)
            {
                isLoggedIn = true;
                LoadUserData(user.UserId);
            }
        }
    }

    public void EarnCoins(int coins)
    {
        currentCoins += coins;
        SaveUserData(); // 코인 획득 시 자동 저장
    }

    public void SaveUserData()
    {
        if (!isFirebaseReady || user == null || db == null) return;
        DocumentReference docRef = db.Collection("Users").Document(user.UserId);
        Dictionary<string, object> userData = new Dictionary<string, object> { { "coins", currentCoins } };
        docRef.SetAsync(userData, SetOptions.MergeAll);
    }

    private void LoadUserData(string userId)
    {
        if (db == null) return;

        db.Collection("Users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            // [안전 검사] 데이터 로드 실패나 서버 연결이 끊겼을 때 task.Result 접근 제한
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("서버 데이터 로드 실패: " + task.Exception);
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                currentCoins = task.Result.GetValue<int>("coins");
                Debug.Log($"서버에서 데이터 로드 완료! 현재 코인: {currentCoins}");
            }
        });
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

    public void UploadGameResult()
    {
        if (!isFirebaseReady || db == null) return;

        Dictionary<string, object> resultData = new Dictionary<string, object> {
            { "score", gameScore },
            { "playTime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };
        if (isLoggedIn && user != null)
        {
            resultData.Add("userId", user.UserId); // 로그인 유저일 경우 로그에 UID 남기기
        }
        List<Dictionary<string, object>> logList = new List<Dictionary<string, object>>();
        foreach (var log in currentSessionLogs)
        {
            var doc = new Dictionary<string, object>
        {
            { "trashName", log.trashName },
            { "userAnswer", log.userAnswer },
            { "correctAnswer", log.correctAnswer },
            { "isCorrect", log.isCorrect }, { "reason", log.reason }
            };
            logList.Add(doc);
        }
        resultData.Add("logs", logList);
        db.Collection("GameResults").AddAsync(resultData).ContinueWithOnMainThread(task =>
        {
            // [안전 검사] 콜백 완료 및 에러 유무까지 확실히 확인 후 로그 출력
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("[Firebase] 이번 판의 전체 게임 결과가 성공적으로 서버에 업로드되었습니다!");
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("[Firebase] 게임 결과 업로드 실패: " + task.Exception);
            }
        });
    }
}