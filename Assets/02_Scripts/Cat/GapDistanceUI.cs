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
    /// 같은 오브젝트의 TMP 텍스트를 가져온다
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
        _text.text = "9999999";
        _text.ForceMeshUpdate();
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
