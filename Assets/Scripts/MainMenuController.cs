using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject createTournamentPanel;
    [SerializeField] private GameObject openTournamentPanel;
    [SerializeField] private GameObject importTournamentPanel;
    [SerializeField] private GameObject pairingPanel;
    [SerializeField] private GameObject playerListPanel;
    [SerializeField] private PlayerListController playerListController;
    [SerializeField] private GameObject resultPanel;
    private void Start()
    {
        ShowCreateTournament();
    }

    public void ShowCreateTournament()
    {
        ShowOnly(createTournamentPanel);
    }

    public void ShowResult()
    {
        ShowOnly(resultPanel);
    }

    public void ShowPlayerList()
    {
        ShowOnly(playerListPanel);
    }
    public void ShowOpenTournament()
    {
        ShowOnly(openTournamentPanel);
    }

    public void ShowPairing()
    {
        ShowOnly(pairingPanel);
    }
    public void ShowImportTournament()
    {
        ShowOnly(importTournamentPanel);
    }


    private void ShowOnly(GameObject targetPanel)
    {
        createTournamentPanel.SetActive(targetPanel == createTournamentPanel);
        openTournamentPanel.SetActive(targetPanel == openTournamentPanel);
        importTournamentPanel.SetActive(targetPanel == importTournamentPanel);
        pairingPanel.SetActive(targetPanel == pairingPanel);
        playerListPanel.SetActive(targetPanel == playerListPanel);
        resultPanel.SetActive(targetPanel == resultPanel);
    }

    public void OpenSavedTournament()
    {
        TournamentData data = SaveLoadManager.LoadLatestTournament();

        if (data == null)
        {
            Debug.LogWarning("Không tìm thấy giải đang làm dở.");
            return;
        }

        TournamentManager.Instance.CurrentTournament = data;

        ShowPlayerList();

        playerListController.RefreshFromCurrentTournament();
    }

}