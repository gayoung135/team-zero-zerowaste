using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;

public class LogoutPopupController : MonoBehaviour
{
    [Header("--- UI Panel ---")]
    [Tooltip("로그아웃 확인 팝업 패널 (LogoutPanel)")]
    public GameObject logoutPopupPanel;

    [Header("--- Scene Settings ---")]
    [Tooltip("로그아웃 후 이동할 시작/로그인 씬 이름")]
    public string startSceneName = "StartScene";

    private void Awake()
    {
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(false);
        }
    }

    // ----------------------------------------------------
    // 1. [설정창 내 로그아웃 버튼] 클릭 시 호출
    // ----------------------------------------------------
    public void OnClickOpenLogoutPopup()
    {
        // 로그인 상태 체크
        bool isUserLoggedIn = false;

        if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
        {
            isUserLoggedIn = true;
        }
        else if (UGSManager.Instance != null && UGSManager.Instance.isLoggedIn)
        {
            isUserLoggedIn = true;
        }

        if (!isUserLoggedIn)
        {
            Debug.LogWarning("[Logout] 현재 비로그인 상태입니다. 로그아웃 팝업을 열지 않습니다.");
            if (logoutPopupPanel != null)
            {
                logoutPopupPanel.SetActive(false);
            }
            return;
        }

        // 프레임 씹힘 방지를 위해 코루틴으로 1프레임 지연 후 팝업 호출
        StartCoroutine(OpenPopupNextFrame());
    }

    private IEnumerator OpenPopupNextFrame()
    {
        // 클릭 이벤트 전달을 마무리할 수 있도록 1프레임 대기
        yield return null;

        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(true);
            logoutPopupPanel.transform.SetAsLastSibling(); // 화면 맨 앞으로
            Debug.Log("[Logout] 로그아웃 팝업창이 성공적으로 열렸습니다!");
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