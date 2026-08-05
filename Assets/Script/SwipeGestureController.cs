using UnityEngine;
using System.Reflection;

public class SwipeGestureController : MonoBehaviour
{
    private SwipeTrashMovement currentTargetTrash; 
    private Vector3 targetBinPosition;    
    private bool isFlying = false;        
    private float flySpeed = 30f; 

    private GameObject leftBinObj;
    private GameObject rightBinObj;

    // 제스처 감지 쿨타임 및 원샷 발사 방지 플래그
    private float gestureCooldown = 0.25f;
    private float lastGestureTime = 0f;
    private bool isGestureActive = false; 

    // 강제 충돌 판정용 목표 쓰레기통 보관 변수
    private GameObject currentTargetBinObj;

    // HandManager 리플렉션 접근용
    private FieldInfo handArrayField;

    void Start()
    {
        leftBinObj = GameObject.Find("RecycleTrashCan");
        rightBinObj = GameObject.Find("GeneralTrashCan");

        if (typeof(HandManager) != null)
        {
            handArrayField = typeof(HandManager).GetField("hand", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }

    void Update()
    {
        // 1. 쓰레기가 중간에 파괴되어 사라지면 락 즉시 해제
        if (isFlying && currentTargetTrash == null)
        {
            isFlying = false;
        }

        // 2. 날아가는 중이 아닐 때만 키보드 및 제스처 입력 처리
        if (!isFlying)
        {
            // [A] 키보드 테스트 기능 (방향키 백업용)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TriggerSwipeLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TriggerSwipeRight();
            }

            // [B] HandManager 실시간 손동작 검사
            CheckHandManagerGesture();
        }

        // 3. 쓰레기통으로 부드럽고 빠르게 이동
        if (isFlying && currentTargetTrash != null)
        {
            currentTargetTrash.transform.position = Vector3.MoveTowards(
                currentTargetTrash.transform.position, 
                targetBinPosition, 
                flySpeed * Time.deltaTime
            );

            // [핵심 해결] 도착 판정 거리 감지를 0.5f -> 1.2f 로 여유롭게 늘려 멈춤/먹통 현상 완벽 방지
            if (Vector3.Distance(currentTargetTrash.transform.position, targetBinPosition) < 1.2f)
            {
                if (currentTargetTrash != null)
                {
                    // 쓰레기가 도착하자마자 삭제되기 직전 쓰레기통으로 OnTriggerEnter2D 강제 충돌 신호 전달 (점수 씹힘 방지)
                    if (currentTargetBinObj != null)
                    {
                        currentTargetBinObj.SendMessage("OnTriggerEnter2D", currentTargetTrash.GetComponent<Collider2D>(), SendMessageOptions.DontRequireReceiver);
                    }

                    Destroy(currentTargetTrash.gameObject);
                }
                
                // 이동 완료 후 상태 깨끗하게 초기화 (락 해제)
                currentTargetTrash = null;
                currentTargetBinObj = null;
                isFlying = false; 
            }
        }
    }

    private void CheckHandManagerGesture()
    {
        if (HandManager.Instance == null || handArrayField == null) return;

        Vector2[,] handData = handArrayField.GetValue(HandManager.Instance) as Vector2[,];
        if (handData == null) return;

        // 현재 실제로 감지되고 있는 손 인덱스 안전하게 탐색
        int validIndex = -1;
        for (int i = 0; i < handData.GetLength(0); i++)
        {
            if (handData[i, 3].sqrMagnitude > 0.01f) // 손목(wrist) 감지 확인
            {
                validIndex = i;
                break;
            }
        }

        if (validIndex == -1)
        {
            isGestureActive = false;
            return;
        }

        Vector2 thumbTip = handData[validIndex, 0];
        Vector2 indexTip = handData[validIndex, 1];
        Vector2 ringTip  = handData[validIndex, 2];
        Vector2 wrist    = handData[validIndex, 3];
        Vector2 middleMCP= handData[validIndex, 4];

        // 1. ✌️ 브이 포즈 조건
        bool isIndexUp = (wrist.y - indexTip.y) > 0.12f && (middleMCP.y - indexTip.y) > 0.04f;
        bool isPeaceSign = isIndexUp;

        // 2. 👍 엄지척 포즈 조건
        bool isThumbUp = (wrist.y - thumbTip.y) > 0.12f;
        bool isIndexFolded = Vector2.Distance(indexTip, wrist) < 0.35f;
        bool isThumbsUpSign = isThumbUp && isIndexFolded;

        // [락 해제] 두 포즈 중 아무것도 아니면 다음 인식 대기
        if (!isPeaceSign && !isThumbsUpSign)
        {
            isGestureActive = false;
            return;
        }

        // 이미 발사했다면 손을 오므릴 때까지 대기
        if (isGestureActive) return;

        // 쿨타임 검사 후 발사
        if (Time.time - lastGestureTime < gestureCooldown) return;

        if (isPeaceSign)
        {
            isGestureActive = true;
            TriggerSwipeLeft();
        }
        else if (isThumbsUpSign)
        {
            isGestureActive = true;
            TriggerSwipeRight();
        }
    }

    public void TriggerSwipeLeft()
    {
        if (isFlying) return;
        
        SwipeTrashMovement target = GetOldestTrash();
        if (target != null)
        {
            FlyToBin(target, true);
            lastGestureTime = Time.time;
        }
    }

    public void TriggerSwipeRight()
    {
        if (isFlying) return;

        SwipeTrashMovement target = GetOldestTrash();
        if (target != null)
        {
            FlyToBin(target, false);
            lastGestureTime = Time.time;
        }
    }

    private SwipeTrashMovement GetOldestTrash()
    {
        SwipeTrashMovement[] allTrashes = FindObjectsOfType<SwipeTrashMovement>();
        SwipeTrashMovement oldestTrash = null;
        float maxX = -9999f;

        foreach (var trash in allTrashes)
        {
            if (trash.enabled)
            {
                if (trash.transform.position.x > maxX)
                {
                    maxX = trash.transform.position.x;
                    oldestTrash = trash;
                }
            }
        }
        return oldestTrash;
    }

    private void FlyToBin(SwipeTrashMovement trash, bool toRecycleBin)
    {
        currentTargetTrash = trash;
        currentTargetTrash.enabled = false; 

        // 손 잡기 컴포넌트 강제 비활성화
        var grabComponent = currentTargetTrash.GetComponent("GrabItem") as MonoBehaviour;
        if (grabComponent != null)
        {
            grabComponent.enabled = false;
        }

        // Rigidbody2D 물리 이동 제어 해제
        Rigidbody2D rb = currentTargetTrash.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
        }

        // Ignore Raycast 레이어(2번)로 변경하여 손 간섭 차단
        currentTargetTrash.gameObject.layer = 2; 

        // 쓰레기통 오브젝트 재확인
        if (leftBinObj == null) leftBinObj = GameObject.Find("RecycleTrashCan");
        if (rightBinObj == null) rightBinObj = GameObject.Find("GeneralTrashCan");

        // [핵심] 목표 쓰레기통 지정 및 정확한 targetBinPosition 좌표 산출
        if (toRecycleBin)
        {
            currentTargetBinObj = leftBinObj;
            if (leftBinObj != null)
                targetBinPosition = new Vector3(leftBinObj.transform.position.x, leftBinObj.transform.position.y, currentTargetTrash.transform.position.z);
            else
                targetBinPosition = new Vector3(-6f, -1.8f, currentTargetTrash.transform.position.z);
        }
        else
        {
            currentTargetBinObj = rightBinObj;
            if (rightBinObj != null)
                targetBinPosition = new Vector3(rightBinObj.transform.position.x, rightBinObj.transform.position.y, currentTargetTrash.transform.position.z);
            else
                targetBinPosition = new Vector3(6f, -1.8f, currentTargetTrash.transform.position.z);
        }
            
        isFlying = true;
    }
}