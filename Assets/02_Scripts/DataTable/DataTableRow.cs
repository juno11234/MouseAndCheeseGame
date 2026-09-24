/// <summary>
/// 시트에서 파싱한 데이터 테이블 한 행을 담는 클래스
/// </summary>
public class DataTableRow
{
    /// <summary>
    /// 값을 적용할 대상 ScriptableObject 클래스 이름
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// 값을 적용할 대상 프로퍼티 이름
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// 값의 타입을 나타내는 문자열("float", "int", "bool")
    /// </summary>
    public string ValueType { get; set; }

    /// <summary>
    /// 문자열 그대로의 값
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 기획자가 남기는 설명
    /// </summary>
    public string Description { get; set; }
}
