using TMPro;
using UnityEngine;

/// <summary>
/// 플레이 중 매 프레임 현재 점수를 TMP 텍스트에 실시간으로 표시하는 클래스. Canvas에 부착한다
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private ScoreController _scoreController;

    private TMP_Text _text;
    private int _displayedScore = -1;

    /// <summary>
    /// TMP 텍스트 컴포넌트를 가져온다
    /// </summary>
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 충분히 긴 문자열로 TMP 내부 배열을 미리 확보한다
    /// </summary>
    private void Start()
    {
        // TMP는 .text를 넣는 순간이 아니라 프레임 끝 캔버스 갱신 때 파싱하므로, 첫 Update에서 덮이기 전에 즉시 파싱시킨다.
        // TMP 자신의 Awake가 끝나야 동작하므로 Awake가 아닌 Start에서 호출한다
        _text.text = "Score : 9999999";
        _text.ForceMeshUpdate();
    }

    /// <summary>
    /// 점수가 바뀌었을 때만 텍스트를 갱신한다
    /// </summary>
    private void Update()
    {
        int score = _scoreController.CurrentScore;
        if (score == _displayedScore)
        {
            return;
        }

        _displayedScore = score;
        _text.text = "Score : " + score.ToString();
    }
}
