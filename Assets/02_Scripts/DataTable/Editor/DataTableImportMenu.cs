using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 데이터 테이블 임포트 파이프라인(시트 다운로드 → 파싱 → 값 적용)을 실행하는 에디터 메뉴 진입점
/// </summary>
public static class DataTableImportMenu
{
    /// <summary>
    /// 값을 적용할 대상 SO 에셋 경로와 그 테이블명을 묶은 항목
    /// </summary>
    private struct ImportTarget
    {
        /// <summary>
        /// 대상 ScriptableObject 에셋의 경로
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// DataTableRow.TableName과 대조할 테이블 이름
        /// </summary>
        public string TableName;
    }

    /// <summary>
    /// 값을 적용할 5개 SO 에셋(SpeedEffectData 제외, 0-1절 근거)
    /// </summary>
    private static readonly ImportTarget[] ImportTargets = new ImportTarget[]
    {
        new ImportTarget { AssetPath = "Assets/05_Data/PlayerStatData.asset", TableName = "PlayerStatData" },
        new ImportTarget { AssetPath = "Assets/05_Data/CatStatData.asset", TableName = "CatStatData" },
        new ImportTarget { AssetPath = "Assets/05_Data/LeafFlightData.asset", TableName = "LeafFlightData" },
        new ImportTarget { AssetPath = "Assets/05_Data/ObjectSpawnData.asset", TableName = "ObjectSpawnData" },
        new ImportTarget { AssetPath = "Assets/05_Data/ScoreData.asset", TableName = "ScoreData" },
    };

    /// <summary>
    /// "MouseAndCheeseTable" 시트를 내려받아 5개 데이터 테이블 SO 에셋에 값을 적용한다
    /// </summary>
    [MenuItem("MouseAndCheese/Data Table/Import From Sheet")]
    public static void Import()
    {
        IReadOnlyList<string[]> rawGrid = DataTableFetcher.FetchGrid();
        if (rawGrid.Count == 0)
        {
            return;
        }

        DataTableRowParser parser = new DataTableRowParser();
        List<DataTableRow> rows = parser.Parse(rawGrid);

        for (int i = 0; i < ImportTargets.Length; i++)
        {
            ApplyToAsset(ImportTargets[i], rows);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"데이터 테이블 임포트 완료: {rows.Count}개 행을 {ImportTargets.Length}개 에셋에 적용했습니다.");
    }

    /// <summary>
    /// 대상 경로의 에셋을 로드해 DataTableAssetWriter.Apply로 값을 적용한다. 에셋을 찾지 못하면 경고를 남기고 건너뛴다
    /// </summary>
    private static void ApplyToAsset(ImportTarget importTarget, List<DataTableRow> rows)
    {
        ScriptableObject target = AssetDatabase.LoadAssetAtPath<ScriptableObject>(importTarget.AssetPath);
        if (target == null)
        {
            Debug.LogWarning($"데이터 테이블 대상 에셋을 찾을 수 없습니다: {importTarget.AssetPath}");
            return;
        }

        DataTableAssetWriter.Apply(target, rows, importTarget.TableName);
    }
}
