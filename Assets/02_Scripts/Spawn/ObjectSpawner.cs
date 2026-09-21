using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh 위 무작위 지점에 사과·치즈·쥐덫을 오브젝트 풀로 생성·회수한다. 빈 GameObject에 부착한다
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private ObjectSpawnData _spawnData;
    [SerializeField] private Food _applePrefab;
    [SerializeField] private Food _cheesePrefab;
    [SerializeField] private Food _trapPrefab;
    [SerializeField] private int _agentTypeId;
    [SerializeField] private int _areaMask = NavMesh.AllAreas;

    private readonly Dictionary<Food, FoodPool> _activeFoods = new Dictionary<Food, FoodPool>();
    private readonly List<ExpiringCheese> _expiringCheeses = new List<ExpiringCheese>();
    private NavMeshRandomPointSampler _sampler;
    private FoodPool _applePool;
    private FoodPool _cheesePool;
    private FoodPool _trapPool;
    private float _spawnTimer;

    /// <summary>
    /// 치즈와 그 만료 시각을 묶은 추적 정보
    /// </summary>
    private sealed class ExpiringCheese
    {
        /// <summary>
        /// 만료를 추적하는 치즈
        /// </summary>
        public Food Cheese;

        /// <summary>
        /// 치즈가 만료되는 Time.time 시각
        /// </summary>
        public float ExpireTime;
    }

    /// <summary>
    /// 이미 생성된 오브젝트의 소비 이벤트를 구독한다
    /// </summary>
    private void OnEnable()
    {
        foreach (Food food in _activeFoods.Keys)
        {
            food.OnConsumed += HandleFoodConsumed;
        }
    }

    /// <summary>
    /// 이미 생성된 오브젝트의 소비 이벤트 구독을 해제한다
    /// </summary>
    private void OnDisable()
    {
        foreach (Food food in _activeFoods.Keys)
        {
            food.OnConsumed -= HandleFoodConsumed;
        }
    }

    /// <summary>
    /// NavMesh 샘플러와 종류별 풀을 만들고 시작 시 일괄 생성을 시도한다
    /// </summary>
    private void Start()
    {
        _sampler = new NavMeshRandomPointSampler(_agentTypeId, _areaMask, _spawnData);

        _applePool = new FoodPool(_applePrefab, transform, _spawnData.MaxActiveApples);
        _cheesePool = new FoodPool(_cheesePrefab, transform, _spawnData.MaxActiveCheeses);
        _trapPool = new FoodPool(_trapPrefab, transform, _spawnData.MaxActiveTraps);
        _applePool.Prewarm();
        _cheesePool.Prewarm();
        _trapPool.Prewarm();

        for (int i = 0; i < _spawnData.InitialSpawnAttempts; i++)
        {
            TrySpawnRandom();
        }
    }

    /// <summary>
    /// 만료된 치즈를 회수하고 생성 주기마다 생성을 시도한다
    /// </summary>
    private void Update()
    {
        ExpireCheeses();

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnData.SpawnInterval)
        {
            return;
        }

        _spawnTimer = 0f;
        TrySpawnRandom();
    }

    /// <summary>
    /// 가중치로 사과·치즈·쥐덫 중 하나를 골라 생성을 시도한다
    /// </summary>
    private void TrySpawnRandom()
    {
        float appleWeight = _spawnData.AppleSpawnWeight;
        float cheeseWeight = _spawnData.CheeseSpawnWeight;
        float roll = Random.value * (appleWeight + cheeseWeight + _spawnData.TrapSpawnWeight);
        if (roll < appleWeight)
        {
            TrySpawnApple();
        }
        else if (roll < appleWeight + cheeseWeight)
        {
            TrySpawnCheeseWithTrap();
        }
        else
        {
            TrySpawnTrap();
        }
    }

    /// <summary>
    /// 상한을 넘지 않고 조건을 만족하는 NavMesh 위치를 찾으면 사과를 생성한다
    /// </summary>
    private void TrySpawnApple()
    {
        if (_applePool.ActiveCount >= _spawnData.MaxActiveApples)
        {
            return;
        }

        if (TryFindSpawnPosition(out Vector3 position))
        {
            SpawnFood(_applePool, position);
        }
    }

    /// <summary>
    /// 상한을 넘지 않고 조건을 만족하는 NavMesh 위치를 찾으면 치즈와 무관한 독립 쥐덫을 생성한다
    /// </summary>
    private void TrySpawnTrap()
    {
        if (_trapPool.ActiveCount >= _spawnData.MaxActiveTraps)
        {
            return;
        }

        if (TryFindSpawnPosition(out Vector3 position))
        {
            SpawnFood(_trapPool, position);
        }
    }

    /// <summary>
    /// 치즈 위치와 그 근처 동반 쥐덫 위치를 모두 찾았고 쥐덫 풀에 자리가 있을 때만 둘을 함께 생성한다
    /// </summary>
    private void TrySpawnCheeseWithTrap()
    {
        if (_cheesePool.ActiveCount >= _spawnData.MaxActiveCheeses)
        {
            return;
        }

        if (_trapPool.ActiveCount >= _spawnData.MaxActiveTraps)
        {
            return;
        }

        if (TryFindSpawnPosition(out Vector3 cheesePosition) == false)
        {
            return;
        }

        if (TryFindTrapPosition(cheesePosition, out Vector3 trapPosition) == false)
        {
            return;
        }

        ExpiringCheese expiring = new ExpiringCheese();
        expiring.Cheese = SpawnFood(_cheesePool, cheesePosition);
        expiring.ExpireTime = Time.time + _spawnData.CheeseLifetime;
        _expiringCheeses.Add(expiring);
        SpawnFood(_trapPool, trapPosition);
    }

    /// <summary>
    /// NavMesh 전체에서 최소 간격 조건을 만족하는 위치를 최대 시도 횟수만큼 찾는다
    /// </summary>
    private bool TryFindSpawnPosition(out Vector3 position)
    {
        for (int i = 0; i < _spawnData.MaxPlacementAttempts; i++)
        {
            if (_sampler.TryGetRandomPoint(out Vector3 navPoint) == false)
            {
                continue;
            }

            Vector3 candidate = ToSpawnPosition(navPoint);
            if (IsValidSpawnPosition(candidate))
            {
                position = candidate;
                return true;
            }
        }

        position = default;
        return false;
    }

    /// <summary>
    /// 치즈 주변 링 안에서 최소 간격 조건을 만족하는 쥐덫 위치를 최대 시도 횟수만큼 찾는다
    /// </summary>
    private bool TryFindTrapPosition(Vector3 cheesePosition, out Vector3 position)
    {
        float minDistance = _spawnData.TrapMinDistanceFromCheese;
        float maxDistance = _spawnData.TrapMaxDistanceFromCheese;
        for (int i = 0; i < _spawnData.MaxPlacementAttempts; i++)
        {
            if (_sampler.TryGetPointAround(cheesePosition, minDistance, maxDistance, out Vector3 navPoint) == false)
            {
                continue;
            }

            Vector3 candidate = ToSpawnPosition(navPoint);
            float sqrDistanceToCheese = (candidate - cheesePosition).sqrMagnitude;
            bool isInRing = sqrDistanceToCheese >= minDistance * minDistance
                            && sqrDistanceToCheese <= maxDistance * maxDistance;
            if (isInRing && IsValidSpawnPosition(candidate))
            {
                position = candidate;
                return true;
            }
        }

        position = default;
        return false;
    }

    /// <summary>
    /// 플레이어와 이미 생성된 모든 오브젝트로부터 최소 거리 밖인지 제곱 거리로 확인한다
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        float minPlayerDistance = _spawnData.MinDistanceFromPlayer;
        Vector3 toPlayer = position - _player.transform.position;
        if (toPlayer.sqrMagnitude < minPlayerDistance * minPlayerDistance)
        {
            return false;
        }

        float sqrMinObjectDistance = _spawnData.MinObjectDistance * _spawnData.MinObjectDistance;
        foreach (Food food in _activeFoods.Keys)
        {
            Vector3 offset = food.transform.position - position;
            if (offset.sqrMagnitude < sqrMinObjectDistance)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// NavMesh 표면 지점을 트리거가 플레이어 캡슐과 겹치도록 위로 띄운 배치 좌표로 바꾼다
    /// </summary>
    private Vector3 ToSpawnPosition(Vector3 navPoint)
    {
        return navPoint + Vector3.up * _spawnData.SpawnHeightOffset;
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내 배치하고 소비 이벤트를 구독한다
    /// </summary>
    private Food SpawnFood(FoodPool pool, Vector3 position)
    {
        Food food = pool.Get(position);
        food.OnConsumed += HandleFoodConsumed;
        _activeFoods.Add(food, pool);
        return food;
    }

    /// <summary>
    /// 오브젝트의 구독을 해제하고 생성 때 꺼낸 풀로 돌려보낸다. 이미 회수된 오브젝트는 무시한다
    /// </summary>
    private void DespawnFood(Food food)
    {
        if (_activeFoods.TryGetValue(food, out FoodPool pool) == false)
        {
            return;
        }

        _activeFoods.Remove(food);
        food.OnConsumed -= HandleFoodConsumed;
        pool.Release(food);
    }

    /// <summary>
    /// 만료 시각이 지난 치즈를 회수한다. 동반 쥐덫은 치즈와 무관하게 남는다
    /// </summary>
    private void ExpireCheeses()
    {
        for (int i = _expiringCheeses.Count - 1; i >= 0; i--)
        {
            ExpiringCheese expiring = _expiringCheeses[i];
            if (Time.time >= expiring.ExpireTime)
            {
                _expiringCheeses.RemoveAt(i);
                DespawnFood(expiring.Cheese);
            }
        }
    }

    /// <summary>
    /// 소비된 치즈를 만료 추적 목록에서 뺀다
    /// </summary>
    private void RemoveExpiringCheese(Food cheese)
    {
        for (int i = 0; i < _expiringCheeses.Count; i++)
        {
            if (_expiringCheeses[i].Cheese == cheese)
            {
                _expiringCheeses.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// 플레이어에게 소비된 오브젝트를 회수한다. 치즈면 만료 추적도 함께 끝낸다
    /// </summary>
    private void HandleFoodConsumed(Food food)
    {
        RemoveExpiringCheese(food);
        DespawnFood(food);
    }
}
