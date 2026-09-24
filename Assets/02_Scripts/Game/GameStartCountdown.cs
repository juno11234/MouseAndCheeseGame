using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 인게임 씬 시작 시 게임을 정지하고 카운트다운을 표시해, 플레이어에게 조작 준비 시간을 준다. Canvas에 부착한다
/// </summary>
public class GameStartCountdown : MonoBehaviour
{
    [SerializeField] private int _countdownSeconds = 3;
    [SerializeField] private TextMeshProUGUI _countdownText;

    /// <summary>
    /// 씬 시작과 동시에 게임을 정지시키고 카운트다운을 시작한다
    /// </summary>
    private void Start()
    {
        Time.timeScale = 0f;
        StartCoroutine(Countdown());
    }

    /// <summary>
    /// 실시간 기준(Time.timeScale의 영향을 받지 않음)으로 1초마다 숫자를 하나씩 줄이다가
    /// 0이 되면 텍스트를 숨기고 게임을 다시 진행시킨다
    /// </summary>
    private IEnumerator Countdown()
    {
        _countdownText.gameObject.SetActive(true);
        for (int remaining = _countdownSeconds; remaining > 0; remaining--)
        {
            _countdownText.text = remaining.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        _countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}
