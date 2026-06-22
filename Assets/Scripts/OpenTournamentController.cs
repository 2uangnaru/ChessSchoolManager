using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class OpenTournamentController : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SavedTournamentRowItem rowPrefab;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private PlayerListController playerListController;
    [SerializeField] private Button openSelectedButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageInfoText;

    private int currentPage = 1;
    private const int pageSize = 10;
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

        int totalItems = tournaments.Count;
        int totalPages = GetTotalPages(totalItems);

        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        int startIndex = (currentPage - 1) * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, totalItems);

        for (int i = startIndex; i < endIndex; i++)
        {
            string tournamentName = tournaments[i];

            SavedTournamentRowItem row =
                Instantiate(rowPrefab, contentRoot);

            row.Setup(tournamentName, OnTournamentSelected);
        }

        UpdatePaginationUI(totalItems);

        selectedTournament = null;
        selectedRow = null;

        openSelectedButton.interactable = false;
    }


    public void PreviousPage()
    {
        if (currentPage <= 1)
            return;

        currentPage--;
        RefreshList();
    }

    public void NextPage()
    {
        currentPage++;
        RefreshList();
    }

    private int GetTotalPages(int totalItems)
    {
        if (totalItems <= 0)
            return 1;

        return Mathf.CeilToInt(totalItems / (float)pageSize);
    }

    private void UpdatePaginationUI(int totalItems)
    {
        int totalPages = GetTotalPages(totalItems);

        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        pageInfoText.text = $"Trang {currentPage} / {totalPages}";
        previousPageButton.interactable = currentPage > 1;
        nextPageButton.interactable = currentPage < totalPages;
    }

    public void DeleteSelectedTournament()
    {
        if (string.IsNullOrEmpty(selectedTournament))
        {
            Debug.LogWarning("Chưa chọn giải nào để xóa.");
            return;
        }

        SaveLoadManager.DeleteTournament(selectedTournament);

        selectedTournament = null;
        selectedRow = null;

        RefreshList();
        mainMenuController.RefreshLeftMenuButtons();
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

        mainMenuController.RefreshLeftMenuButtons();

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
        openSelectedButton.interactable = true;
    }
}