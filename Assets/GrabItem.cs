using UnityEngine;

public class GrabItem : MonoBehaviour
{
    //해당 스크립트는 쓰레기에 들어가야 합니다(collider 필요)
    //카메라 projection이 Orthographic이어야 합니다
    private Vector3 offset;
    private bool isDragging = false;

    //마우스 상호작용
    private void OnMouseDown()
    /*주석 추가함*/ 
    //마우스로 쓰레기를 클릭하는 순간 실행되는 메서드임.
    //클릭한 마우스의 좌표를 계산해서 드래그 상태를 활성화함.
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        offset = transform.position - mousePos;
        isDragging = true;
    }

    private void OnMouseDrag()
    /*주석 추가함*/  
    //마우스를 누른 채로 움직일때 실시간으로 실행되는 메서드임.
    //마우스 위치에 맞춰 쓰레기의 위치를 이동시킨다.
    {
        if (!isDragging) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos + offset;
    }

    private void OnMouseUp()
    /*주석 추가함*/ 
    //누르고 있던 마우스 버튼을 떼는 순간 실행되는 메서드임.
    //드래그 상태를 해제해,쓰레기를 제자리에 놓음.
    {
        isDragging = false;
    }

    //웹캠 상호작용
    public void OnGrab() //잡을때 신호
    /*주석 추가함*/
    //사용자의 '손 쥐기' 제스처를 인식했을 때 외부 스크립트에서 호출하는 메서드
    {
        Debug.Log($"{gameObject.name} 잡음");
        isDragging = true;
    }

    public void OnDrop() //놓을때 신호
    /*주석 추가함*/
    //사용자의 '손 펴기' 제스처를 인식했을 때 외부 스크립트에서 호출하는 메서드
    {
        Debug.Log($"{gameObject.name} 놓음");
        isDragging = false;
    }

    public void OnHandMove(Vector3 worldPos) //움직임
    /*주석 추가함*/
    //사용자의 실시간 손 위치(좌표)를 보내줄 때 실행되는 메서드
    {
        if (!isDragging) return;
        transform.position = worldPos;
    }
}

