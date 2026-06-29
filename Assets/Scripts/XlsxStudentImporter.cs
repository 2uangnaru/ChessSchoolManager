using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using UnityEngine;

public static class XlsxStudentImporter
{
    public static void ImportFromXlsx(string filePath)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
        {
            Debug.LogWarning("Chưa có giải đấu.");
            return;
        }

        if (tournament.Rounds.Count > 0)
        {
            Debug.LogWarning("Đã bốc thăm rồi, không thể import thêm học sinh.");
            return;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Không tìm thấy file Excel: {filePath}");
            return;
        }

        List<Dictionary<string, string>> rows = ReadFirstSheet(filePath);

        int nameCol = -1;
        int classCol = -1;
        int imported = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            Dictionary<string, string> row = rows[i];

            if (nameCol == -1 || classCol == -1)
            {
                foreach (var cell in row)
                {
                    string value = Normalize(cell.Value);

                    int colIndex = ColumnNameToIndex(cell.Key);

                    if (value == "hoten" || value == "hovaten" || value == "name")
                        nameCol = colIndex;

                    if (value == "lop" || value == "class")
                        classCol = colIndex;
                }

                continue;
            }

            string studentName = GetCell(row, nameCol).Trim();
            string className = GetCell(row, classCol).Trim();

            if (string.IsNullOrEmpty(studentName) || string.IsNullOrEmpty(className))
                continue;

            tournament.Players.Add(new PlayerData
            {
                Id = GetNextPlayerId(tournament),
                Name = studentName,
                ClassName = className,
                Score = 0,
                Buchholz = 0,
                Wins = 0,
                WhiteCount = 0,
                BlackCount = 0,
                HadBye = false,
                InitialElo = 1000,
                CurrentElo = 1000
            });

            imported++;
        }

        SaveLoadManager.SaveTournament(tournament);

        Debug.Log($"Đã import {imported} học sinh từ Excel.");
    }

    private static List<Dictionary<string, string>> ReadFirstSheet(string filePath)
    {
        List<string> sharedStrings = new();
        List<Dictionary<string, string>> rows = new();

        using ZipArchive archive = ZipFile.OpenRead(filePath);

        ZipArchiveEntry sharedEntry = archive.GetEntry("xl/sharedStrings.xml");

        if (sharedEntry != null)
        {
            XmlDocument sharedDoc = LoadXml(sharedEntry);
            XmlNodeList nodes = sharedDoc.GetElementsByTagName("t");

            foreach (XmlNode node in nodes)
                sharedStrings.Add(node.InnerText);
        }

        ZipArchiveEntry sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");

        if (sheetEntry == null)
        {
            Debug.LogWarning("Không tìm thấy sheet1 trong file Excel.");
            return rows;
        }

        XmlDocument sheetDoc = LoadXml(sheetEntry);
        XmlNodeList rowNodes = sheetDoc.GetElementsByTagName("row");

        foreach (XmlNode rowNode in rowNodes)
        {
            Dictionary<string, string> row = new();

            foreach (XmlNode cellNode in rowNode.ChildNodes)
            {
                if (cellNode.Name != "c")
                    continue;

                string cellRef = cellNode.Attributes["r"]?.Value;
                string cellType = cellNode.Attributes["t"]?.Value;

                if (string.IsNullOrEmpty(cellRef))
                    continue;

                string colName = GetColumnName(cellRef);
                string value = "";

                XmlNode valueNode = cellNode["v"];

                if (valueNode != null)
                {
                    value = valueNode.InnerText;

                    if (cellType == "s" && int.TryParse(value, out int sharedIndex))
                    {
                        if (sharedIndex >= 0 && sharedIndex < sharedStrings.Count)
                            value = sharedStrings[sharedIndex];
                    }
                }
                else if (cellType == "inlineStr")
                {
                    XmlNode textNode = cellNode.SelectSingleNode(".//*[local-name()='t']");

                    if (textNode != null)
                        value = textNode.InnerText;
                }

                row[colName] = value;
            }

            rows.Add(row);
        }

        return rows;
    }

    private static XmlDocument LoadXml(ZipArchiveEntry entry)
    {
        XmlDocument doc = new XmlDocument();

        using Stream stream = entry.Open();
        doc.Load(stream);

        return doc;
    }

    private static string GetColumnName(string cellRef)
    {
        string result = "";

        foreach (char c in cellRef)
        {
            if (char.IsLetter(c))
                result += c;
            else
                break;
        }

        return result;
    }

    private static int ColumnNameToIndex(string columnName)
    {
        int result = 0;

        foreach (char c in columnName)
        {
            result *= 26;
            result += c - 'A' + 1;
        }

        return result - 1;
    }

    private static string GetCell(Dictionary<string, string> row, int columnIndex)
    {
        string columnName = IndexToColumnName(columnIndex);

        return row.TryGetValue(columnName, out string value)
            ? value
            : "";
    }

    private static string IndexToColumnName(int index)
    {
        index++;
        string name = "";

        while (index > 0)
        {
            int rem = (index - 1) % 26;
            name = (char)('A' + rem) + name;
            index = (index - 1) / 26;
        }

        return name;
    }

    private static string Normalize(string value)
    {
        return value
            .ToLower()
            .Replace(" ", "")
            .Replace("ọ", "o")
            .Replace("ỏ", "o")
            .Replace("õ", "o")
            .Replace("ó", "o")
            .Replace("ò", "o")
            .Replace("ộ", "o")
            .Replace("ố", "o")
            .Replace("ồ", "o")
            .Replace("ớ", "o")
            .Replace("ờ", "o")
            .Replace("ợ", "o")
            .Replace("ơ", "o")
            .Replace("ô", "o")
            .Replace("ắ", "a")
            .Replace("ằ", "a")
            .Replace("ặ", "a")
            .Replace("ă", "a")
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("ạ", "a")
            .Replace("ả", "a")
            .Replace("ã", "a")
            .Replace("ê", "e")
            .Replace("ế", "e")
            .Replace("ề", "e")
            .Replace("ệ", "e")
            .Replace("é", "e")
            .Replace("è", "e")
            .Replace("ẹ", "e")
            .Replace("í", "i")
            .Replace("ì", "i")
            .Replace("ị", "i")
            .Replace("ú", "u")
            .Replace("ù", "u")
            .Replace("ụ", "u")
            .Replace("ư", "u")
            .Replace("ứ", "u")
            .Replace("ừ", "u")
            .Replace("ự", "u")
            .Replace("ý", "y")
            .Replace("ỳ", "y")
            .Replace("đ", "d");
    }

    private static int GetNextPlayerId(TournamentData tournament)
    {
        int maxId = 0;

        foreach (PlayerData player in tournament.Players)
        {
            if (player.Id > maxId)
                maxId = player.Id;
        }

        return maxId + 1;
    }
}