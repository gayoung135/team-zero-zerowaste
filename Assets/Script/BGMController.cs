using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("게임오버 상태일 때 활성화되는 오브젝트를 드래그해서 넣어주세요")]
    public GameObject gameOverObject;

    [Header("게임오버 효과음 설정")]
    public AudioClip gameOverSound;

    private bool isGameOverSoundPlayed = false;

    // -------------------------------------------------------------
    // 소리 ON / OFF 버튼 (드래그 안 될 경우 자동 찾기 수행)
    // -------------------------------------------------------------
    [Header("--- Sound Toggle UI Buttons ---")]
    [Tooltip("드래그가 안 될 경우 하이어라키의 오브젝트 이름으로 자동 탐색합니다.")]
    public GameObject soundOnBtn;  // 소리 켜짐 버튼
    public GameObject soundOffBtn; // 음소거 버튼

    [Header("--- 버튼 자동 찾기용 오브젝트 이름 ---")]
    public string soundOnButtonName = "Sound_Button";   // 실제 하이어라키의 소리ON 버튼 이름
    public string soundOffButtonName = "Mute_Button"; // 실제 하이어라키의 음소거 버튼 이름

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // 내 오브젝트에 붙은 오디오 소스 자동 할당

        // [자동 찾기] 인스펙터 슬롯이 비어있다면(null) 이름으로 찾아옵니다!
        if (soundOnBtn == null)
        {
            soundOnBtn = GameObject.Find(soundOnButtonName);
        }

        if (soundOffBtn == null)
        {
            // 혹시 비활성화(Hide) 상태라 Find로 안 찾아질 경우를 대비해 Parent/Transform 검색
            soundOffBtn = GameObject.Find(soundOffButtonName);
            
            // 만약 시작할 때 끄기로 해둬서 안 찾아진다면 전체 오브젝트에서 탐색
            if (soundOffBtn == null)
            {
                Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t.name == soundOffButtonName && t.gameObject.scene.isLoaded)
                    {
                        soundOffBtn = t.gameObject;
                        break;
                    }
                }
            }
        }

        // 시작 시 기본 상태 초기화 (소리ON 버튼 켜기 / 음소거 버튼 꺼기)
        if (soundOnBtn != null) soundOnBtn.SetActive(true);
        if (soundOffBtn != null) soundOffBtn.SetActive(false);

        // 시작 시 음소거 해제
        if (audioSource != null)
        {
            audioSource.mute = false;
        }
    }

    void Update()
    {
        if (gameOverObject == null) // [자동 찾기 기능] 수동 연결 안 했을 경우 대비한 안전장치
        {
            gameOverObject = GameObject.Find("GameOver_Panel"); // 1순위: 화면에 뜬 GameOver_Panel find
            
            if (gameOverObject == null) // 2순위: 안 찾아지면 GameOver_Canvas find
            {
                gameOverObject = GameObject.Find("GameOver_Canvas");
            }
        }

        if (gameOverObject != null && gameOverObject.activeInHierarchy) // 오브젝트 존재, 시각적 활성화(true) 상태 -> 음악 정지
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("[BGM_Manager] 게임오버 감지! 배경음악 정지합니다.");
            }
            if (audioSource != null && gameOverSound != null && !isGameOverSoundPlayed)
            {
                audioSource.PlayOneShot(gameOverSound);
                isGameOverSoundPlayed = true; 
            }
        }
    }

    // -------------------------------------------------------------
    // 음량/음소거 버튼 클릭 시 실행할 토글 함수
    // -------------------------------------------------------------
    public void ToggleSound()
    {
        // 만약 여전히 못 찾았다면 한 번 더 찾기 시도
        if (soundOnBtn == null) soundOnBtn = GameObject.Find(soundOnButtonName);
        if (soundOffBtn == null) soundOffBtn = GameObject.Find(soundOffButtonName);

        if (soundOnBtn == null || soundOffBtn == null)
        {
            Debug.LogError("[BGMController] 버튼 오브젝트 이름을 찾을 수 없습니다! 이름을 확인해주세요.");
            return;
        }

        // 1. 현재 '소리 켜짐 버튼(soundOnBtn)'이 켜져 있는지 확인
        bool isMuting = soundOnBtn.activeSelf;

        // 2. 버튼 오브젝트 ON/OFF 전환
        soundOnBtn.SetActive(!isMuting);
        soundOffBtn.SetActive(isMuting);

        // 3. AudioSource 소리 Mute 처리
        if (audioSource != null)
        {
            audioSource.mute = isMuting; // true면 소리 끔, false면 소리 켬
        }
    }
}