/// <summary>
/// IHungerObj를 구현하는 오브젝트의 종류. 장애물/음식(사과/치즈)을 구분한다.
/// </summary>
public enum HungerObjType
{
    Obstacle,
    Apple,
    Cheese
}

/// <summary>
/// 배고픔 변경을 유발하는 오브젝트(음식/장애물)가 구현하는 인터페이스.
/// 음식·장애물 프리팹은 이 인터페이스를 구현한 컴포넌트를 부착해 HungerController와 소통한다.
/// </summary>
public interface IHungerObj
{
    /// <summary>
    /// 이 오브젝트의 종류(장애물/사과/치즈). 배고픔 변경 계산에는 쓰이지 않지만, 종류 구분이 필요한
    /// 다른 Feature(예: 점수 시스템)가 GetComponent로 이 인터페이스를 얻어 읽을 수 있다
    /// (6-12절 근거 — 이전에는 종류를 알 방법이 없었던 문제를 이 프로퍼티가 부분적으로 해소한다)
    /// </summary>
    HungerObjType HungerObjType { get; }

    /// <summary>
    /// 이 오브젝트와 접촉했을 때 배고픔에 적용할 변경량을 그대로 들고 있는다.
    /// 장애물은 음수, 음식은 양수를 값 자체에 이미 반영해 저장한다(별도 가공 함수 없음).
    /// 치즈의 "즉시 풀 회복"은 이 값을 MaxHunger보다 충분히 큰 양수로 구성해두면, HungerController의
    /// clamp가 자연스럽게 풀 회복으로 만들어준다(6-4절 근거).
    /// </summary>
    float Amount { get; }
}
