using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameObject rankingPanel;
    [SerializeField] private PairingController pairingController;
    [SerializeField] private ResultController resultController;
    [SerializeField] private RankingController rankingController;
    [SerializeField] private Button pairingMenuButton;
    [SerializeField] private Button rankingMenuButton;


    private void Start()
    {
        TournamentManager.Instance.CurrentTournament = null;
        ShowCreateTournament();
        ShowCreateTournament();
        RefreshLeftMenuButtons();
    }

    public void ShowCreateTournament()
    {
        ShowOnly(createTournamentPanel);
    }

    public void ShowRanking()
    {
        ShowOnly(rankingPanel);
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

    public void RefreshLeftMenuButtons()
    {
        TournamentData tournament =
            TournamentManager.Instance.CurrentTournament;

        bool hasTournament =
            tournament != null;

        bool canOpenPairing =
            hasTournament &&
            tournament.Players.Count >= 2;

        pairingMenuButton.interactable =
            canOpenPairing;

        bool canOpenRanking =
            hasTournament &&
            tournament.Players.Count > 0;

        rankingMenuButton.interactable = canOpenRanking;
    }
    private void ShowOnly(GameObject targetPanel)
    {
        createTournamentPanel.SetActive(targetPanel == createTournamentPanel);
        openTournamentPanel.SetActive(targetPanel == openTournamentPanel);
        importTournamentPanel.SetActive(targetPanel == importTournamentPanel);
        pairingPanel.SetActive(targetPanel == pairingPanel);
        playerListPanel.SetActive(targetPanel == playerListPanel);
        resultPanel.SetActive(targetPanel == resultPanel);
        rankingPanel.SetActive(targetPanel == rankingPanel);



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

    public void ResetAllRuntimePanels()
    {
        playerListController?.RefreshFromCurrentTournament();
        resultController?.RefreshResultPanel();
        rankingController?.RefreshRanking();
        pairingController?.RefreshPairingPanel();
        rankingController?.RefreshRanking();
    }


}