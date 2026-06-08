using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListController : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField classInput;

    [Header("Table")]
    [SerializeField] private Transform studentRowsContent;
    [SerializeField] private StudentRowItem studentRowPrefab;

    public void AddPlayer()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

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
            WhiteCount = 0,
            BlackCount = 0,
            HadBye = false,
            InitialElo = 1000,
            CurrentElo = 1000
        };

        tournament.Players.Add(player);

        nameInput.text = "";
        classInput.text = "";

        RefreshFromCurrentTournament();
        SaveLoadManager.SaveTournament(tournament);
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

        if (tournament == null)
            return;

        tournament.Players.Remove(player);

        RefreshFromCurrentTournament();
        SaveLoadManager.SaveTournament(tournament);
    }

    public void RefreshFromCurrentTournament()
    {
        ClearTable();

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
            return;

        for (int i = 0; i < tournament.Players.Count; i++)
        {
            StudentRowItem row =
                Instantiate(studentRowPrefab, studentRowsContent);

            row.Setup(tournament.Players[i], i + 1, DeletePlayer);
        }
    }

    private void ClearTable()
    {
        foreach (Transform child in studentRowsContent)
        {
            Destroy(child.gameObject);
        }
    }
}