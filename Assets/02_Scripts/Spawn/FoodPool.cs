using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 음식·장애물 프리팹 하나를 재사용하는 오브젝트 풀. UnityEngine.Pool.ObjectPool을 감싼다
/// </summary>
public class FoodPool
{
    private readonly Food _prefab;
    private readonly Transform _parent;
    private readonly int _maxSize;
    private readonly ObjectPool<Food> _pool;

    /// <summary>
    /// 지금 사용 중인(풀에서 꺼내 반환하지 않은) 오브젝트 수
    /// </summary>
    public int ActiveCount => _pool.CountActive;

    /// <summary>
    /// 프리팹과 부모를 받아 풀을 만든다. 동시에 쓸 최대 수를 보관 상한으로 둔다
    /// </summary>
    public FoodPool(Food prefab, Transform parent, int maxSize)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = maxSize;
        _pool = new ObjectPool<Food>(
            createFunc: CreateFood,
            actionOnRelease: DeactivateFood,
            actionOnDestroy: DestroyFood,
            collectionCheck: true,
            defaultCapacity: maxSize,
            maxSize: maxSize);
    }

    /// <summary>
    /// 상한만큼 비활성 인스턴스를 미리 만들어 풀에 보관한다
    /// </summary>
    public void Prewarm()
    {
        List<Food> created = new List<Food>(_maxSize);
        for (int i = 0; i < _maxSize; i++)
        {
            created.Add(_pool.Get());
        }

        for (int i = 0; i < created.Count; i++)
        {
            _pool.Release(created[i]);
        }
    }

    /// <summary>
    /// 풀에서 오브젝트를 꺼내 위치를 옮긴 뒤 활성화한다. 위치를 먼저 옮겨 재사용 순간에 이전 자리에서 트리거가 발생하지 않게 한다
    /// </summary>
    public Food Get(Vector3 position)
    {
        Food food = _pool.Get();
        food.transform.position = position;
        food.gameObject.SetActive(true);
        return food;
    }

    /// <summary>
    /// 오브젝트를 비활성화해 풀에 돌려보낸다
    /// </summary>
    public void Release(Food food)
    {
        _pool.Release(food);
    }

    /// <summary>
    /// 새 인스턴스를 부모 아래에 만들고 비활성 상태로 둔다
    /// </summary>
    private Food CreateFood()
    {
        Food food = Object.Instantiate(_prefab, _parent);
        food.gameObject.SetActive(false);
        return food;
    }

    /// <summary>
    /// 풀로 돌아온 오브젝트를 비활성화한다
    /// </summary>
    private void DeactivateFood(Food food)
    {
        food.gameObject.SetActive(false);
    }

    /// <summary>
    /// 보관 상한을 넘어 버려지는 오브젝트를 파괴한다
    /// </summary>
    private void DestroyFood(Food food)
    {
        Object.Destroy(food.gameObject);
    }
}
