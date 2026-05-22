using UnityEngine;

public class ItemController : MonoBehaviour
{
    // 쓰레기 아이템이 하늘에서 아래로 떨어지는 기본 낙하 속도입니다.
    public float dropSpeed = -0.015f; 

    void Update()
    {
        // 1. 매 프레임마다 지정된 속도만큼 Y축 아래 방향으로 아이템을 일정하게 하강시킵니다.
        transform.Translate(0, this.dropSpeed, 0);

        // 2. 아이템이 화면 하단(Y 좌표 -5.0f 이하)을 벗어나면 메모리 관리를 위해 오브젝트를 스스로 파괴합니다.
        if (transform.position.y < -5.0f)
        {
            Destroy(gameObject);
        }
    }
}