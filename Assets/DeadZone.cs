using UnityEngine;

public class DeadZone : MonoBehaviour
{
    // 무언가 트리거 영역(쓰레기통)에 들어왔을 때 유니티가 자동으로 실행해주는 함수
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 부딪힌 오브젝트의 태그 이름이 "Trash" 라면
        if (other.CompareTag("Trash"))
        {
            // 그 오브젝트(놓친 쓰레기)를 게임 세상에서 파괴(삭제)합니다.
            Destroy(other.gameObject);
            
            // 유니티 왼쪽 아래 Console 창에 잘 작동하는지 기록을 남깁니다.(잘 되나 테스트용)
            Debug.Log("놓친 쓰레기가 쓰레기통에 닿아 정상적으로 파괴되었습니다!");
        }
    }
}