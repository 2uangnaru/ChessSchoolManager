using System.Collections.Generic;
using UnityEngine;

public class OpenTournamentController : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SavedTournamentRowItem rowPrefab;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private PlayerListController playerListController;
    private SavedTournamentRowItem selectedRow;
    private string selectedTournament;

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        List<string> tournaments =
            SaveLoadManager.GetTournamentNames();

        foreach (string tournamentName in tournaments)
        {
            SavedTournamentRowItem row =
                Instantiate(rowPrefab, contentRoot);

            row.Setup(tournamentName, OnTournamentSelected);
        }
    }

    public void OpenSelectedTournament()
    {
        if (string.IsNullOrEmpty(selectedTournament))
        {
            Debug.LogWarning("Chưa chọn giải nào.");
            return;
        }

        TournamentData data = SaveLoadManager.LoadTournament(selectedTournament);

        if (data == null)
        {
            Debug.LogWarning($"Không mở được giải: {selectedTournament}");
            return;
        }

        TournamentManager.Instance.CurrentTournament = data;

        mainMenuController.ShowPlayerList();

        playerListController.RefreshFromCurrentTournament();

        Debug.Log($"Đã mở giải: {data.TournamentName}");
    }
    private void OnTournamentSelected(
        string tournamentName,
        SavedTournamentRowItem row)
    {
        selectedTournament = tournamentName;

        if (selectedRow != null)
            selectedRow.SetSelected(false);

        selectedRow = row;
        selectedRow.SetSelected(true);

        Debug.Log($"Đã chọn giải: {tournamentName}");
    }
}