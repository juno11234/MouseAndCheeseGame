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
    /// 게임 시작 시 마우스 커서를 숨긴다
    /// </summary>
    private void Start()
    {
        Cursor.visible = false;
    }

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
    /// 포획되면 GameOver 패널을 활성화하고 마우스 커서를 다시 보이게 한 뒤 게임을 정지시킨다.
    /// Time.timeScale만 0으로 바꾸므로 UI 입력(버튼 클릭)은 영향받지 않고 그대로 동작한다
    /// </summary>
    private void HandlePlayerCaught()
    {
        _gameOverPanel.SetActive(true);
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 정지시켰던 게임 속도를 되돌리고 현재 씬을 다시 로드해 게임을 재시작한다. Re_Button의 onClick에 연결한다
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
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
