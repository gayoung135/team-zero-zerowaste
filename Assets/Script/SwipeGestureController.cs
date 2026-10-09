using UnityEngine;
using System.Reflection;

public class SwipeGestureController : MonoBehaviour
{

    // 쓰레기 처리
    private SwipeTrashMovement currentTargetTrash;
    private Vector3 targetBinPosition;

    private bool isFlying = false;
    private float flySpeed = 30f;

    private GameObject leftBinObj;
    private GameObject rightBinObj;
    private GameObject currentTargetBinObj;

    // 제스처 관련
    private FieldInfo handArrayField;

    // 한 번 인식한 후 같은 포즈로 다시 발동하지 않도록 잠금
    private bool isGestureLocked = false;

    // 제스처 인식 후 최소 대기시간
    private float gestureCooldown = 0.5f;
    private float lastGestureTime = -10f;

    // 같은 포즈가 몇 프레임 유지되어야 인정할지
    private int requiredStableFrames = 5;

    private int paperStableFrames = 0;
    private int fistStableFrames = 0;

  
    // 제스처 상태
    private enum GestureType
    {
        None,
        Paper,
        Fist
    }

    private GestureType currentGesture = GestureType.None;

    void Start()
    {
        leftBinObj = GameObject.Find("RecycleTrashCan");
        rightBinObj = GameObject.Find("GeneralTrashCan");

        handArrayField = typeof(HandManager).GetField(
            "hand",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
    }

    void Update()
    {

        // 현재 쓰레기가 사라졌다면 비행 종료
        if (isFlying && currentTargetTrash == null)
        {
            FinishTrashMovement();
        }


        // 쓰레기 이동 중이 아닐 때만 입력 처리
        if (!isFlying)
        {
            // 키보드 테스트
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                TriggerSwipeLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                TriggerSwipeRight();
            }

            // 손 제스처
            CheckHandManagerGesture();
        }


        // 쓰레기를 쓰레기통으로 이동

        if (isFlying && currentTargetTrash != null)
        {
            MoveTrashToBin();
        }
    }

    // 쓰레기 이동
    private void MoveTrashToBin()
    {
        currentTargetTrash.transform.position = Vector3.MoveTowards(
            currentTargetTrash.transform.position,
            targetBinPosition,
            flySpeed * Time.deltaTime
        );


        float distance = Vector3.Distance(
            currentTargetTrash.transform.position,
            targetBinPosition
        );


        // 충분히 가까워지면 쓰레기통에 들어간 것으로 처리
        if (distance < 1.2f)
        {
            if (currentTargetTrash != null)
            {
                // 쓰레기통의 OnTriggerEnter2D를 강제로 호출
                if (currentTargetBinObj != null)
                {
                    Collider2D trashCollider =
                        currentTargetTrash.GetComponent<Collider2D>();

                    currentTargetBinObj.SendMessage(
                        "OnTriggerEnter2D",
                        trashCollider,
                        SendMessageOptions.DontRequireReceiver
                    );
                }


                Destroy(currentTargetTrash.gameObject);
            }


            FinishTrashMovement();
        }
    }

    // 쓰레기 이동 종료
    private void FinishTrashMovement()
    {
        currentTargetTrash = null;
        currentTargetBinObj = null;
        isFlying = false;

        // 중요:
        // 쓰레기가 끝났다고 해서 바로 같은 포즈를 다시 허용하지 않음.
        // 반드시 손이 중립 상태가 된 다음 (손을 캠에서 내리고 다시 포즈를 취하라는 뜻)
        // 새로운 주먹/보자기를 해야 함.

        isGestureLocked = true;

        paperStableFrames = 0;
        fistStableFrames = 0;

        currentGesture = GestureType.None;
    }


    // 손 제스처 검사
    private void CheckHandManagerGesture()
    {
        if (HandManager.Instance == null || handArrayField == null)
            return;


        Vector2[,] handData =
            handArrayField.GetValue(HandManager.Instance) as Vector2[,];

        if (handData == null)
            return;


        // 감지된 손 찾기
        int validIndex = -1;

        for (int i = 0; i < handData.GetLength(0); i++)
        {
            // 손목
            Vector2 wrist = handData[i, 3];

            if (wrist.sqrMagnitude > 0.01f)
            {
                validIndex = i;
                break;
            }
        }


        // 손이 아예 안 보이는 경우

        if (validIndex == -1)
        {
            ResetGestureDetection();
            return;
        }


        // 손 데이터
        Vector2 thumbTip = handData[validIndex, 0];
        Vector2 indexTip = handData[validIndex, 1];
        Vector2 ringTip = handData[validIndex, 2];

        Vector2 wristPos = handData[validIndex, 3];
        Vector2 middleMCP = handData[validIndex, 4];


        // 손 크기 계산
        float handScale =
            Vector2.Distance(wristPos, middleMCP);

        if (handScale < 0.01f)
        {
            handScale = 0.15f;
        }

        // 손가락 거리
        float distIndex =
            Vector2.Distance(indexTip, wristPos) / handScale;

        float distRing =
            Vector2.Distance(ringTip, wristPos) / handScale;



        // 주먹 / 보자기 판정

        // 보자기
        // 검지 + 약지가 충분히 펴져 있어야 함
        //
        bool isPaper =
            distIndex > 1.25f &&
            distRing > 1.25f;


        // 주먹
        // 검지 + 약지가 손바닥 쪽으로 들어온 상태
        //
        bool isFist =
            distIndex < 0.85f &&
            distRing < 0.85f;


        // 중립 상태
        if (!isPaper && !isFist)
        {
            paperStableFrames = 0;
            fistStableFrames = 0;

            currentGesture = GestureType.None;

            //  !!핵심!!
            //
            // 중립 상태가 확인되면 다시 제스처 사용 가능
            //
            isGestureLocked = false;

            return;
        }

        // 보자기 안정성 검사
        if (isPaper)
        {
            paperStableFrames++;

            fistStableFrames = 0;

            currentGesture = GestureType.Paper;
        }
        else
        {
            paperStableFrames = 0;
        }

        // 주먹 안정성 검사
        if (isFist)
        {
            fistStableFrames++;

            paperStableFrames = 0;

            currentGesture = GestureType.Fist;
        }
        else
        {
            fistStableFrames = 0;
        }

        // 이미 발동한 상태면 아무것도 하지 않음
        if (isGestureLocked)
            return;

        // 쿨타임
        if (Time.time - lastGestureTime < gestureCooldown)
            return;


        // 보자기 발동
        if (paperStableFrames >= requiredStableFrames)
        {
            // 즉시 잠금
            isGestureLocked = true;

            lastGestureTime = Time.time;

            paperStableFrames = 0;
            fistStableFrames = 0;

            TriggerSwipeLeft();

            return;
        }


        // 주먹 발동
        if (fistStableFrames >= requiredStableFrames)
        {
            // 즉시 잠금
            isGestureLocked = true;

            lastGestureTime = Time.time;

            paperStableFrames = 0;
            fistStableFrames = 0;

            TriggerSwipeRight();

            return;
        }
    }


