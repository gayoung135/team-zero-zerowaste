using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    //씬 이동을 총괄하는 스크립트입니다
    //상황에 맞게 씬 이동 스크립트 작성해서 넣어주세요

    //첫 화면으로 돌아가기
    public void onClickMain()
    {
        SceneManager.LoadScene("StartScene");
    }

    //Catch&Drop으로 이동
    public void onClickCatchandDrop()
    {
        SceneManager.LoadScene("CatchandDrop");
    }

    //Swipe로 이동
    public void onClickSwipe()
    {
        SceneManager.LoadScene("Swipe");
        
    }

    //다시하기
    public void onClickReplay()
    {        
        // 현재 인게임 화면을 처음부터 다시 시작합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // [TERRARIUM] 테라리움 가기 버튼을 누르면 실행될 기능 (방학용 미리 만들기)
    public void OnClickTerrarium()
    {
        // 지금은 작동 안 하고, 잘 눌리는지 콘솔창에 로그만 띄웁니다.
        Debug.Log("테라리움 가기 버튼 클릭됨! (방학 때 구현할 예정입니다.)");
    }
}
