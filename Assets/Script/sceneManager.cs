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
    // 테라리움 이동
    public void OnClickTerrarium()
    {
        SceneManager.LoadScene("Terrarium");
    }

    //테라리움 씬에 있는 BackButton 누르면 모드선택 장면으로 이동
    public void OnClickBackButton()
    {
        SceneManager.LoadScene("StartScene");
    }
}
