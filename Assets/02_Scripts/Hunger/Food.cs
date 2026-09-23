using System;
using UnityEngine;

public class Food : MonoBehaviour, IHungerObj
{
    public HungerObjType HungerObjType => _type;
    public float Amount => _amount;
    [SerializeField] private float _amount;
    [SerializeField] private HungerObjType _type;

    /// <summary>
    /// 플레이어에게 소비되어 배고픔 변경이 적용된 직후 발생하는 이벤트(오브젝트 풀 반환 트리거)
    /// </summary>
    public event Action<Food> OnConsumed;

    /// <summary>
    /// 어떤 Food 인스턴스든 소비되면 함께 발생하는 정적 이벤트(점수 등 전역 구독자용)
    /// </summary>
    public static event Action<Food> OnAnyConsumed;

    /// <summary>
    /// 플레이어(HungerController)와 닿으면 배고픔 변경을 적용하고 소비 이벤트를 발생시킨다
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out HungerController hungerController))
        {
            hungerController.ChangeHunger(_amount);
            OnConsumed?.Invoke(this);
            OnAnyConsumed?.Invoke(this);
        }
    }
}