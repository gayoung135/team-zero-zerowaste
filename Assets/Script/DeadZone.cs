using UnityEngine;

public class DeadZone : MonoBehaviour
{
    // 놓친 쓰레기가 바닥(DeadZone 트리거)에 닿으면 실행
    void OnTriggerEnter2D(Collider2D other)
    {
        // 바닥에 닿은 쓰레기 오브젝트를 메모리에서 깔끔하게 삭제
        Destroy(other.gameObject); 
        Debug.Log("놓친 쓰레기가 바닥에 닿아 파괴되었습니다.");
    }
}