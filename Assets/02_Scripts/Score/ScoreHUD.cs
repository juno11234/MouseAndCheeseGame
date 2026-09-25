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
    /// TMP 텍스트 컴포넌트를 가져오고, 충분히 긴 문자열로 내부 배열을 미리 할당한다.
    /// TMP는 한 번 늘어난 내부 배열을 줄이지 않으므로, 이후 자릿수가 늘어날 때마다 배열을 다시
    /// 할당하며 GC가 발생하는 것을 막을 수 있다
    /// </summary>
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _text.text = "Score : 9999999";
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
