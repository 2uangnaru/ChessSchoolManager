using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using System.Linq;

public static class XlsxExportManager
{
    public static void ExportCurrentPairing()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Rounds.Count == 0)
        {
            Debug.LogWarning("Chưa có dữ liệu bốc thăm để xuất.");
            return;
        }

        RoundData round = tournament.Rounds[tournament.Rounds.Count - 1];

        List<List<string>> rows = new();

        rows.Add(new List<string> { tournament.TournamentName });
        rows.Add(new List<string> { $"Ván {round.RoundNumber} / {tournament.TotalRounds}" });
        rows.Add(new List<string>());
        rows.Add(new List<string> { "Bàn", "Trắng", "Đen", "Kết quả" });

        foreach (MatchData match in round.Matches)
        {
            PlayerData white = FindPlayer(tournament, match.WhitePlayerId);
            PlayerData black = FindPlayer(tournament, match.BlackPlayerId);

            rows.Add(new List<string>
            {
                match.BoardNumber.ToString(),
                white != null ? white.Name : "",
                match.IsBye ? "Miễn đấu" : (black != null ? black.Name : ""),
                GetResultText(match)
            });
        }

        string desktopPath =
    System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.Desktop
    );

        string folder = Path.Combine(
            desktopPath,
            "ChessTournamentExports"
        );
        Directory.CreateDirectory(folder);

        string fileName = $"{SafeFileName(tournament.TournamentName)}_Van_{round.RoundNumber}_BocTham.xlsx";
        string path = Path.Combine(folder, fileName);

        WriteSimpleXlsx(path, rows);

        Debug.Log($"Đã xuất bốc thăm: {path}");
        System.Diagnostics.Process.Start(
            "explorer.exe",
            folder
        );
    }

    private static PlayerData FindPlayer(TournamentData tournament, int id)
    {
        return tournament.Players.Find(p => p.Id == id);
    }

    private static string GetResultText(MatchData match)
    {
        if (match.IsBye)
            return "BYE";

        return match.Result switch
        {
            MatchResult.WhiteWin => "1-0",
            MatchResult.Draw => "1/2-1/2",
            MatchResult.BlackWin => "0-1",
            _ => "Chưa nhập"
        };
    }

    private static string SafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name;
    }

    private static void WriteSimpleXlsx(string filePath, List<List<string>> rows)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        using ZipArchive archive = ZipFile.Open(filePath, ZipArchiveMode.Create);

        AddEntry(archive, "[Content_Types].xml", GetContentTypesXml());
        AddEntry(archive, "_rels/.rels", GetRootRelsXml());
        AddEntry(archive, "xl/workbook.xml", GetWorkbookXml());
        AddEntry(archive, "xl/_rels/workbook.xml.rels", GetWorkbookRelsXml());
        AddEntry(archive, "xl/worksheets/sheet1.xml", GetSheetXml(rows));
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        ZipArchiveEntry entry = archive.CreateEntry(path);

        using StreamWriter writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(content);
    }

    private static string GetContentTypesXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
<Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
<Default Extension=""xml"" ContentType=""application/xml""/>
<Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
<Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
</Types>";
    }

    private static string GetRootRelsXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>";
    }

    private static string GetWorkbookXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
<sheets>
<sheet name=""Boc tham"" sheetId=""1"" r:id=""rId1""/>
</sheets>
</workbook>";
    }

    private static string GetWorkbookRelsXml()
    {
        return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
<Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
</Relationships>";
    }

    private static string GetSheetXml(List<List<string>> rows)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
<sheetData>");

        for (int r = 0; r < rows.Count; r++)
        {
            int rowIndex = r + 1;
            sb.Append($"<row r=\"{rowIndex}\">");

            for (int c = 0; c < rows[r].Count; c++)
            {
                string cellRef = $"{GetColumnName(c)}{rowIndex}";
                string value = EscapeXml(rows[r][c]);

                sb.Append($"<c r=\"{cellRef}\" t=\"inlineStr\"><is><t>{value}</t></is></c>");
            }

            sb.Append("</row>");
        }

        sb.Append("</sheetData></worksheet>");

        return sb.ToString();
    }

    private static string GetColumnName(int index)
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

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }


    public static void ExportRanking()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Players.Count == 0)
        {
            Debug.LogWarning("Chưa có dữ liệu xếp hạng để xuất.");
            return;
        }

        List<PlayerData> sortedPlayers = tournament.Players
            .OrderByDescending(p => p.Score)
            .ThenByDescending(p => p.CurrentElo)
            .ThenBy(p => p.Name)
            .ToList();

        List<List<string>> rows = new();

        rows.Add(new List<string> { tournament.TournamentName });
        rows.Add(new List<string> { $"Bảng xếp hạng sau ván {tournament.CurrentRound} / {tournament.TotalRounds}" });
        rows.Add(new List<string>());
        rows.Add(new List<string> { "Hạng", "Họ tên", "Lớp", "Điểm", "Elo" });

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            PlayerData player = sortedPlayers[i];

            rows.Add(new List<string>
        {
            (i + 1).ToString(),
            player.Name,
            player.ClassName,
            player.Score.ToString("0.0"),
            player.CurrentElo.ToString()
        });
        }

        string desktopPath =
            System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Desktop
            );

        string folder = Path.Combine(
            desktopPath,
            "ChessTournamentExports"
        );

        Directory.CreateDirectory(folder);

        string fileName =
            $"{SafeFileName(tournament.TournamentName)}_BangXepHang.xlsx";

        string path = Path.Combine(folder, fileName);

        WriteSimpleXlsx(path, rows);

        Debug.Log($"Đã xuất bảng xếp hạng: {path}");

        System.Diagnostics.Process.Start(
            "explorer.exe",
            folder
        );
    }


}