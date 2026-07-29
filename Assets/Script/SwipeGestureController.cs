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
        // 1. 쓰레기가 중간에 사라지면 락 해제
        if (isFlying && currentTargetTrash == null)
        {
            isFlying = false;
        }

        // 2. 날아가는 중이 아닐 때만 입력 및 제스처 처리
        if (!isFlying)
        {
            // [A] 키보드 테스트 기능 (백업용)
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

            if (Vector3.Distance(currentTargetTrash.transform.position, targetBinPosition) < 0.5f)
            {
                if (currentTargetTrash != null)
                {
                    Destroy(currentTargetTrash.gameObject);
                }
                currentTargetTrash = null;
                isFlying = false; 
            }
        }
    }

    private void CheckHandManagerGesture()
    {
        if (HandManager.Instance == null || handArrayField == null) return;

        Vector2[,] handData = handArrayField.GetValue(HandManager.Instance) as Vector2[,];
        if (handData == null) return;

        Vector2 thumbTip = handData[0, 0];
        Vector2 indexTip = handData[0, 1];
        Vector2 ringTip  = handData[0, 2];
        Vector2 wrist    = handData[0, 3];
        Vector2 middleMCP= handData[0, 4];

        // 손이 화면에 없거나 (0,0) 좌표면 상태 초기화 후 즉시 종료
        if (wrist.sqrMagnitude < 0.01f || indexTip.sqrMagnitude < 0.01f)
        {
            isGestureActive = false;
            return;
        }

        // 1. ✌️ 브이 포즈 조건
        bool isIndexUp = (wrist.y - indexTip.y) > 0.12f && (middleMCP.y - indexTip.y) > 0.04f;
        bool isPeaceSign = isIndexUp;

        // 2. 👍 엄지척 포즈 조건
        bool isThumbUp = (wrist.y - thumbTip.y) > 0.12f;
        bool isIndexFolded = Vector2.Distance(indexTip, wrist) < 0.35f;
        bool isThumbsUpSign = isThumbUp && isIndexFolded;

        // [락 해제] 두 포즈 중 아무것도 아니면 즉시 차기 발사 준비
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
            //Debug.Log("✌️ [스와이프] 브이 감지! -> 왼쪽 발사");   (콘솔창에 자주떠서 꺼두겠습니다)
            isGestureActive = true;
            TriggerSwipeLeft();
        }
        else if (isThumbsUpSign)
        {
            //Debug.Log("👍 [스와이프] 엄지척 감지! -> 오른쪽 발사");
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

        // [핵심 수정] 손 잡기 연산 컴포넌트 강제 비활성화 및 레이어 변경으로 손에 물리는 현상 차단
        var grabComponent = currentTargetTrash.GetComponent("GrabItem") as MonoBehaviour;
        if (grabComponent != null)
        {
            grabComponent.enabled = false;
        }

        // Rigidbody2D가 있다면 물리 이동 제어를 받지 않게 Kinematic 처리
        Rigidbody2D rb = currentTargetTrash.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
        }

        // Ignore Raycast 레이어(2번)로 바꿔서 HandManager의 레이캐스트 잡기에 안 걸리게 설정
        currentTargetTrash.gameObject.layer = 2; 

        if (toRecycleBin)
        {
            if (leftBinObj != null)
                targetBinPosition = leftBinObj.transform.position;
            else
                targetBinPosition = new Vector3(10.63f, 108.12f, currentTargetTrash.transform.position.z);
        }
        else
        {
            if (rightBinObj != null)
                targetBinPosition = rightBinObj.transform.position;
            else
                targetBinPosition = new Vector3(22.35f, 108.12f, currentTargetTrash.transform.position.z);
        }
            
        isFlying = true;
    }
} 