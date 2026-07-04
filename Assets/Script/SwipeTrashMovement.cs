using UnityEngine;

public class SwipeTrashMovement : MonoBehaviour  //이 스크립트 삭제하시면 안돼요
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float destroyXThreshold;
    private bool isLeftToRight;

    void Awake()
    {
        // [1. 공용 프리팹 낙하 관성 차단]
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; 
            rb.gravityScale = 0f; 
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // [2. 캐치앤드롭 전용 낙하 스크립트 강제 차단 강수]
        // 프리팹에 ItemController가 붙어있다면, 스와이프 모드 동안은 작동하지 못하게 꺼버립니다.
        MonoBehaviour itemController = GetComponent("ItemController") as MonoBehaviour;
        if (itemController != null)
        {
            itemController.enabled = false; 
            //Debug.Log($"[Swipe] 공용 프리팹의 낙하 스크립트(ItemController)를 감지하여 비활성화했습니다.");
            // 캐치앤드롭용 낙하 스크립트가 켜져 있으면 에러 나니까 끄는 코드입니다. (콘솔 도배돼서 로그만 잠금)
        }
    }

    public void Setup(Vector3 direction, float speed, float destroyX, bool leftToRight)
    {
        this.moveDirection = direction;
        this.moveSpeed = speed;
        this.destroyXThreshold = destroyX;
        this.isLeftToRight = leftToRight;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        // 지정된 방향과 속도로 가로 이동
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // 화면 밖 도달 시 소멸
        if (isLeftToRight)
        {
            if (transform.position.x > destroyXThreshold)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (transform.position.x < destroyXThreshold)
            {
                Destroy(gameObject);
            }
        }
    }
}