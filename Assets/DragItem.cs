using UnityEngine;

public class DragItem : MonoBehaviour
{
    //해당 스크립트는 쓰레기에 들어가야 합니다(collider 필요)
    //카메라 projection이 Orthographic이어야 합니다
    private Vector3 offset;
    private bool isDragging = false;

    private void OnMouseDown()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        offset = transform.position - mousePos;
        isDragging = true;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos + offset;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}
