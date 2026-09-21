using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 베이크된 NavMesh 위의 무작위 지점을 면적·높이 가중치로 뽑아 주는 클래스
/// </summary>
public class NavMeshRandomPointSampler
{
    private readonly ObjectSpawnData _spawnData;
    private readonly NavMeshQueryFilter _filter;
    private readonly Vector3[] _vertices;
    private readonly int[] _indices;
    private readonly int[] _triangleFirstIndices;
    private readonly float[] _cumulativeWeights;
    private readonly float _totalWeight;

    /// <summary>
    /// 현재 로드된 NavMesh의 삼각형을 한 번 읽어 높이 규칙을 반영한 가중치 누적 배열을 만든다. 이후 NavMesh가 바뀌어도 갱신하지 않는다
    /// </summary>
    public NavMeshRandomPointSampler(int agentTypeId, int areaMask, ObjectSpawnData spawnData)
    {
        _spawnData = spawnData;
        _filter = new NavMeshQueryFilter { agentTypeID = agentTypeId, areaMask = areaMask };

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        _vertices = triangulation.vertices;
        _indices = triangulation.indices;

        List<int> triangleFirstIndices = new List<int>();
        List<float> cumulativeWeights = new List<float>();
        float totalWeight = 0f;
        int triangleCount = _indices.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            if ((areaMask & (1 << triangulation.areas[i])) == 0)
            {
                continue;
            }

            Vector3 a = _vertices[_indices[i * 3]];
            Vector3 b = _vertices[_indices[i * 3 + 1]];
            Vector3 c = _vertices[_indices[i * 3 + 2]];
            float centroidY = (a.y + b.y + c.y) / 3f;
            if (IsHeightAllowed(centroidY) == false)
            {
                continue;
            }

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            if (area <= 0f)
            {
                continue;
            }

            float heightMultiplier = centroidY >= _spawnData.ElevatedHeight ? _spawnData.ElevatedWeightMultiplier : 1f;
            totalWeight += area * heightMultiplier;
            triangleFirstIndices.Add(i * 3);
            cumulativeWeights.Add(totalWeight);
        }

        _triangleFirstIndices = triangleFirstIndices.ToArray();
        _cumulativeWeights = cumulativeWeights.ToArray();
        _totalWeight = totalWeight;
    }

    /// <summary>
    /// NavMesh 전체에서 가중치에 비례해 무작위 지점을 뽑아 NavMesh 위로 스냅한다. NavMesh가 없으면 false를 반환한다
    /// </summary>
    public bool TryGetRandomPoint(out Vector3 point)
    {
        if (_totalWeight <= 0f)
        {
            point = default;
            return false;
        }

        float target = Random.value * _totalWeight;
        int index = System.Array.BinarySearch(_cumulativeWeights, target);
        if (index < 0)
        {
            index = ~index;
        }

        index = Mathf.Min(index, _cumulativeWeights.Length - 1);

        int first = _triangleFirstIndices[index];
        Vector3 a = _vertices[_indices[first]];
        Vector3 b = _vertices[_indices[first + 1]];
        Vector3 c = _vertices[_indices[first + 2]];

        float u = Random.value;
        float v = Random.value;
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }

        Vector3 candidate = a + (b - a) * u + (c - a) * v;
        return SnapToNavMesh(candidate, out point);
    }

    /// <summary>
    /// 기준점을 중심으로 수평 거리 minDistance~maxDistance 링 안의 무작위 방향에서 NavMesh 위 지점을 찾는다
    /// </summary>
    public bool TryGetPointAround(Vector3 center, float minDistance, float maxDistance, out Vector3 point)
    {
        float distance = Random.Range(minDistance, maxDistance);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
        return SnapToNavMesh(candidate, out point);
    }

    /// <summary>
    /// 후보 지점을 가장 가까운 NavMesh 위 지점으로 옮긴다. 스냅 거리 안에 NavMesh가 없거나 제한 높이 구간이면 false를 반환한다
    /// </summary>
    private bool SnapToNavMesh(Vector3 candidate, out Vector3 point)
    {
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _spawnData.NavMeshSnapDistance, _filter)
            && IsHeightAllowed(hit.position.y))
        {
            point = hit.position;
            return true;
        }

        point = default;
        return false;
    }

    /// <summary>
    /// 월드 Y 높이가 생성을 제한하는 구간 밖인지 확인한다. 제한이 꺼져 있으면 항상 true다
    /// </summary>
    private bool IsHeightAllowed(float y)
    {
        if (_spawnData.RestrictHeight == false)
        {
            return true;
        }

        return y < _spawnData.RestrictedMinHeight || y > _spawnData.RestrictedMaxHeight;
    }
}
