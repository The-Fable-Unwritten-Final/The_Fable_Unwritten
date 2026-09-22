using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

public static class LocalizationTableExporter
{
    private const string ExportFolder = "Assets/LocalizationExports";

    [MenuItem("Tools/Localization/Export Selected Table to CSV")]
    private static void ExportSelectedTable()
    {
        StringTableCollection collection =
            Selection.activeObject as StringTableCollection;

        if (collection == null)
        {
            EditorUtility.DisplayDialog(
                "Localization Export",
                "Project 창에서 String Table Collection을 선택해주세요.",
                "확인"
            );

            return;
        }

        ExportCollection(collection);
    }

    private static void ExportCollection(StringTableCollection collection)
    {
        // Assets/LocalizationExports
        string exportPath = Path.Combine(
            Application.dataPath,
            "LocalizationExports"
        );

        Directory.CreateDirectory(exportPath);

        string fileName = SanitizeFileName(
            collection.TableCollectionName
        );

        string filePath = Path.Combine(
            exportPath,
            fileName + ".csv"
        );

        StringBuilder csv = new StringBuilder();

        // --------------------------------------------------
        // Header
        // --------------------------------------------------

        csv.Append("Key");

        foreach (var table in collection.StringTables)
        {
            csv.Append(",");
            csv.Append(EscapeCsv(table.LocaleIdentifier.Code));
        }

        csv.AppendLine();

        // --------------------------------------------------
        // Rows
        // --------------------------------------------------

        foreach (var row in collection.GetRowEnumerator())
        {
            csv.Append(
                EscapeCsv(row.KeyEntry.Key)
            );

            foreach (var tableEntry in row.TableEntries)
            {
                csv.Append(",");

                if (tableEntry != null)
                {
                    csv.Append(
                        EscapeCsv(tableEntry.Value)
                    );
                }
            }

            csv.AppendLine();
        }

        // UTF-8 BOM
        File.WriteAllText(
            filePath,
            csv.ToString(),
            new UTF8Encoding(true)
        );

        AssetDatabase.Refresh();

        // --------------------------------------------------
        // 완료 메시지
        // --------------------------------------------------

        bool openFolder = EditorUtility.DisplayDialog(
            "Localization Export Complete",
            "CSV 추출이 완료되었습니다.\n\n" +
            "저장 위치:\n" +
            filePath,
            "폴더 열기",
            "확인"
        );

        if (openFolder)
        {
            // Windows 탐색기에서 파일 선택
            EditorUtility.RevealInFinder(filePath);
        }
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        value = value.Replace("\"", "\"\"");

        if (value.Contains(",") ||
            value.Contains("\"") ||
            value.Contains("\n") ||
            value.Contains("\r"))
        {
            return $"\"{value}\"";
        }

        return value;
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        return fileName;
    }
}
