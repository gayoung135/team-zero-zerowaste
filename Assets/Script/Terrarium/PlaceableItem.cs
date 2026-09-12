using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceableItem : MonoBehaviour
{
    [Header("--- Sprite Settings ---")]
    private SpriteRenderer spriteRenderer;
    private bool isFlipped = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 마우스로 배치된 개구리를 클릭했을 때 실행
    void OnMouseDown()
    {
        // UI 버튼 클릭 중에는 개구리가 다시 선택되지 않도록 차단
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Debug.Log($"<color=yellow>[PlaceableItem] {gameObject.name} 선택됨!</color>");

        if (TerrariumManager.Instance != null)
        {
            TerrariumManager.Instance.SelectPlacedItem(this);
        }
    }

    // 1. Preview / Rotate (좌우 반전 / 미리보기)
    public void PreviewItem()
    {
        if (spriteRenderer != null)
        {
            isFlipped = !isFlipped;
            spriteRenderer.flipX = isFlipped;
            Debug.Log($"[PlaceableItem] Preview - 좌우 반전: {isFlipped}");
        }
    }

    // 2. Save (배치 확정)
    public void SaveItem()
    {
        Debug.Log($"<color=green>[PlaceableItem] {gameObject.name} 배치 완료 확정!</color>");
        // 필요 시 위치 저장 로직 추가 가능
    }

    // 3. Cancel / Store (배치 취소 및 보관)
    public void CancelItem()
    {
        Debug.Log($"<color=red>[PlaceableItem] {gameObject.name} 배치 취소 (삭제 및 보관)</color>");
        
        Destroy(gameObject);

        // EditModeManager의 Empty 문구 갱신
        EditModeManager editManager = FindObjectOfType<EditModeManager>();
        if (editManager != null)
        {
            editManager.UpdateEmptyMessage();
        }
    }
}