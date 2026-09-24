using System.Collections.Generic;

/// <summary>
/// 원시 셀 격자를 DataTableRow 목록으로 변환한다
/// </summary>
public class DataTableRowParser
{
    private const int ColumnTable = 0;
    private const int ColumnField = 1;
    private const int ColumnType = 2;
    private const int ColumnValue = 3;
    private const int ColumnDescription = 4;

    /// <summary>
    /// 첫 행을 헤더로 보고 건너뛴 뒤 나머지 행을 DataTableRow로 변환한다
    /// </summary>
    public List<DataTableRow> Parse(IReadOnlyList<string[]> rawGrid)
    {
        List<DataTableRow> rows = new List<DataTableRow>();
        for (int i = 1; i < rawGrid.Count; i++)
        {
            string[] cells = rawGrid[i];
            if (cells.Length <= ColumnValue)
            {
                continue;
            }

            DataTableRow row = new DataTableRow();
            row.TableName = cells[ColumnTable];
            row.FieldName = cells[ColumnField];
            row.ValueType = cells[ColumnType];
            row.Value = cells[ColumnValue];
            row.Description = cells.Length > ColumnDescription ? cells[ColumnDescription] : string.Empty;
            rows.Add(row);
        }

        return rows;
    }
}
