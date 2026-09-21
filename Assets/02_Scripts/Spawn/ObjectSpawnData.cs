using UnityEngine;

/// <summary>
/// 음식·장애물 랜덤 생성의 밸런싱 수치를 담는 임시 스텁 ScriptableObject. 정식 수치는 데이터 테이블 Feature에서 확정한다
/// </summary>
[CreateAssetMenu(fileName = "ObjectSpawnData", menuName = "MouseAndCheese/Object Spawn Data")]
public class ObjectSpawnData : ScriptableObject
{
    [Header("생성 주기와 확률")]
    [SerializeField] private float _spawnInterval = 1f;
    [SerializeField] private float _appleSpawnWeight = 9f;
    [SerializeField] private float _cheeseSpawnWeight = 1f;
    [SerializeField] private float _trapSpawnWeight = 2f;
    [SerializeField] private int _initialSpawnAttempts = 5;

    [Header("동시 존재 최대 수(풀 크기)")]
    [SerializeField] private int _maxActiveApples = 20;
    [SerializeField] private int _maxActiveCheeses = 2;
    [SerializeField] private int _maxActiveTraps = 6;

    [Header("최소 간격")]
    [SerializeField] private float _minObjectDistance = 4f;
    [SerializeField] private float _minDistanceFromPlayer = 6f;

    [Header("치즈와 쥐덫")]
    [SerializeField] private float _cheeseLifetime = 15f;
    [SerializeField] private float _trapMinDistanceFromCheese = 1.5f;
    [SerializeField] private float _trapMaxDistanceFromCheese = 3f;

    [Header("높이 구간")]
    [SerializeField] private float _elevatedHeight = 1f;
    [SerializeField] private float _elevatedWeightMultiplier = 3f;
    [SerializeField] private bool _restrictHeight = false;
    [SerializeField] private float _restrictedMinHeight = 0f;
    [SerializeField] private float _restrictedMaxHeight = 0f;

    [Header("배치")]
    [SerializeField] private float _spawnHeightOffset = 0.08f;
    [SerializeField] private int _maxPlacementAttempts = 10;
    [SerializeField] private float _navMeshSnapDistance = 0.5f;

    /// <summary>
    /// 생성을 시도하는 주기(초)
    /// </summary>
    public float SpawnInterval => _spawnInterval;

    /// <summary>
    /// 생성 시도마다 사과가 선택될 상대 가중치
    /// </summary>
    public float AppleSpawnWeight => _appleSpawnWeight;

    /// <summary>
    /// 생성 시도마다 치즈가 선택될 상대 가중치
    /// </summary>
    public float CheeseSpawnWeight => _cheeseSpawnWeight;

    /// <summary>
    /// 생성 시도마다 독립 쥐덫이 선택될 상대 가중치
    /// </summary>
    public float TrapSpawnWeight => _trapSpawnWeight;

    /// <summary>
    /// 게임 시작 시 한꺼번에 시도하는 생성 횟수
    /// </summary>
    public int InitialSpawnAttempts => _initialSpawnAttempts;

    /// <summary>
    /// 사과의 동시 존재 최대 수이자 풀 크기
    /// </summary>
    public int MaxActiveApples => _maxActiveApples;

    /// <summary>
    /// 치즈의 동시 존재 최대 수이자 풀 크기
    /// </summary>
    public int MaxActiveCheeses => _maxActiveCheeses;

    /// <summary>
    /// 쥐덫(독립 쥐덫과 치즈 동반 쥐덫 합산)의 동시 존재 최대 수이자 풀 크기
    /// </summary>
    public int MaxActiveTraps => _maxActiveTraps;

    /// <summary>
    /// 모든 오브젝트 사이에 유지할 최소 거리(3D)
    /// </summary>
    public float MinObjectDistance => _minObjectDistance;

    /// <summary>
    /// 플레이어로부터 이 거리 안에는 생성하지 않는다
    /// </summary>
    public float MinDistanceFromPlayer => _minDistanceFromPlayer;

    /// <summary>
    /// 치즈가 생성된 뒤 사라지기까지의 시간(초)
    /// </summary>
    public float CheeseLifetime => _cheeseLifetime;

    /// <summary>
    /// 동반 쥐덫이 치즈로부터 떨어져야 하는 최소 거리
    /// </summary>
    public float TrapMinDistanceFromCheese => _trapMinDistanceFromCheese;

    /// <summary>
    /// 동반 쥐덫이 치즈로부터 떨어질 수 있는 최대 거리
    /// </summary>
    public float TrapMaxDistanceFromCheese => _trapMaxDistanceFromCheese;

    /// <summary>
    /// 이 월드 Y 높이 이상의 NavMesh 표면은 생성 확률이 올라간다
    /// </summary>
    public float ElevatedHeight => _elevatedHeight;

    /// <summary>
    /// 높은 표면(ElevatedHeight 이상)의 면적에 곱하는 생성 가중치 배율
    /// </summary>
    public float ElevatedWeightMultiplier => _elevatedWeightMultiplier;

    /// <summary>
    /// true이면 제한 높이 구간(RestrictedMinHeight~RestrictedMaxHeight)에는 생성하지 않는다
    /// </summary>
    public bool RestrictHeight => _restrictHeight;

    /// <summary>
    /// 생성을 제한하는 높이 구간의 최소 Y
    /// </summary>
    public float RestrictedMinHeight => _restrictedMinHeight;

    /// <summary>
    /// 생성을 제한하는 높이 구간의 최대 Y
    /// </summary>
    public float RestrictedMaxHeight => _restrictedMaxHeight;

    /// <summary>
    /// NavMesh 표면 위로 오브젝트를 띄우는 높이
    /// </summary>
    public float SpawnHeightOffset => _spawnHeightOffset;

    /// <summary>
    /// 조건을 만족하는 위치를 찾기 위해 한 번의 생성에서 시도하는 최대 횟수
    /// </summary>
    public int MaxPlacementAttempts => _maxPlacementAttempts;

    /// <summary>
    /// 뽑은 지점을 NavMesh 위로 스냅할 때 허용하는 최대 거리
    /// </summary>
    public float NavMeshSnapDistance => _navMeshSnapDistance;
}
