using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;
    public Camera mainCamera;

    private bool[] isDragging = new bool[2];
    private Transform[] targetObject = new Transform[2];
    private Vector2[,] hand = new Vector2[2, 5];
    private bool[] hasNewData = new bool[2];

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60; //임시 프레임제한, 나중에 전체 관리 코드가 있다면 거기 넣어주세요
    }

    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!hasNewData[i]) continue;
            hasNewData[i] = false;
            ProcessHand(i);
        }
    }

    public void UpdateHandData(
        int handIndex,
        float thumbX, float thumbY,
        float indexX, float indexY,
        float ringX, float ringY,
        float wristX, float wristY,
        float middleX, float middleY)
    {
        hand[handIndex, 0] = new Vector2(thumbX, thumbY);
        hand[handIndex, 1] = new Vector2(indexX, indexY);
        hand[handIndex, 2] = new Vector2(ringX, ringY);
        hand[handIndex, 3] = new Vector2(wristX, wristY);
        hand[handIndex, 4] = new Vector2(middleX, middleY);
        hasNewData[handIndex] = true;
    }

    private void ProcessHand(int handIndex)
    {
        Vector2[] handPoint = new Vector2[5];
        for (int i = 0; i < 5; i++) handPoint[i] = hand[handIndex, i];

        bool isPinching = Vector2.Distance(handPoint[0], handPoint[1]) < 0.05f; // 엄지-검지
        bool isFist     = Vector2.Distance(handPoint[2], handPoint[3]) < 0.1f;  // 약지-손목
        bool isGrab     = isPinching || isFist;

        if (!isGrab) //놓기
        {
            isDragging[handIndex] = false;
            if (targetObject[handIndex] != null) targetObject[handIndex].GetComponent<GrabItem>()?.OnDrop();
            targetObject[handIndex] = null;
            return;
        }

        Vector3 screenPos;
        if (isPinching) screenPos = new Vector3(handPoint[1].x * Screen.width,(1 - handPoint[1].y) * Screen.height,10f); //집을 경우 검지 끝 위치로 오브젝트 이동
        else screenPos = new Vector3(handPoint[4].x * Screen.width,(1 - handPoint[4].y) * Screen.height,10f); //쥘 경우 중지-손바닥 연결 마디 부분으로 이동
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        if (!isDragging[handIndex]) //잡기
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                bool heldByOther = false;
                for (int i = 0; i < 2; i++)
                {
                    if (i == handIndex) continue;
                    if (targetObject[i] == hit.transform) 
                    {
                        heldByOther = true; 
                        break; 
                    }
                }

                if (!heldByOther)
                {
                    targetObject[handIndex] = hit.transform;
                    isDragging[handIndex] = true;
                    hit.GetComponent<GrabItem>()?.OnGrab();
                }
            }
        }

        if (isDragging[handIndex] && targetObject[handIndex] != null) targetObject[handIndex].GetComponent<GrabItem>()?.OnHandMove(worldPos);
    }
}