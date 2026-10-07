using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TerrariumEnterGate : MonoBehaviour
{
    [Header("--- UI References ---")]
    [Tooltip("테라리움 이동 버튼")]
    [SerializeField] private Button goTerrariumButton;

    [Tooltip("'Please sign in to use this service.' 안내 메시지 패널")]
    [SerializeField] private GameObject noticeMessagePanel;

    [Header("--- Settings ---")]
    [Tooltip("안내 메시지가 유지될 시간 (초)")]
    [SerializeField] private float noticeDuration = 2.0f;

    [Tooltip("이동할 테라리움 씬 이름")]
    [SerializeField] private string terrariumSceneName = "Terrarium";

    private Coroutine noticeCoroutine;

    private void Start()
    {
        // 1. 씬 시작 시 안내 메시지 패널 비활성화
        if (noticeMessagePanel != null)
        {
            noticeMessagePanel.SetActive(false);
        }

        // 2. 버튼 클릭 이벤트 연결
        InitButtonListener();
    }

    private void InitButtonListener()
    {
        if (goTerrariumButton == null)
        {
            goTerrariumButton = GetComponent<Button>();
        }

        if (goTerrariumButton != null)
        {
            // 인스펙터 중복 이벤트 제거 후 코드 기반 리스너 등록
            goTerrariumButton.onClick.RemoveAllListeners();
            goTerrariumButton.onClick.AddListener(OnClickGoTerrarium);
        }
        else
        {
            Debug.LogError("[TerrariumEnterGate] goTerrariumButton이 연결되지 않았습니다!");
        }
    }

    /// <summary>
    /// [테라리움 가기] 버튼 클릭 시 실행
    /// </summary>
    public void OnClickGoTerrarium()
    {
        bool isUserLoggedIn = false;

        if (UGSManager.Instance != null)
        {
            isUserLoggedIn = UGSManager.Instance.isLoggedIn;
        }

        if (isUserLoggedIn)
        {
            // [회원] 게임 일시정지가 걸려있을 수 있으므로 timeScale을 1로 복구 후 씬 이동
            Time.timeScale = 1.0f;
            Debug.Log("<color=green>[입장 허용]</color> 로그인 유저 확인 완료. 테라리움 씬으로 이동합니다.");
            SceneManager.LoadScene(terrariumSceneName);
        }
        else
        {
            // [비회원] 씬 이동 차단 + 2초 타이머 팝업 출력
            Debug.Log("<color=yellow>[입장 차단]</color> 비회원 유저입니다.");
            ShowNoticeMessage();
        }
    }

    /// <summary>
    /// 안내 메시지를 noticeDuration(2초) 동안 켜두었다가 자동으로 끌어주는 함수
    /// </summary>
    private void ShowNoticeMessage()
    {
        if (noticeMessagePanel == null)
        {
            Debug.LogWarning("[경고] Notice Message Panel이 연결되어 있지 않습니다!");
            return;
        }

        // 연타했을 때 타이머가 꼬이지 않도록 기존 코루틴 멈춤 후 재시작
        if (noticeCoroutine != null)
        {
            StopCoroutine(noticeCoroutine);
        }

        noticeCoroutine = StartCoroutine(CoShowNoticeTimer());
    }

    private IEnumerator CoShowNoticeTimer()
    {
        // 1. 안내 문구 패널 활성화
        noticeMessagePanel.SetActive(true);

        // 2. Time.timeScale = 0 (게임 정지) 상태에서도 동작하는 리얼타임 대기 (2초)
        yield return new WaitForSecondsRealtime(noticeDuration);

        // 3. 2초 후 안내 문구 패널 비활성화
        noticeMessagePanel.SetActive(false);
    }
}