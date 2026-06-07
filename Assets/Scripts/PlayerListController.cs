using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListController : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField classInput;
    [SerializeField] private Transform studentRowsContent;
    [SerializeField] private StudentRowItem studentRowPrefab;

    private readonly List<PlayerData> players = new();
    private int nextPlayerId = 1;

    public void AddPlayer()
    {
        string playerName = nameInput.text.Trim();
        string className = classInput.text.Trim();

        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(className))
            return;

        players.Add(new PlayerData
        {
            Id = nextPlayerId++,
            Name = playerName,
            ClassName = className
        });

        nameInput.text = "";
        classInput.text = "";

        RefreshTable();
    }

    private void DeletePlayer(PlayerData player)
    {
        players.Remove(player);
        RefreshTable();
    }

    private void RefreshTable()
    {
        foreach (Transform child in studentRowsContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count; i++)
        {
            StudentRowItem row = Instantiate(studentRowPrefab, studentRowsContent);
            row.Setup(players[i], i + 1, DeletePlayer);
        }
    }
}