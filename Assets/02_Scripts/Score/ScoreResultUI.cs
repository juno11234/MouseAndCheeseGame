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