    // 제스처 초기화
    private void ResetGestureDetection()
    {
        paperStableFrames = 0;
        fistStableFrames = 0;

        currentGesture = GestureType.None;

        // 손이 사라져도 바로 잠금 해제
        // 다음 손이 들어왔을 때 새로운 포즈로 인식 가능
        isGestureLocked = false;
    }

    // 왼쪽 이동
    public void TriggerSwipeLeft()
    {
        // 이동 중이면 절대 새로운 쓰레기를 선택하지 않음
        if (isFlying)
            return;


        SwipeTrashMovement target = GetLeadingTrash();


        if (target != null)
        {
            FlyToBin(target, true);
        }
    }


    // 오른쪽 이동
    public void TriggerSwipeRight()
    {
        // 이동 중이면 절대 새로운 쓰레기를 선택하지 않음
        if (isFlying)
            return;


        SwipeTrashMovement target = GetLeadingTrash();


        if (target != null)
        {
            FlyToBin(target, false);
        }
    }


    // 가장 앞에 있는 쓰레기 찾기
    private SwipeTrashMovement GetLeadingTrash()
    {
        SwipeTrashMovement[] allTrashes =
            FindObjectsOfType<SwipeTrashMovement>();


        SwipeTrashMovement leadingTrash = null;

        float leadingX = -9999f;


        foreach (var trash in allTrashes)
        {
            if (trash == null)
                continue;


            if (!trash.enabled)
                continue;


            if (!trash.gameObject.activeInHierarchy)
                continue;


            float x = trash.transform.position.x;


            if (x > leadingX)
            {
                leadingX = x;

                leadingTrash = trash;
            }
        }


        return leadingTrash;
    }

    // 쓰레기를 쓰레기통으로 보내기
    private void FlyToBin(
        SwipeTrashMovement trash,
        bool toRecycleBin
    )
    {
        if (trash == null)
            return;


        // 혹시라도 이미 이동 중이면 차단
        if (isFlying)
            return;


        // 대상 쓰레기 지정

        currentTargetTrash = trash;

        isFlying = true;


        // 이동 중에는 SwipeTrashMovement 정지
        currentTargetTrash.enabled = false;


        // GrabItem 정지
        var grabComponent =
            currentTargetTrash.GetComponent("GrabItem")
            as MonoBehaviour;


        if (grabComponent != null)
        {
            grabComponent.enabled = false;
        }


        // Rigidbody 정지
        Rigidbody2D rb =
            currentTargetTrash.GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
        }


        // Raycast 무시
        currentTargetTrash.gameObject.layer = 2;


        // 쓰레기통 찾기
        if (leftBinObj == null)
        {
            leftBinObj =
                GameObject.Find("RecycleTrashCan");
        }


        if (rightBinObj == null)
        {
            rightBinObj =
                GameObject.Find("GeneralTrashCan");
        }

        // 재활용 쓰레기통
        if (toRecycleBin)
        {
            currentTargetBinObj = leftBinObj;


            if (leftBinObj != null)
            {
                targetBinPosition = new Vector3(
                    leftBinObj.transform.position.x,
                    leftBinObj.transform.position.y,
                    currentTargetTrash.transform.position.z
                );
            }
            else
            {
                targetBinPosition = new Vector3(
                    -6f,
                    -1.8f,
                    currentTargetTrash.transform.position.z
                );
            }
        }


        // 일반 쓰레기통
        else
        {
            currentTargetBinObj = rightBinObj;


            if (rightBinObj != null)
            {
                targetBinPosition = new Vector3(
                    rightBinObj.transform.position.x,
                    rightBinObj.transform.position.y,
                    currentTargetTrash.transform.position.z
                );
            }
            else
            {
                targetBinPosition = new Vector3(
                    6f,
                    -1.8f,
                    currentTargetTrash.transform.position.z
                );
            }
        }
    }
}