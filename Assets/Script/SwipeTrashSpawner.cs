using UnityEngine;

public class SwipeTrashSpawner : MonoBehaviour
{
    [Header("쓰레기 프리팹 배열")]
    public GameObject[] trashPrefabs; 

    [Header("밸런스 설정")]
    public float span = 2.5f;        // 생성 주기(초)
    public float moveSpeed = 3.0f;   // 쓰레기가 옆으로 흘러가는 속도
    
    [Header("이동 방향 설정 (체크하면 왼쪽->오른쪽, 해제하면 오른쪽->왼쪽)")]
    public bool leftToRight = true;

    private float delta = 0;   
    private float spawnX;
    private float spawnY;
    private float destroyX;
    private Vector3 moveDirection;

    void Start()
    {
        // [동적 화면 크기 기반 좌우 끝단 계산]
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        // 기획서 스크린샷의 컨베이어 벨트 높이에 맞춰 소폭 조정 가능 (현재는 중앙 Y축 기준 부근)
        // 씬 뷰를 보고 쓰레기통들 살짝 위(예: -1.0f 등)로 고정하고 싶다면 아래 spawnY를 고정값으로 바꾸셔도 됩니다.
        //spawnY = (bottomLeft.y + topRight.y) * 0.3f; 

        spawnY = -2.5f;

        if (leftToRight)
        {
            spawnX = bottomLeft.x - 1.0f;   // 화면 왼쪽 바깥에서 생성
            destroyX = topRight.x + 1.0f;   // 화면 오른쪽 바깥으로 나가면 삭제
            moveDirection = Vector3.right;
        }
        else
        {
            spawnX = topRight.x + 1.0f;    // 화면 오른쪽 바깥에서 생성
            destroyX = bottomLeft.x - 1.0f; // 화면 왼쪽 바깥으로 나가면 삭제
            moveDirection = Vector3.left;
        }
    }

    void Update()
    {
        this.delta += Time.deltaTime;

        if (this.delta > this.span)
        {
            this.delta = 0; 
            SpawnSwipeTrash();
        }
    }

    void SpawnSwipeTrash()
    {
        if (trashPrefabs.Length == 0) return;

        // 생성 위치 결정
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, -0.05f); 

        // 무작위 쓰레기 종류 선택
        int randomIndex = Random.Range(0, trashPrefabs.Length);
        GameObject selectedTrash = trashPrefabs[randomIndex];

        // 쓰레기 생성
        GameObject instantiatedTrash = Instantiate(selectedTrash, spawnPosition, Quaternion.identity);

        // 생성된 쓰레기에 가로 이동 컴포넌트를 동적으로 붙이고 설정값을 주입합니다.
        SwipeTrashMovement movement = instantiatedTrash.GetComponent<SwipeTrashMovement>();
        if (movement == null)
        {
            movement = instantiatedTrash.AddComponent<SwipeTrashMovement>();
        }
        
        // 이동에 필요한 데이터 전달
        movement.Setup(moveDirection, moveSpeed, destroyX, leftToRight);
    }
}