using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 풀잎(Leaf) 오브젝트에 부착해 "비행을 시작할 수 있는 상호작용 지점"임을 표시하고,
/// 이 풀잎 개별의 쿨타임을 직접 관리하는 컴포넌트. Player는 트리거 접촉 시
/// 이 컴포넌트로 풀잎인지 판별하고(태그 대신 컴포넌트 식별 방식 — HungerController/IHungerObj
/// 선례와 동일, 1-5절 근거), IsAvailable로 이 풀잎을 지금 사용할 수 있는지 확인한다.
/// 쿨타임은 풀잎마다 개별로 흐른다 — 이 풀잎을 사용하면 이 풀잎만 쿨타임에 들어가고,
/// 다른 풀잎 오브젝트는 영향받지 않는다.
/// 이 컴포넌트가 부착된 오브젝트의 Collider는 Is Trigger = true로 설정해야 한다.
/// </summary>
public class LeafFlightTrigger : MonoBehaviour
{
    [SerializeField] private LeafFlightData _leafFlightData;
    [SerializeField] private GameObject _leaf;

    private float _cooldownTimeRemaining;

    /// <summary>
    /// 현재 이 풀잎으로 비행을 시작할 수 있는지 여부 (쿨타임 중이 아니면 true)
    /// </summary>
    public bool IsAvailable => _cooldownTimeRemaining <= 0f;

    /// <summary>
    /// 매 프레임 이 풀잎의 쿨타임을 시간 경과에 따라 감소시킨다
    /// </summary>
    private void Update()
    {
        if (_cooldownTimeRemaining > 0f)
        {
            _cooldownTimeRemaining -= Time.deltaTime;
        }
        else if (_cooldownTimeRemaining <= 0f && _leaf.activeInHierarchy == false)
        {
            _leaf.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player) && IsAvailable)
        {
            StartCooldown();
            player.StartFlight();
        }
    }

    /// <summary>
    /// Player가 이 풀잎으로 비행을 시작시킬 때 호출해, 이 풀잎을 쿨타임 상태로 전환한다
    /// </summary>
    private void StartCooldown()
    {
        _leaf.SetActive(false);
        _cooldownTimeRemaining = _leafFlightData.CooldownDuration;
    }
}