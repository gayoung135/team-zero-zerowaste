using UnityEngine;
using UnityEngine.SceneManagement; // 화면을 이동하기 위해 필요한 도구입니다.

public class GameOverController : MonoBehaviour
{
    // [REPLAY] 다시하기 버튼을 누르면 실행될 기능
    public void OnClickReplay()
    {
        Time.timeScale = 1f; // 0초가 되면서 멈췄던 게임을 다시 흐르게 합니다.
        
        // 현재 인게임 화면(CatchandDrop)을 처음부터 다시 시작합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // [EXIT] 나가기 버튼을 누르면 실행될 기능
    public void OnClickExit()
    {
        Time.timeScale = 1f; // 멈췄던 게임을 다시 흐르게 풀어주고!

        // 메인 화면과 선택 화면이 있는 'StartScene'으로 돌아갑니다.
        SceneManager.LoadScene("StartScene");
    }

    // [TERRARIUM] 테라리움 가기 버튼을 누르면 실행될 기능 (방학용 미리 만들기)
    public void OnClickTerrarium()
    {
        // 지금은 작동 안 하고, 잘 눌리는지 콘솔창에 로그만 띄웁니다.
        Debug.Log("테라리움 가기 버튼 클릭됨! (방학 때 구현할 예정입니다.)");
    }
}