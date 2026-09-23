using System;
using UnityEngine;

/// <summary>
/// 생존시간과 음식 섭취를 합산해 점수를 계산하고, 게임오버 시 로컬 최고기록을 갱신·저장하는 클래스. Player GameObject에 부착한다
/// </summary>
public class ScoreController : MonoBehaviour
{
    private const string HighScoreKey = "MouseAndCheese_HighScore";

    [SerializeField] private CatController _catController;
    [SerializeField] private ScoreData _scoreData;

    private float _rawScore;
    private int _highScore;
    private bool _isGameOver;

    /// <summary>
    /// 점수가 확정(게임오버, 최고기록 비교·저장 완료)되면 한 번 발생하는 이벤트
    /// </summary>
    public event Action OnScoreFinalized;

    /// <summary>
    /// 지금까지 누적된 점수(정수로 반올림)
    /// </summary>
    public int CurrentScore => Mathf.RoundToInt(_rawScore);

    /// <summary>
    /// 로컬에 저장된 최고기록
    /// </summary>
    public int HighScore => _highScore;

    /// <summary>
    /// 이번 판 점수가 최고기록을 갱신했는지 여부(게임오버 확정 후에만 유효)
    /// </summary>
    public bool IsNewHighScore { get; private set; }

    /// <summary>
    /// 저장된 최고기록을 불러온다
    /// </summary>
    private void Awake()
    {
        _highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    /// <summary>
    /// 포획 이벤트와 음식 소비 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        _catController.OnPlayerCaught += HandlePlayerCaught;
        Food.OnAnyConsumed += HandleFoodConsumed;
    }

    /// <summary>
    /// 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _catController.OnPlayerCaught -= HandlePlayerCaught;
        Food.OnAnyConsumed -= HandleFoodConsumed;
    }

    /// <summary>
    /// 게임오버 전까지 매 프레임 생존시간을 점수에 누적한다
    /// </summary>
    private void Update()
    {
        if (_isGameOver)
        {
            return;
        }

        _rawScore += _scoreData.SurvivalScorePerSecond * Time.deltaTime;
    }

    /// <summary>
    /// 소비된 음식이 사과·치즈면 해당 보너스를 점수에 더한다. 장애물은 점수에 영향을 주지 않는다(0절 근거)
    /// </summary>
    private void HandleFoodConsumed(Food food)
    {
        if (_isGameOver)
        {
            return;
        }

        switch (food.HungerObjType)
        {
            case HungerObjType.Apple:
                _rawScore += _scoreData.AppleScoreBonus;
                break;
            case HungerObjType.Cheese:
                _rawScore += _scoreData.CheeseScoreBonus;
                break;
        }
    }

    /// <summary>
    /// 점수 누적을 멈추고, 최고기록을 갱신했다면 저장한 뒤 확정 이벤트를 발생시킨다
    /// </summary>
    private void HandlePlayerCaught()
    {
        _isGameOver = true;
        IsNewHighScore = CurrentScore > _highScore;
        if (IsNewHighScore)
        {
            _highScore = CurrentScore;
            PlayerPrefs.SetInt(HighScoreKey, _highScore);
            PlayerPrefs.Save();
        }

        OnScoreFinalized?.Invoke();
    }
}
