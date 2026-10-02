using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;

public class UGSAuthManager : MonoBehaviour
{
    [Header("UI References (Signup/Login UI)")]
    public GameObject loginPopup;
    public GameObject startUI;

    [Space(10)]
    [Header("[회원가입 전용 UI 입력창]")]
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupIdInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;

    [Space(10)]
    [Header("[로그인 전용 UI 입력창]")]
    public TMP_InputField signinEmailInput;
    public TMP_InputField signinPasswordInput;

    [Space(10)]
    [Header("[로그인 컴포넌트들 직접 연동]")]
    [Tooltip("유니티 인스펙터에서 하이어라키의 Login_Button을 드래그해서 연결하세요.")]
    public Button loginButton;

    [Space(10)]
    [Header("[로그인 기억하기 UI 연동]")]
    [Tooltip("유니티 인스펙터에서 토글 버튼이나 체크박스(Toggle) 컴포넌트를 연결하세요.")]
    public Toggle rememberMeToggle;

    private bool isRememberMeOn = false;

    void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnClickSignIn);
            Debug.Log("[이벤트 성공] 인스펙터로 연결된 로그인 버튼(Login_Button) 활성화 완료!");
        }
        else
        {
            // 자식 오브젝트 전체 탐색
            Button backupBtn = GetComponentInChildren<Button>(true); // true 옵션 시 꺼져있는 버튼 찾아냄
            if (backupBtn != null && backupBtn.gameObject.name.Contains("Login"))
            {
                loginButton = backupBtn;
                loginButton.onClick.RemoveAllListeners();
                loginButton.onClick.AddListener(OnClickSignIn);
                Debug.Log("[이벤트 성공] 꺼져있던 로그인 버튼을 추적하여 자동으로 강제 연결했습니다.");
            }
            else
            {
                Debug.LogError("[경고] loginButton 변수가 유니티 인스펙터 창에서 연결되지 않았습니다. 드래그해서 넣어주세요.");
            }
        }

        // 저장된 이메일 및 '기억하기 체크 상태' 불러옴
        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");
            if (signinEmailInput != null)
            {
                signinEmailInput.text = savedEmail;
                isRememberMeOn = true;

                // 화면이 켜질 때 유니티 UI 토글 체크박스 시각적 상태도 자동으로 ON(체크) 처리
                if (rememberMeToggle != null)
                {
                    rememberMeToggle.isOn = true;
                }

                Debug.Log("기억된 이메일과 토글 상태를 성공적으로 불러왔습니다: " + savedEmail);
            }
        }
        else
        {
            isRememberMeOn = false;
            if (rememberMeToggle != null) rememberMeToggle.isOn = false;
        }

        // 입력창 폰트 깨짐 방지
        SetInputFieldTextSize(24f, 36f);
    }

    public void ToggleLoginPopup(bool show)
    {
        if (loginPopup != null) loginPopup.SetActive(show);
    }

    // UI 체크박스(Toggle)시 호출되어 상태 동기화
    public void OnClickToggleRememberMe()
    {
        if (rememberMeToggle != null)
        {
            isRememberMeOn = rememberMeToggle.isOn;
        }
        else
        {
            isRememberMeOn = !isRememberMeOn;
        }
        Debug.Log("아이디 기억하기 체크 상태: " + isRememberMeOn);
    }

    // 비밀번호 찾기 웹 연동 시스템
    public void OnClickForgotPassword()
    {
        string email = signinEmailInput.text;
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("비밀번호를 바꿀 이메일 주소를 [로그인 이메일 입력창]에 먼저 작성해 주세요!");
            return;
        }

        try
        {
            // 구글 유니티 공식 비밀번호 분실 복구 웹 페이지 주소
            string resetUrl = "https://id.unity.com/en/password/new";

            // 인터넷 브라우저 창 열기
            Application.OpenURL(resetUrl);

            Debug.Log($"[{email}] 유저 계정의 비밀번호 분실 복구 페이지를 정상적으로 열었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError("비밀번호 재설정 페이지 열기 실패: " + e.Message);
        }
    }

    public async void OnClickSignUp()
    {
        if (UGSManager.Instance == null || !UGSManager.Instance.IsUGSReady)
        {
            Debug.LogError("UGS 서비스가 아직 준비되지 않았거나 매니저를 찾을 수 없습니다.");
            return;
        }

        string email = signupEmailInput.text;
        string id = signupIdInput.text;
        string password = signupPasswordInput.text;
        string confirmPassword = signupConfirmPasswordInput.text;

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
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(email, password);
            Debug.Log("회원가입 성공!");

            UGSManager.Instance.SetLoggedIn(true);
            UGSManager.Instance.SaveUserDataOnRegister(id);
            ToggleLoginPopup(false);
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("회원가입 실패: " + e.Message);
        }
    }

    public async void OnClickSignIn()
    {
        // 1. 매니저 상태 사전 체크
        if (UGSManager.Instance == null)
        {
            Debug.LogError("[로그인 차단] UGSManager.Instance 싱글톤을 찾을 수 없습니다! 하이어라키를 확인하세요.");
            return;
        }

        if (!UGSManager.Instance.IsUGSReady)
        {
            Debug.LogError("[로그인 차단] UGS 게이밍 서비스가 아직 초기화(Ready)되지 않았습니다.");
            return;
        }

        string email = signinEmailInput.text;
        string password = signinPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("이메일과 비밀번호를 모두 입력해주세요.");
            return;
        }

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("[세션 초기화] 이전 씬의 인증이 발견되어 로그아웃 세션을 비웁니다.");
                AuthenticationService.Instance.SignOut(true); // true 인자를 주어 세션 토큰까지 완전히 클리어
            }

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("<color=green>[성공]</color> UGS 서버 로그인에 성공했습니다!");

            UGSManager.Instance.SetLoggedIn(true);

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
            if (startUI != null) startUI.SetActive(true);

            UGSManager.Instance.LoadUserData();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"[로그인 실패] UGS 서버 인증 오류: 코드({e.ErrorCode}) - {e.Message}");

            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[시스템 오류] 예기치 못한 에러 발생: {ex.Message}");
        }
    }


    // 모든 InputField창의 텍스트 컴포넌트 폰트 크기 코드로
    private void SetInputFieldTextSize(float minSize, float maxSize)
    {
        TMP_InputField[] allFields = new TMP_InputField[] { signupEmailInput, signupIdInput, signupPasswordInput, signupConfirmPasswordInput, signinEmailInput, signinPasswordInput };

        foreach (var field in allFields)
        {
            if (field == null) continue;

            // 본문 텍스트 창 크기 제어
            if (field.textComponent != null)
            {
                field.textComponent.enableAutoSizing = true;
                field.textComponent.fontSizeMin = minSize;
                field.textComponent.fontSizeMax = maxSize;
            }
            // 가이드 문구(Placeholder) 창 크기 제어
            if (field.placeholder != null)
            {
                TMP_Text placeholderText = field.placeholder.GetComponent<TMP_Text>();
                if (placeholderText != null)
                {
                    placeholderText.enableAutoSizing = true;
                    placeholderText.fontSizeMin = minSize;
                    placeholderText.fontSizeMax = maxSize;
                }
            }
        }
    }
}
