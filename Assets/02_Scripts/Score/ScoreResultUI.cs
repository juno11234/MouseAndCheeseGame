using TMPro;
using UnityEngine;

/// <summary>
/// 게임오버 시 이번 판 점수와 최고기록을 TMP 텍스트에 표시하고, 신기록이면 배지를 노출하는 클래스. 항상 활성 상태인 오브젝트(Canvas)에 부착한다
/// </summary>
public class ScoreResultUI : MonoBehaviour
{
    [SerializeField] private ScoreController _scoreController;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private GameObject _newRecordBadge;

    /// <summary>
    /// 충분히 긴 문자열로 내부 배열을 미리 할당한다. TMP는 한 번 늘어난 내부 배열을 줄이지 않으므로,
    /// 실제 점수를 표시할 때 자릿수 때문에 배열을 다시 할당하며 GC가 발생하는 것을 막을 수 있다
    /// </summary>
    private void Awake()
    {
        _scoreText.text = "9999999";
        _highScoreText.text = "9999999";
    }

    /// <summary>
    /// ScoreController의 점수 확정 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        _scoreController.OnScoreFinalized += HandleScoreFinalized;
    }

    /// <summary>
    /// 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _scoreController.OnScoreFinalized -= HandleScoreFinalized;
    }

    /// <summary>
    /// 확정된 이번 판 점수와 최고기록을 텍스트에 반영하고, 신기록이면 배지를 활성화한다
    /// </summary>
    private void HandleScoreFinalized()
    {
        _scoreText.text = _scoreController.CurrentScore.ToString();
        _highScoreText.text = _scoreController.HighScore.ToString();
        _newRecordBadge.SetActive(_scoreController.IsNewHighScore);
    }
}