using System;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;

public class UGSAuthManager : MonoBehaviour
{
    [Header("UI References (Signup/Login UI)")]
    public GameObject loginPopup; // (Background_Panel)
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

    private bool isRememberMeOn = false;

    void Start()
    {
        // 저장된 이메일 불러오기
        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");
            if (signinEmailInput != null)
            {
                signinEmailInput.text = savedEmail;
                isRememberMeOn = true;
                Debug.Log("기억된 이메일을 성공적으로 불러왔습니다: " + savedEmail);
            }
        }
    }

    public void ToggleLoginPopup(bool show)
    {
        if (loginPopup != null) loginPopup.SetActive(show);
    }

    public void OnClickToggleRememberMe()
    {
        isRememberMeOn = !isRememberMeOn;
        Debug.Log("아이디 기억하기 체크 상태: " + isRememberMeOn);
    }

    public void OnClickForgotPassword()
    {
        string email = signinEmailInput.text;
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("비밀번호를 바꿀 이메일 주소를 입력창에 먼저 작성해 주세요!");
            return;
        }
        Debug.Log($"[{email}] UGS Username/Password 방식은 대시보드 관리자나 별도 시스템을 통해 계정 관리가 가능합니다.");
    }

    public async void OnClickSignUp()
    {
        if (!UGSManager.Instance.IsUGSReady) return;

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
            
            // UGSManager에 로그인 성공 알림 및 데이터 초기화 요청
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
        if (!UGSManager.Instance.IsUGSReady) return;

        string email = signinEmailInput.text;
        string password = signinPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("이메일과 비밀번호를 모두 입력해주세요.");
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(email, password);
            Debug.Log("로그인 성공!");
            
            // UGSManager에 로그인 성공 알림
            UGSManager.Instance.SetLoggedIn(true);

            // 이메일 기억하기 처리
            if (isRememberMeOn) PlayerPrefs.SetString("SavedEmail", email);
            else PlayerPrefs.DeleteKey("SavedEmail");
            PlayerPrefs.Save();

            ToggleLoginPopup(false);
            if (startUI != null) startUI.SetActive(true);

            UGSManager.Instance.LoadUserData();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("로그인 실패: " + e.Message);
        }
    }
}
