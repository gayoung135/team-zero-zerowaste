using UnityEngine;
using UnityEngine.UI;

public class TrashSpawner : MonoBehaviour
{
    [Header("쓰레기 프리팹")]
    public GameObject trashPrefab; 

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
        if (GameDBManager.Instance == null || GameDBManager.Instance.trashGuideMasterDB.Count == 0 || trashPrefab == null) return;

        //DB에서 하나 무작위 추첨
        int randomDbIndex = Random.Range(0, GameDBManager.Instance.trashGuideMasterDB.Count);
        TrashGuideData selectedDbData = GameDBManager.Instance.trashGuideMasterDB[randomDbIndex];

        //생성
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, -0.05f); 
        GameObject Trash = Instantiate(trashPrefab, spawnPosition, Quaternion.identity);
        TrashObject TrashScript = Trash.GetComponent<TrashObject>();
        if (TrashScript != null)
        {
            TrashScript.TrashID = selectedDbData.id;
            SpriteRenderer image = Trash.GetComponent<SpriteRenderer>();
            if (image != null) image.sprite = selectedDbData.item_image;
        }
    }
}
