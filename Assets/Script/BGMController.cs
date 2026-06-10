using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("게임오버 상태일 때 활성화되는 오브젝트를 드래그해서 넣어주세요")]
    public GameObject gameOverObject;

    [Header("게임오버 효과음 설정")]
    public AudioClip gameOverSound;

    private bool isGameOverSoundPlayed = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // 내 오브젝트에 붙은 오디오 소스를 자동으로 가져옴.
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
}
