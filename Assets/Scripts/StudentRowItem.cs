using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudentRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private Button deleteButton;

    private PlayerData playerData;
    private Action<PlayerData> onDeleteClicked;

    public void Setup(PlayerData data, int index, Action<PlayerData> onDelete)
    {
        playerData = data;
        onDeleteClicked = onDelete;

        indexText.text = index.ToString("00");
        nameText.text = data.Name;
        classText.text = data.ClassName;

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() =>
        {
            onDeleteClicked?.Invoke(playerData);
        });
    }
}