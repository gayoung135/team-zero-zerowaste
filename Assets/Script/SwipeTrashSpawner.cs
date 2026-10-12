using UnityEngine;

public class SwipeTrashSpawner : MonoBehaviour
{
    [Header("쓰레기 프리팹 배열")]
    public GameObject[] trashPrefabs; 

    public float span = 2.5f;        
    public float moveSpeed = 3.0f;   
    public bool leftToRight = true;

    private float delta = 0;   
    private float spawnX;
    private float spawnY;
    private float destroyX;
    private Vector3 moveDirection;

    void Start()
    {
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        spawnY = -2.5f;

        if (leftToRight)
        {
            spawnX = bottomLeft.x - 1.0f;   
            destroyX = topRight.x + 1.0f;   
            moveDirection = Vector3.right;
        }
        else
        {
            spawnX = topRight.x + 1.0f;    
            destroyX = bottomLeft.x - 1.0f; 
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
        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB.Count == 0 || trashPrefabs.Length == 0) return;

        // 1. DB 무작위 추첨
        int randomDbIndex = Random.Range(0, GameDBManager.Instance.trashGuideMasterDB.Count);
        TrashGuideData selectedDbData = GameDBManager.Instance.trashGuideMasterDB[randomDbIndex];

        // 2. 프리팹 매칭
        GameObject targetPrefab = FindPrefabByTrashName(selectedDbData.trash_name);
        if (targetPrefab == null) targetPrefab = trashPrefabs[0];

        // 3. 생성 및 데이터 세팅
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, -0.05f); 
        GameObject instantiatedTrash = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

        TrashObject trashComponent = instantiatedTrash.GetComponent<TrashObject>();
        if (trashComponent == null) trashComponent = instantiatedTrash.AddComponent<TrashObject>();
        trashComponent.trashId = selectedDbData.id; 

        // 4. 이동 컴포넌트 장착
        SwipeTrashMovement movement = instantiatedTrash.GetComponent<SwipeTrashMovement>();
        if (movement == null) movement = instantiatedTrash.AddComponent<SwipeTrashMovement>();
        movement.Setup(moveDirection, moveSpeed, destroyX, leftToRight);
    }

    private GameObject FindPrefabByTrashName(string trashName)
    {
        foreach (GameObject prefab in trashPrefabs)
        {
            if (prefab.name == trashName) return prefab;
        }
        return null;
    }
}
