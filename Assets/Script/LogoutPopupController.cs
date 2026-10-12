using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

public class LogoutPopupController : MonoBehaviour
{
    [Header("--- UI Panel & Buttons ---")]
    [Tooltip("설정창 내의 로그아웃 버튼/항목 오브젝트 (Logout)")]
    public GameObject logoutButtonObject;

    [Tooltip("로그아웃 확인 팝업 패널 (LogoutPanel)")]
    public GameObject logoutPopupPanel;

    [Header("--- Scene Settings ---")]
    [Tooltip("로그아웃 후 이동할 시작/로그인 씬 이름")]
    public string startSceneName = "StartScene";

    private void Awake()
    {
        // 시작 시 팝업 비활성화
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(false);
        }
    }

    // 설정창(SettingUI)이 켜질 때마다 자동으로 로그인 여부를 체크하여 로그아웃 버튼을 표시/숨김 합니다.
    private void OnEnable()
    {
        CheckLogoutButtonVisibility();
    }

    
    // 로그인 상태면 로그아웃 버튼 표시, 비로그인 상태면 로그아웃 버튼 숨김
    public void CheckLogoutButtonVisibility()
    {
        bool isUserLoggedIn = false;

        // UGS 로그인 상태 체크
        if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
        {
            isUserLoggedIn = true;
        }
        else if (UGSManager.Instance != null && UGSManager.Instance.isLoggedIn)
        {
            isUserLoggedIn = true;
        }

        // 로그인 상태에 따라 로그아웃 버튼 자체를 켜고 끔
        if (logoutButtonObject != null)
        {
            logoutButtonObject.SetActive(isUserLoggedIn);
            Debug.Log($"[SettingUI] 로그인 상태: {isUserLoggedIn} -> 로그아웃 버튼 활성화 여부: {isUserLoggedIn}");
        }
    }

    // ----------------------------------------------------
    // 1. [설정창 내 로그아웃 버튼] 클릭 시 호출
    // ----------------------------------------------------
    public void OnClickOpenLogoutPopup()
    {
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(true);
            logoutPopupPanel.transform.SetAsLastSibling(); // 최상단 레이어로 이동
            Debug.Log("[Logout] 로그아웃 팝업창이 열렸습니다.");
        }
    }

    // ----------------------------------------------------
    // 2. [팝업의 'NO' (아니오) 버튼] 클릭 시 호출
    // ----------------------------------------------------
    public void OnClickCancelLogout()
    {
        Debug.Log("[Logout] 로그아웃 취소");
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(false);
        }
    }

    // ----------------------------------------------------
    // 3. [팝업의 'YES' (예) 버튼] 클릭 시 호출
    // ----------------------------------------------------
    public void OnClickConfirmLogout()
    {
        try
        {
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
                AuthenticationService.Instance.ClearSessionToken();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Logout] UGS 세션 삭제 안내: " + e.Message);
        }

        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.isLoggedIn = false;
            Destroy(UGSManager.Instance.gameObject);
        }

        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(false);
        }

        Debug.Log("[UGS] 성공적으로 로그아웃되었습니다!");
        SceneManager.LoadScene(startSceneName);
    }
}