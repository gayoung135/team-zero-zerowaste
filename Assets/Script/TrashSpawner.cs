using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("쓰레기 프리팹 배열")]
    [Tooltip("여기에 수십 개의 쓰레기 프리팹들을 넣어두세요. 프리팹 이름이 DB의 trash_name(예: Trash_PetBottle)과 같아야 매칭됩니다.")]
    public GameObject[] trashPrefabs; 

    float span = 2.5f; 
    float delta = 0;   

    private float minX;
    private float maxX;
    private float spawnY;

    void Start()
    {
        Camera mainCamera = Camera.main;
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        minX = bottomLeft.x + 0.8f;
        maxX = topRight.x - 0.8f;
        spawnY = topRight.y;
    }

    void Update()
    {
        this.delta += Time.deltaTime;
        if (this.delta > this.span)
        {
            this.delta = 0; 
            SpawnTrash();
        }
    }

    void SpawnTrash()
    {
        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB.Count == 0 || trashPrefabs.Length == 0) return;

        // 1. DB에서 하나 무작위 추첨
        int randomDbIndex = Random.Range(0, GameDBManager.Instance.trashGuideMasterDB.Count);
        TrashGuideData selectedDbData = GameDBManager.Instance.trashGuideMasterDB[randomDbIndex];

        // 2. 추첨된 이름과 똑같은 프리팹 탐색
        GameObject targetPrefab = FindPrefabByTrashName(selectedDbData.trash_name);
        if (targetPrefab == null)
        {
            Debug.LogWarning($"[TrashSpawner] {selectedDbData.trash_name} 프리팹을 찾지 못해 0번 프리팹을 사용합니다.");
            targetPrefab = trashPrefabs[0];
        }

        // 3. 생성
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, -0.05f); 
        GameObject instantiatedTrash = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);

        // 4. 주민번호 주입
        TrashObject trashComponent = instantiatedTrash.GetComponent<TrashObject>();
        if (trashComponent == null) trashComponent = instantiatedTrash.AddComponent<TrashObject>();
        trashComponent.trashId = selectedDbData.id; 
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
