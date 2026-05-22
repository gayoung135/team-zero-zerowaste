using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;
    public Camera mainCamera;
    public float pinchThreshold = 0.05f;
    private bool isDragging = false;
    private Transform targetObject;
    private Vector2 thumb;
    private Vector2 index;
    private bool hasNewData = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!hasNewData) return;
        hasNewData = false;
        ProcessHand();
    }

    public void UpdateHandData(float thumbX, float thumbY, float indexX, float indexY)
    {
        thumb = new Vector2(thumbX, thumbY);
        index = new Vector2(indexX, indexY);
        hasNewData = true;
    }
    private void ProcessHand()
    {
        float distance = Vector2.Distance(thumb, index);
        bool isPinching = distance < pinchThreshold;

        Vector3 screenPos = new Vector3(index.x * Screen.width,(1 - index.y) * Screen.height,10f);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        if (isPinching && !isDragging)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                targetObject = hit.transform;
                isDragging = true;
            }
        }

        if (isPinching && isDragging && targetObject != null) targetObject.position = worldPos; //잡기

        if (!isPinching) //손 떼기
        {
            isDragging = false;
            targetObject = null;
        }
    }
}