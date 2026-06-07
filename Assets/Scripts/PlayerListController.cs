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

    private int nextPlayerId = 1;

    public void AddPlayer()
    {
        if (TournamentManager.Instance.CurrentTournament == null)
        {
            Debug.LogWarning("Chưa có giải đấu. Hãy tạo giải trước.");
            return;
        }

        string playerName = nameInput.text.Trim();
        string className = classInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Chưa nhập họ tên học sinh");
            return;
        }

        if (string.IsNullOrEmpty(className))
        {
            Debug.LogWarning("Chưa nhập lớp");
            return;
        }

        PlayerData player = new PlayerData
        {
            Id = nextPlayerId++,
            Name = playerName,
            ClassName = className,
            Score = 0,
            WhiteCount = 0,
            BlackCount = 0,
            HadBye = false
        };

        TournamentManager.Instance.CurrentTournament.Players.Add(player);

        nameInput.text = "";
        classInput.text = "";

        RefreshTable();

        SaveLoadManager.SaveTournament(TournamentManager.Instance.CurrentTournament);
    }

    private void DeletePlayer(PlayerData player)
    {
        TournamentManager.Instance.CurrentTournament.Players.Remove(player);
        RefreshTable();
        SaveLoadManager.SaveTournament(TournamentManager.Instance.CurrentTournament);
    }

    private void RefreshTable()
    {
        foreach (Transform child in studentRowsContent)
        {
            Destroy(child.gameObject);
        }

        List<PlayerData> players = TournamentManager.Instance.CurrentTournament.Players;

        for (int i = 0; i < players.Count; i++)
        {
            StudentRowItem row = Instantiate(studentRowPrefab, studentRowsContent);
            row.Setup(players[i], i + 1, DeletePlayer);
        }
    }

    public void RefreshFromCurrentTournament()
    {
        RefreshTable();
    }

}