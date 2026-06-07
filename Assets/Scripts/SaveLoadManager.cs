using System.IO;
using UnityEngine;
using System.Collections.Generic;
public static class SaveLoadManager
{
    private const string SAVE_FILE_NAME = "current_tournament.json";
    private static string TournamentFolder =>
    Path.Combine(Application.persistentDataPath, "Tournaments");

    public static string LastTournamentName { get; private set; }
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    public static void SaveTournament(TournamentData data)
    {
        if (data == null)
            return;

        Directory.CreateDirectory(TournamentFolder);

        string filePath = Path.Combine(
            TournamentFolder,
            $"{data.TournamentName}.json"
        );

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(filePath, json);

        Debug.Log($"Đã lưu: {filePath}");

        LastTournamentName = data.TournamentName;

        var list = SaveLoadManager.GetTournamentNames();

        foreach (var item in list)
        {
            Debug.Log(item);
        }
    }

    public static TournamentData LoadLastTournament()
    {
        if (string.IsNullOrEmpty(LastTournamentName))
        {
            Debug.LogWarning("Chưa có giải vừa tạo trong phiên làm việc này.");
            return null;
        }

        return LoadTournament(LastTournamentName);
    }

    public static TournamentData LoadLatestTournament()
    {
        if (!Directory.Exists(TournamentFolder))
        {
            Debug.LogWarning("Chưa có thư mục Tournaments.");
            return null;
        }

        string[] files = Directory.GetFiles(TournamentFolder, "*.json");

        if (files.Length == 0)
        {
            Debug.LogWarning("Chưa có file giải nào.");
            return null;
        }

        string latestFile = files[0];

        foreach (string file in files)
        {
            if (File.GetLastWriteTime(file) > File.GetLastWriteTime(latestFile))
            {
                latestFile = file;
            }
        }

        string json = File.ReadAllText(latestFile);
        TournamentData data = JsonUtility.FromJson<TournamentData>(json);

        Debug.Log($"Đã mở giải mới nhất: {data.TournamentName}");

        return data;
    }
    public static TournamentData LoadTournament()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Chưa có file giải đã lưu.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        TournamentData data = JsonUtility.FromJson<TournamentData>(json);

        Debug.Log($"Đã mở giải: {data.TournamentName}");
        return data;
    }

    public static TournamentData LoadTournament(string tournamentName)
    {
        string filePath = Path.Combine(
            TournamentFolder,
            $"{tournamentName}.json"
        );

        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);

        return JsonUtility.FromJson<TournamentData>(json);
    }

    public static List<string> GetTournamentNames()
    {
        List<string> result = new();

        if (!Directory.Exists(TournamentFolder))
            return result;

        string[] files =
            Directory.GetFiles(TournamentFolder, "*.json");

        foreach (string file in files)
        {
            result.Add(
                Path.GetFileNameWithoutExtension(file)
            );
        }

        return result;


    }



    public static bool HasSavedTournament()
    {
        return File.Exists(SavePath);

    }
}