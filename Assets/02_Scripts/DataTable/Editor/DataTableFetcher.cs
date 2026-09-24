using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 공개 공유된 "MouseAndCheeseTable" 시트를 TSV로 내려받아 원시 셀 격자로 변환한다(에디터 전용)
/// </summary>
public static class DataTableFetcher
{
    private const string SpreadsheetId = "1g5WEcxEHic73LiLZFZlbchHXNLCDCjxaHyoAk7KLXFs";
    private const int SheetGid = 0;

    /// <summary>
    /// DataTable 탭을 TSV로 내려받아 셀 격자로 반환한다. 실패하면 빈 격자를 반환한다
    /// </summary>
    public static IReadOnlyList<string[]> FetchGrid()
    {
        string url = $"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/export?format=tsv&gid={SheetGid}";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        // 에디터 메뉴 명령이라 코루틴 없이 완료를 기다린다
        while (operation.isDone == false)
        {
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"시트 다운로드 실패: {request.error}\n시트가 '링크가 있는 모든 사용자 - 보기 가능'으로 공유돼 있는지 확인하세요.");
            return Array.Empty<string[]>();
        }

        return ParseGrid(request.downloadHandler.text);
    }

    /// <summary>
    /// TSV 텍스트를 줄바꿈과 탭 기준으로 잘라 셀 격자로 만든다
    /// </summary>
    private static IReadOnlyList<string[]> ParseGrid(string tsvText)
    {
        string normalized = tsvText.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = normalized.Split('\n');

        List<string[]> grid = new List<string[]>();
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                continue;
            }

            grid.Add(lines[i].Split('\t'));
        }

        return grid;
    }
}
