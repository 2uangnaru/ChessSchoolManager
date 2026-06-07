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

    private readonly List<PlayerData> players = new();
    private int nextPlayerId = 1;

    public void AddPlayer()
    {
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
            ClassName = className
        };

        players.Add(player);

        StudentRowItem row = Instantiate(studentRowPrefab, studentRowsContent);
        row.Setup(player, players.Count);

        nameInput.text = "";
        classInput.text = "";

        Debug.Log($"Đã thêm học sinh: {player.Name} - {player.ClassName}");
    }
}