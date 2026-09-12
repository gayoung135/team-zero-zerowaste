using UnityEngine;
using UnityEngine.UI;

public class ItemControlUI : MonoBehaviour
{
    [Header("--- Buttons ---")]
    public Button confirmButton; // 확인
    public Button storeButton;   // 보관
    public Button cancelButton;  // 취소
    public Button rotateButton;  // 회전

    private PlaceableItem currentTargetItem;

    // 아이템 선택 시 UI 호출 및 위치 추적
    public void ShowUI(PlaceableItem item)
    {
        currentTargetItem = item;
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void HideUI()
    {
        currentTargetItem = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentTargetItem != null)
        {
            UpdatePosition();
        }
    }

    // 아이템 위치에 맞게 UI 이동 (월드 좌표 -> 스크린 좌표 전환)
    private void UpdatePosition()
    {
        if (currentTargetItem == null) return;

        // 2D/3D 월드 좌표를 화면 UI 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTargetItem.transform.position);
        transform.position = screenPos;
    }
}