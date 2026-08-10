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

    private void Start()
    {
        // 시작 시 팝업 패널 비활성화
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
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(true);
        }
    }

    // ----------------------------------------------------
    // 2. [팝업의 'NO' (아니오) 버튼] 클릭 시 호출
    // ----------------------------------------------------
    public void OnClickCancelLogout()
    {
        // 로그아웃 취소 안내 로그
        Debug.Log("[Logout] 로그아웃을 취소했습니다. 설정 화면을 유지합니다.");

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
            // 1. UGS 인증 토큰 및 세션 정보 완전히 삭제
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut(true);
                AuthenticationService.Instance.ClearSessionToken();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Logout] UGS 세션 삭제 중 안내: " + e.Message);
        }

        // 2. 재로그인 시 UI 인풋창 연결(Missing) 문제를 방지하기 위해 싱글톤 파괴
        if (UGSManager.Instance != null)
        {
            Destroy(UGSManager.Instance.gameObject);
        }

        // 3. 팝업 패널 비활성화
        if (logoutPopupPanel != null)
        {
            logoutPopupPanel.SetActive(false);
        }

        // 4. 콘솔 창에 선명하게 로그아웃 성공 메시지 출력!
        Debug.Log("[UGS] 성공적으로 로그아웃되었습니다! 로그인/시작 화면으로 이동합니다.");

        // 5. 시작/로그인 씬으로 이동
        SceneManager.LoadScene(startSceneName);
    }
}