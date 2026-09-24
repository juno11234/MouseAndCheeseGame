using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

/// <summary>
/// DataTableRow 목록을 대상 ScriptableObject 필드에 적용한다(에디터 전용)
/// </summary>
public static class DataTableAssetWriter
{
    /// <summary>
    /// tableName과 일치하는 행을 target의 SerializedProperty에 쓰고 저장한다
    /// </summary>
    public static void Apply(ScriptableObject target, List<DataTableRow> rows, string tableName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        for (int i = 0; i < rows.Count; i++)
        {
            DataTableRow row = rows[i];
            if (row.TableName != tableName)
            {
                continue;
            }

            string fieldName = ToBackingFieldName(row.FieldName);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                continue;
            }

            ApplyValue(property, row);
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    /// <summary>
    /// 프로퍼티 이름(PascalCase)을 직렬화 필드 이름(_camelCase)으로 바꾼다
    /// </summary>
    private static string ToBackingFieldName(string propertyName)
    {
        return "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
    }

    /// <summary>
    /// 프로퍼티 타입에 맞춰 문자열 값을 파싱해 대입한다
    /// </summary>
    private static void ApplyValue(SerializedProperty property, DataTableRow row)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Float:
                property.floatValue = float.Parse(row.Value, CultureInfo.InvariantCulture);
                break;
            case SerializedPropertyType.Integer:
                property.intValue = int.Parse(row.Value, CultureInfo.InvariantCulture);
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = bool.Parse(row.Value);
                break;
        }
    }
}
