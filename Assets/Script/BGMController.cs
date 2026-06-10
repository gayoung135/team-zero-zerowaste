using UnityEngine;

public class BGMController : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("게임오버 상태일 때 활성화되는 오브젝트를 드래그해서 넣어주세요")]
    public GameObject gameOverObject;

    void Start()
    {
        // 내 오브젝트에 붙은 오디오 소스를 자동으로 가져옵니다.
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // [자동 찾기 기능] 수동 연결을 안 했을 경우를 대비한 안전장치
        if (gameOverObject == null)
        {
            // 1순위: 화면에 뜬 GameOver_Panel 직접 찾기
            gameOverObject = GameObject.Find("GameOver_Panel");
            
            // 2순위: 안 찾아지면 GameOver_Canvas 찾기
            if (gameOverObject == null)
            {
                gameOverObject = GameObject.Find("GameOver_Canvas");
            }
        }

        // 오브젝트가 존재하고, 시각적으로 활성화(true) 상태라면 음악을 정지합니다.
        if (gameOverObject != null && gameOverObject.activeInHierarchy)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("🎵 [BGM_Manager] 게임오버 감지! 배경음악을 정지합니다.");
            }
        }
    }
}
