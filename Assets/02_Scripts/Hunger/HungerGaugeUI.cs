using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HungerController의 배고픔 비율을 UI Slider에 반영해 시각적으로 표시하는 컴포넌트.
/// </summary>
public class HungerGaugeUI : MonoBehaviour
{
    [SerializeField] private HungerController _hungerController;
    [SerializeField] private Slider _slider;

    /// <summary>
    /// HungerController의 변경 이벤트를 구독하고, 현재 배고픔 비율을 슬라이더에 즉시 반영한다
    /// </summary>
    private void OnEnable()
    {
        _hungerController.OnHungerChanged += HandleHungerChanged;
        _slider.value = _hungerController.HungerRatio;
    }

    /// <summary>
    /// 컴포넌트가 비활성화될 때 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        _hungerController.OnHungerChanged -= HandleHungerChanged;
    }

    /// <summary>
    /// 배고픔 비율이 바뀔 때마다 슬라이더 값을 갱신한다
    /// </summary>
    private void HandleHungerChanged(float hungerRatio)
    {
        _slider.value = hungerRatio;
    }
}
