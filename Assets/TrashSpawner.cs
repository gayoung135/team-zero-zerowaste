using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    // 무작위로 생성할 쓰레기 프리팹(원본 오브젝트)들을 보관하는 배열입니다.
    public GameObject[] trashPrefabs; 

    // 쓰레기가 생성되는 주기(시간 간격, 2.5초에 한 번씩 생성)를 정의합니다.
    float span = 2.5f; 
    // 누적된 경과 시간을 실시간으로 기록하는 타이머 변수입니다.
    float delta = 0;   

    // 해상도나 화면 크기에 관계없이 자동으로 계산되어 채워질 화면 경계 좌표 변수입니다.
    private float minX;
    private float maxX;
    private float spawnY;

    void Start()
    {
        // [동적 화면 크기 계산 알고리즘]
        // 해상도가 바뀌어도 대응할 수 있도록 현재 메인 카메라의 시야를 기준으로 생성 범위를 계산합니다.
        Camera mainCamera = Camera.main;
        
        // 뷰포트 좌표(0,0 ~ 1,1)를 실제 게임 세상의 월드 좌표로 변환합니다.
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        // 생성된 쓰레기가 양쪽 화면 벽에 잘려 보이지 않도록 좌우 여백(0.8f)을 주어 안전 구역을 설정합니다.
        minX = bottomLeft.x + 0.8f;
        maxX = topRight.x - 0.8f;

        // 쓰레기가 자연스럽게 떨어지기 시작할 화면 맨 위 천장(최상단) Y 좌표를 지정합니다.
        spawnY = topRight.y;
    }

    void Update()
    {
        // 매 프레임 흐른 시간(Time.deltaTime)을 실시간으로 타이머 변수에 누적합니다.
        this.delta += Time.deltaTime;

        // 누적된 시간이 설정한 생성 주기(2.5초)를 초과하면 타이머를 초기화하고 쓰레기를 생성합니다.
        if (this.delta > this.span)
        {
            this.delta = 0; 
            SpawnTrash();
        }
    }

    void SpawnTrash()
    {
        // 등록된 쓰레기 프리팹이 없다면 에러 방지를 위해 실행을 중단합니다.
        if (trashPrefabs.Length == 0) return;

        // 자동으로 계산된 안전 구역(minX ~ maxX) 사이에서 무작위 가로(X) 위치를 결정합니다.
        float randomX = Random.Range(minX, maxX);
        
        // 무작위 가로 위치와 화면 최상단 높이를 조합하여 최종 생성 위치 좌표를 설정합니다.
        Vector3 spawnPosition = new Vector3(randomX, spawnY, -0.05f); 

        // 등록된 프리팹 배열 중에서 무작위로 하나의 쓰레기 종류를 선택합니다.
        int randomIndex = Random.Range(0, trashPrefabs.Length);
        GameObject selectedTrash = trashPrefabs[randomIndex];

        // 선택된 쓰레기 프리팹을 지정된 위치와 기본 회전값으로 화면에 실시간 생성(인스턴스화)합니다.
        Instantiate(selectedTrash, spawnPosition, Quaternion.identity);
    }
}