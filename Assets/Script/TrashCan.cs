using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("이 쓰레기통이 정답으로 처리할 태그 입력 (Recyclable 또는 GeneralTrash)")]
    public string correctTag; 

    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 쓰레기의 태그가 이 쓰레기통의 정답 태그와 똑같은지 검사
        if (other.CompareTag(correctTag))
        {
            Debug.Log($"★정답! [{correctTag}] 쓰레기를 올바른 쓰레기통에 넣었습니다! 점수 +100점 예정!");
        }
        else
        {
            Debug.Log($"❌오답! 잘못된 쓰레기통에 넣었습니다! 점수 감점 및 안내창 띄울 예정!");
        }

        // 판정이 끝난 쓰레기는 화면에서 삭제
        Destroy(other.gameObject);
    }
}