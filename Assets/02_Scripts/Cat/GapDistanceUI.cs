using TMPro;
using UnityEngine;

/// <summary>
/// CatController의 갭(고양이와의 직선거리)을 자연수로 TMP 텍스트에 표시하는 컴포넌트
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class GapDistanceUI : MonoBehaviour
{
    [SerializeField] private CatController _catController;

    private TMP_Text _text;
    private int _displayedDistance = -1;

    /// <summary>
    /// 같은 오브젝트의 TMP 텍스트를 가져오고, 충분히 긴 문자열로 내부 배열을 미리 할당한다.
    /// TMP는 한 번 늘어난 내부 배열을 줄이지 않으므로, 이후 자릿수가 늘어날 때마다 배열을 다시
    /// 할당하며 GC가 발생하는 것을 막을 수 있다
    /// </summary>
    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _text.text = "9999999";
    }

    /// <summary>
    /// 갭을 자연수로 바꿔 값이 달라졌을 때만 텍스트를 갱신한다
    /// </summary>
    private void Update()
    {
        // OnGapChanged는 비행 중(Frozen)과 포획 후에는 발생하지 않아, 그 구간에도 갱신되도록 매 프레임 갭을 읽는다
        int distance = Mathf.RoundToInt(_catController.Gap);
        if (distance == _displayedDistance)
        {
            return;
        }

        _displayedDistance = distance;
        _text.text = distance.ToString();
    }
}
