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

    // 테라리움 이동
    public void OnClickTerrarium()
    {
        SceneManager.LoadScene("Terrarium");
    }

    //테라리움 상점 이동
    public void OnClickStore()
    {
        SceneManager.LoadScene("TerrariumStore");
    }
}
