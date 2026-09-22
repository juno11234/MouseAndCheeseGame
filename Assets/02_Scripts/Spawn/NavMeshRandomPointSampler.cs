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

        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation(); // NavMesh를 삼각형 목록으로
        _vertices = triangulation.vertices; // 모든 꼭짓점 좌표
        _indices = triangulation.indices; // 3개씩 끊으면 삼각형 단위 별 좌표

        List<int> triangleFirstIndices = new List<int>();
        List<float> cumulativeWeights = new List<float>();
        float totalWeight = 0f;
        int triangleCount = _indices.Length / 3; // 삼각형의 갯수
        for (int i = 0; i < triangleCount; i++)
        {
            if ((areaMask & (1 << triangulation.areas[i])) == 0) // 허용된 영역인가 (비트 연산)
            {
                continue;
            }

            // i번째 삼각형의 꼭짓점
            Vector3 a = _vertices[_indices[i * 3]];
            Vector3 b = _vertices[_indices[i * 3 + 1]];
            Vector3 c = _vertices[_indices[i * 3 + 2]];

            float centroidY = (a.y + b.y + c.y) / 3f; // 세 꼭짓점 높이의 평균
            if (IsHeightAllowed(centroidY) == false) // 생성 제한 구역인지 판단
            {
                continue;
            }

            // 두변을 외적한 벡터 길이 = 평행사변형의 넓이, 삼각형이니 * 0.5f
            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            if (area <= 0f)
            {
                continue;
            }

            float heightMultiplier;
            if (centroidY >= _spawnData.ElevatedHeight) // 평균이 높이 기준보다 높다면 확률 조정
            {
                heightMultiplier = _spawnData.ElevatedWeightMultiplier;
            }
            else heightMultiplier = 1f;

            totalWeight += area * heightMultiplier;
            triangleFirstIndices.Add(i * 3); // 이 삼각형이 몇번 부터인지
            cumulativeWeights.Add(totalWeight); // 가중치 합계
        }

        _triangleFirstIndices = triangleFirstIndices.ToArray(); // 필터를 통과한 삼각형
        _cumulativeWeights = cumulativeWeights.ToArray(); // 그삼각형의 가중치
        _totalWeight = totalWeight;
    }

    /// <summary>
    /// NavMesh 전체에서 가중치에 비례해 무작위 지점을 뽑아 NavMesh 위로 스냅한다. NavMesh가 없으면 false를 반환한다
    /// </summary>
    public bool TryGetRandomPoint(out Vector3 point)
    {
        if (_totalWeight <= 0f) // 하나도 필터를 통과하지 못했을 때
        {
            point = default;
            return false;
        }

        float target = Random.value * _totalWeight;
        int index = System.Array.BinarySearch(_cumulativeWeights, target); // 타겟과 같은 값이 있으면 그위치를 반환 없으면 음수 반환
        if (index < 0) // 음수는 target 보다 처음으로 큰 원소위치 위치를 비트 반전한 값 
        {
            index = ~index;
        }

        index = Mathf.Min(index, _cumulativeWeights.Length - 1);

        int first = _triangleFirstIndices[index]; // 꼭짓점 꺼내기
        Vector3 a = _vertices[_indices[first]];
        Vector3 b = _vertices[_indices[first + 1]];
        Vector3 c = _vertices[_indices[first + 2]];

        //  u + v <= 1 이어야 삼각형 1보다 크면 평행사변형을 기준으로 뒤집힌 삼각형
        float u = Random.value;
        float v = Random.value;
        if (u + v > 1f) //다시 뒤집어줌
        {
            u = 1f - u;
            v = 1f - v;
        }

        Vector3 candidate = a + (b - a) * u + (c - a) * v; // ab 방향으로 u 만큼 ac 방향으로 v 만큼 세 꼭짓점 표현식
        return SnapToNavMesh(candidate, out point);
    }

    /// <summary>
    /// 기준점을 중심으로 수평 거리 minDistance~maxDistance 링 안의 무작위 방향에서 NavMesh 위 지점을 찾는다
    /// </summary>
    public bool TryGetPointAround(Vector3 center, float minDistance, float maxDistance, out Vector3 point)
    {
        float distance = Random.Range(minDistance, maxDistance);
        float angle = Random.Range(0f, Mathf.PI * 2f); // 0~360도 사이 
        Vector3 candidate = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance; // 그 각도를 가리키는 길이 1방향 벡터3 (x,y,z) * 길이 
        return SnapToNavMesh(candidate, out point);
    }

    /// <summary>
    /// 후보 지점을 가장 가까운 NavMesh 위 지점으로 옮긴다. 스냅 거리 안에 NavMesh가 없거나 제한 높이 구간이면 false를 반환한다
    /// </summary>
    private bool SnapToNavMesh(Vector3 candidate, out Vector3 point)
    {
        // candidate 주변안에서 필터조건이 맞는 가장 가까운 NavMesh 지점 찾으면  true
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _spawnData.NavMeshSnapDistance, _filter) 
            && IsHeightAllowed(hit.position.y))
        {
            point = hit.position; // out으로 값 반환
            return true;
        }

        point = default;
        return false; //실패
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