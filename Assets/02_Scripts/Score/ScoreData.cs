using UnityEngine;

/// <summary>
/// 점수 계산에 쓰이는 밸런싱 수치를 담는 임시 스텁 ScriptableObject.
/// 정식 수치는 "데이터 테이블" Feature에서 확정한다(0-1절 참고)
/// </summary>
[CreateAssetMenu(fileName = "ScoreData", menuName = "MouseAndCheese/Score Data")]
public class ScoreData : ScriptableObject
{
    [Header("생존시간 점수")]
    [SerializeField] private float _survivalScorePerSecond = 1f;

    [Header("음식 보너스 점수")]
    [SerializeField] private float _appleScoreBonus = 30f;
    [SerializeField] private float _cheeseScoreBonus = 60f;

    /// <summary>
    /// 생존 1초당 더해지는 점수
    /// </summary>
    public float SurvivalScorePerSecond => _survivalScorePerSecond;

    /// <summary>
    /// 사과 하나를 섭취할 때 더해지는 점수
    /// </summary>
    public float AppleScoreBonus => _appleScoreBonus;

    /// <summary>
    /// 치즈 하나를 섭취할 때 더해지는 점수
    /// </summary>
    public float CheeseScoreBonus => _cheeseScoreBonus;
}
