using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    //씬 이동을 총괄하는 스크립트입니다
    //상황에 맞게 씬 이동 스크립트 작성해서 넣어주세요
    public void moveCatchandDrop()
    {
        SceneManager.LoadScene("CatchandDrop");
    }
}
