using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TerrariumEnterGate : MonoBehaviour
{
    [Header("--- UI References ---")]
    [Tooltip("우측 상단 '테라리움가기' (GO TERRARIUM) 버튼")]
    [SerializeField] private Button goTerrariumButton;

    [Tooltip("'Please sign in to use this service.' 안내 메시지 패널/텍스트 오브젝트")]
    [SerializeField] private GameObject noticeMessagePanel;

    [Header("--- Settings ---")]
    [Tooltip("안내 메시지가 화면에 노출되는 시간 (초)")]
    [SerializeField] private float noticeDuration = 2.0f;

    [Tooltip("이동할 테라리움 씬 이름")]
    [SerializeField] private string terrariumSceneName = "Terrarium";

    private Coroutine noticeCoroutine;

    private void Start()
    {
        // 1. 시작 시 안내 메시지 패널 숨기기
        if (noticeMessagePanel != null)
        {
            noticeMessagePanel.SetActive(false);
        }

        // 2. 버튼 클릭 이벤트 연결 (기존 인스펙터 중복 이벤트 제거)
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
            // [중요] 인스펙터나 타 스크립트에서 중복으로 등록된 씬 이동 이벤트를 제거하여 2초 뒤 자동 입장 방지
            goTerrariumButton.onClick.RemoveAllListeners();
            goTerrariumButton.onClick.AddListener(OnClickGoTerrarium);
        }
        else
        {
            Debug.LogError("[TerrariumEnterGate] Go Terrarium Button이 연결되지 않았습니다!");
        }
    }

    /// <summary>
    /// [GO TERRARIUM] 버튼 클릭 시 실행
    /// </summary>
    public void OnClickGoTerrarium()
    {
        // 1. UGSManager의 로그인 상태 확인
        bool isUserLoggedIn = false;

        if (UGSManager.Instance != null)
        {
            isUserLoggedIn = UGSManager.Instance.isLoggedIn;
        }

        // 2. 회원 / 비회원 완벽 분기 처리
        if (isUserLoggedIn)
        {
            // [회원] 로그인된 유저만 씬 이동 실행
            Debug.Log("<color=green>[입장 허용]</color> 로그인 상태가 확인되었습니다. 테라리움 씬으로 이동합니다.");
            SceneManager.LoadScene(terrariumSceneName);
        }
        else
        {
            // [비회원] 씬 이동(SceneManager.LoadScene)을 절대로 실행하지 않고 메시지만 표시
            Debug.Log("<color=yellow>[입장 차단]</color> 비회원 상태입니다. 씬 이동을 완전히 차단합니다.");
            ShowNoticeMessage();
        }
    }

    /// <summary>
    /// 안내 메시지 팝업을 noticeDuration 동안만 띄우는 함수
    /// </summary>
    private void ShowNoticeMessage()
    {
        if (noticeMessagePanel == null)
        {
            Debug.LogWarning("[경고] Notice Message Panel이 연결되어 있지 않습니다!");
            return;
        }

        // 이미 작동 중인 코루틴이 있다면 타이머 리셋
        if (noticeCoroutine != null)
        {
            StopCoroutine(noticeCoroutine);
        }

        noticeCoroutine = StartCoroutine(CoShowNotice());
    }

    private IEnumerator CoShowNotice()
    {
        noticeMessagePanel.SetActive(true);
        yield return new WaitForSeconds(noticeDuration);
        noticeMessagePanel.SetActive(false); // 2초 뒤 메세지 패널만 뜨겠끔 수정
    }
}