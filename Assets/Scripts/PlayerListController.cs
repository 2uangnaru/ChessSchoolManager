using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using SFB;
public class PlayerListController : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField classInput;

    [Header("Table")]
    [SerializeField] private Transform studentRowsContent;
    [SerializeField] private StudentRowItem studentRowPrefab;

    [Header("Pagination")]
    [SerializeField] private TMP_Text pageInfoText;

    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;


    private int currentPage = 1;
    private const int pageSize = 6;

    public void AddPlayer()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;



        if (tournament == null)
        {
            Debug.LogWarning("Chưa có giải đấu.");
            return;
        }

        if (tournament.Rounds.Count > 0)
        {
            Debug.LogWarning("Đã bốc thăm rồi, không thể thêm học sinh.");
            return;
        }


        if (tournament == null)
        {
            Debug.LogWarning("Chưa có giải đấu.");
            return;
        }

        string playerName = nameInput.text.Trim();
        string className = classInput.text.Trim();

        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(className))
        {
            Debug.LogWarning("Chưa nhập đủ họ tên/lớp.");
            return;
        }

        int nextId = GetNextPlayerId(tournament.Players);

        PlayerData player = new PlayerData
        {
            Id = nextId,
            Name = playerName,
            ClassName = className,
            Score = 0,
            Buchholz = 0,
            Wins = 0,
            WhiteCount = 0,
            BlackCount = 0,
            HadBye = false,
            InitialElo = 1000,
            CurrentElo = 1000
        };

        tournament.Players.Add(player);



        FindFirstObjectByType<MainMenuController>()
    ?.RefreshLeftMenuButtons();

        nameInput.text = "";
        classInput.text = "";

        RefreshFromCurrentTournament();
        SaveLoadManager.SaveTournament(tournament);
    }

    public void ImportExcel()
    {
        Debug.Log("IMPORT CLICK");

        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Chọn file Excel",
            "",
            "xlsx",
            false
        );

        Debug.Log($"Selected count: {paths.Length}");

        if (paths.Length == 0)
            return;

        string filePath = paths[0];

        Debug.Log($"Selected file: {filePath}");

        XlsxStudentImporter.ImportFromXlsx(filePath);

        currentPage = 1;
        RefreshFromCurrentTournament();
        FindFirstObjectByType<MainMenuController>()
    ?.RefreshLeftMenuButtons();

        Debug.Log("IMPORT DONE");
    }

    private int GetNextPlayerId(List<PlayerData> players)
    {
        int maxId = 0;

        foreach (PlayerData player in players)
        {
            if (player.Id > maxId)
                maxId = player.Id;
        }

        return maxId + 1;
    }

    private void DeletePlayer(PlayerData player)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament.Rounds.Count > 0)
        {
            Debug.LogWarning("Đã bốc thăm rồi, không thể xóa học sinh.");
            return;
        }

        if (tournament == null)
            return;

        tournament.Players.Remove(player);

        FindFirstObjectByType<MainMenuController>()
            ?.RefreshLeftMenuButtons();

        RefreshFromCurrentTournament();
        SaveLoadManager.SaveTournament(tournament);
    }

    public void RefreshFromCurrentTournament()
    {
        ClearTable();

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
        {
            UpdatePaginationUI(0);
            return;
        }

        int totalPlayers = tournament.Players.Count;
        int totalPages = GetTotalPages(totalPlayers);

        if (currentPage > totalPages)
            currentPage = totalPages;

        if (currentPage < 1)
            currentPage = 1;

        int startIndex = (currentPage - 1) * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, totalPlayers);

        for (int i = startIndex; i < endIndex; i++)
        {
            StudentRowItem row =
                Instantiate(studentRowPrefab, studentRowsContent);

            row.Setup(tournament.Players[i], i + 1, DeletePlayer);
        }

        UpdatePaginationUI(totalPlayers);
    }

    private void UpdatePaginationUI(int totalItems)
    {
        int totalPages = GetTotalPages(totalItems);

        pageInfoText.text = $"Trang {currentPage} / {totalPages}";

        previousPageButton.interactable = currentPage > 1;
        nextPageButton.interactable = currentPage < totalPages;
    }

    public void PreviousPage()
    {
        if (currentPage <= 1)
            return;

        currentPage--;
        RefreshFromCurrentTournament();
    }

    public void NextPage()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
            return;

        int totalPages = GetTotalPages(tournament.Players.Count);

        if (currentPage >= totalPages)
            return;

        currentPage++;
        RefreshFromCurrentTournament();
    }

    private int GetTotalPages(int totalItems)
    {
        if (totalItems <= 0)
            return 1;

        return Mathf.CeilToInt(totalItems / (float)pageSize);
    }


    private void ClearTable()
    {
        foreach (Transform child in studentRowsContent)
        {
            Destroy(child.gameObject);
        }
    }
}