using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 고양이에게 포획되면 GameOver 패널을 활성화하고, 다시시작·종료 버튼 동작을 제공하는 클래스. Canvas에 부착한다
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private CatController _catController;
    [SerializeField] private GameObject _gameOverPanel;

    /// <summary>
    /// CatController의 포획 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        _catController.OnPlayerCaught += HandlePlayerCaught;
    }

    /// <summary>
    /// 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _catController.OnPlayerCaught -= HandlePlayerCaught;
    }

    /// <summary>
    /// 포획되면 GameOver 패널을 활성화한다
    /// </summary>
    private void HandlePlayerCaught()
    {
        _gameOverPanel.SetActive(true);
    }

    /// <summary>
    /// 현재 씬을 다시 로드해 게임을 재시작한다. Re_Button의 onClick에 연결한다
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// 게임을 종료한다. 에디터에서는 Application.Quit이 동작하지 않아 플레이 모드를 정지시킨다.
    /// Quit_Button의 onClick에 연결한다
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
