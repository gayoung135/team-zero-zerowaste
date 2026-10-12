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

    [Space(5)]
    [Header("[저장 관련 UI 시스템 확장]")]
    [Tooltip("Do you want to save? 라고 뜨는 저장 확인 패널 오브젝트 연결")]
    public GameObject savePopup;

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
    [Tooltip("유니티 인스펙터에서 하이어라키의 Login_Button 연결")]
    public Button loginButton;

    [Space(10)]
    [Header("[로그인 기억하기 UI 연동]")]
    [Tooltip("유니티 인스펙터에서 토글 버튼이나 체크박스(Toggle) 컴포넌트 연결")]
    public Toggle rememberMeToggle;
    private bool isRememberMeOn = false;

    void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnClickSignIn);
            Debug.Log("[이벤트 성공] 인스펙터로 연결된 로그인 버튼 활성화 완료!");
        }
        else
        {
            Button backupBtn = GetComponentInChildren<Button>(true);
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

        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");
            if (signinEmailInput != null)
            {
                signinEmailInput.text = savedEmail;
                isRememberMeOn = true;
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

        SetInputFieldTextSize(24f, 36f);
    }

    public void ToggleLoginPopup(bool show)
    {
        if (loginPopup != null) loginPopup.SetActive(show);
    }

    public void OnClickSaveButton()
    {
        if (UGSManager.Instance == null)
        {
            Debug.LogError("[상태 체크 에러] UGSManager를 찾을 수 없습니다.");
            return;
        }
        if (UGSManager.Instance.isLoggedIn)
        {
            if (loginPopup != null) loginPopup.SetActive(false);
            if (savePopup != null)
            {
                savePopup.SetActive(true);
                Debug.Log("[UI 분기 성공] 로그인 상태이므로 세이브 확인 창을 활성화합니다.");
            }
            else
            {
                Debug.LogError("[링크 누락] savePopup 변수가 유니티 인스펙터 창에 등록되지 않았습니다!");
            }
        }
        else
        {
            if (savePopup != null) savePopup.SetActive(false);
            if (loginPopup != null)
            {
                loginPopup.SetActive(true);
                Debug.Log("[UI 분기 성공] 로그아웃 상태이므로 로그인 유도 창을 활성화합니다.");
            }
        }
    }

    public void OnClickSaveYes()
    {
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.SaveUserData();
            Debug.Log("[클라우드 데이터 저장 완료] 코인 데이터가 성공적으로 서버에 업로드되었습니다.");
        }
        if (savePopup != null) savePopup.SetActive(false);
    }

    public void OnClickSaveNo()
    {
        Debug.Log("[저장 취소] 유저가 저장 취소를 선택하여 팝업을 닫습니다.");
        if (savePopup != null) savePopup.SetActive(false);
    }

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
            string resetUrl = "https://unity.com";
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
                AuthenticationService.Instance.SignOut(true);
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

            // 코인 등 기존 유저 데이터 불러옴
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

    private void SetInputFieldTextSize(float minSize, float maxSize)
    {
        TMP_InputField[] allFields = new TMP_InputField[] { 
            signupEmailInput, signupIdInput, signupPasswordInput, 
            signupConfirmPasswordInput, signinEmailInput, signinPasswordInput
        };

        foreach (var field in allFields)
        {
            if (field == null) continue;
            if (field.textComponent != null)
            {
                field.textComponent.enableAutoSizing = true;
                field.textComponent.fontSizeMin = minSize;
                field.textComponent.fontSizeMax = maxSize;
            }
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
