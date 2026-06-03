using UnityEngine;

/*주석 추가함*/
// MediaPipe 비전 태스크로부터 수신한 양손의 키포인트를 분석하여 
// 인게임 오브젝트의 잡기(Grab), 드래그(Drag), 놓기(Drop) 상호작용을 처리하는 매니저 클래스
public class HandManager : MonoBehaviour
{
    public static HandManager Instance; // 어디서나 손 데이터 인터페이스에 접근할 수 있도록 보장하는 글로벌 전역 싱글톤 인스턴스
    public Camera mainCamera; // 화면 좌표(Screen Space)를 게임 월드 좌표(World Space)로 변환하기 위한 메인 레퍼런스 카메라

    private bool[] isDragging = new bool[2]; // 양손(0: 왼손, 1: 오른손) 각각 현재 오브젝트를 드래그 중인지 여부를 저장하는 플래그 배열
    private Transform[] targetObject = new Transform[2]; // 양손 각각 현재 고정하여 움켜쥐고 있는 타겟 물체의 트랜스폼 배열
    private Vector2[,] hand = new Vector2[2, 5]; // 양손 각각의 주요 관절 포인트 5개(엄지, 검지, 약지, 손목, 중지마디)의 정규화 좌표를 보관하는 2차원 배열
    private bool[] hasNewData = new bool[2]; // MediaPipe 백엔드로부터 새로운 손 관절 데이터가 유입되었는지 판단하는 업데이트 감시 플래그 배열

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60; //임시 프레임제한
    }
    
    /*주석 추가함*/
    // 매 프레임 왼손/오른손을 순회하며, 
    // MediaPipe 스레드로부터 새롭게 계측 및 갱신된 비주얼 패킷 데이터가 존재할 경우 상호작용 연산을 실행
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if (!hasNewData[i]) continue;
            hasNewData[i] = false; // 플래그 소비(Reset)
            ProcessHand(i); // 손 모양 인식 및 물체 물리 연산 처리
        }
    }

    /*주석 추가함*/
    // 외부 MediaPipe 결과 처리부(Output 패킷 핸들러)에서 추출한 핵심 5개 관절의 2D 정규화 좌표를 
    // 본 매니저 인프라에 주입하는 메서드
    public void UpdateHandData(
        int handIndex,
        float thumbX, float thumbY,
        float indexX, float indexY,
        float ringX, float ringY,
        float wristX, float wristY,
        float middleX, float middleY)
    {
        hand[handIndex, 0] = new Vector2(thumbX, thumbY); // 0: 엄지 끝 (Thumb Tip)
        hand[handIndex, 1] = new Vector2(indexX, indexY); // 1: 검지 끝 (Index Tip)
        hand[handIndex, 2] = new Vector2(ringX, ringY); // 2: 약지 끝 (Ring Tip)
        hand[handIndex, 3] = new Vector2(wristX, wristY); // 3: 손목 중심 (Wrist)
        hand[handIndex, 4] = new Vector2(middleX, middleY); // 4: 중지 기저 마디 (Middle MCP)
        hasNewData[handIndex] = true; // 새로운 데이터 도달 완료 플래그 활성화
    }

    /*주석 추가함*/
    // 특정 손 인덱스의 제스처 상태(Pinch/Fist)를 계산하고, 
    // 유니티 2D 물리 엔진과 충돌체(Collider2D)를 연동해 조작 로직을 수행
    private void ProcessHand(int handIndex)
    {
        Vector2[] handPoint = new Vector2[5];
        for (int i = 0; i < 5; i++) handPoint[i] = hand[handIndex, i];

        // 제스처 판정 알고리즘: 관절 간의 2차원 유클리드 거리를 기반으로 임계값 미만일 때 잡기 동작으로 인정
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

        /*주석 추가함*/
        // MediaPipe의 뷰포트 정규화 좌표(0~1)를 유니티 스크린 해상도 픽셀 좌표로 변환하고, 
        // 이를 다시 메인 카메라를 통해 게임 월드 좌표계로 투영
        Vector3 screenPos;
        if (isPinching) screenPos = new Vector3(handPoint[1].x * Screen.width,(1 - handPoint[1].y) * Screen.height,10f); //집을 경우 검지 끝 위치로 오브젝트 이동
        else screenPos = new Vector3(handPoint[4].x * Screen.width,(1 - handPoint[4].y) * Screen.height,10f); //쥘 경우 중지-손바닥 연결 마디 부분으로 이동
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0; // 2D 인터랙션 환경에 최적화되도록 Z축 값 거세

        if (!isDragging[handIndex]) //잡기
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null)
            {
                // 멀티 제어 예외 처리: 반대쪽 손이 이미 해당 오브젝트를 선점하여 조작 중인지 상호 교차 검증을 수행
                bool heldByOther = false;
                for (int i = 0; i < 2; i++)
                {
                    if (i == handIndex) continue;
                    if (targetObject[i] == hit.transform) 
                    {
                        heldByOther = true; // 다른 손이 이미 소유권을 가진 물체라면 인터셉트 방지
                        break; 
                    }
                }

                // 타 손의 독점 상태가 아니라면 해당 물체의 소유권을 획득하고 GrabItem 컴포넌트에 잡기 시작 시그널을 통보
                if (!heldByOther)
                {
                    targetObject[handIndex] = hit.transform;
                    isDragging[handIndex] = true;
                    hit.GetComponent<GrabItem>()?.OnGrab();
                }
            }
        }

        /*주석 추가함*/
        // 물체 선점이 완료되어 드래그 상태가 지속되는 동안, 
        // 매 프레임 갱신되는 마우스/손가락 월드 좌표를 추종하도록 이동 시그널을 주입
        if (isDragging[handIndex] && targetObject[handIndex] != null) targetObject[handIndex].GetComponent<GrabItem>()?.OnHandMove(worldPos);
    }
}