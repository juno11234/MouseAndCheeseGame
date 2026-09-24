using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 화면의 Start·Exit 버튼 동작을 제공하는 클래스. Canvas에 부착한다
/// </summary>
public class TitleUI : MonoBehaviour
{
    [SerializeField] private string _ingameSceneName = "02_Ingame";

    /// <summary>
    /// 인게임 씬을 로드해 게임을 시작한다. Start_Button의 onClick에 연결한다
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(_ingameSceneName);
    }

    /// <summary>
    /// 게임을 종료한다. 에디터에서는 Application.Quit이 동작하지 않아 플레이 모드를 정지시킨다.
    /// Exit_Button의 onClick에 연결한다
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
