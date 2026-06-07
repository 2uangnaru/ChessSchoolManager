using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject createTournamentPanel;
    [SerializeField] private GameObject openTournamentPanel;
    [SerializeField] private GameObject importTournamentPanel;
    [SerializeField] private GameObject guidePanel;

    private void Start()
    {
        ShowCreateTournament();
    }

    public void ShowCreateTournament()
    {
        ShowOnly(createTournamentPanel);
    }

    public void ShowOpenTournament()
    {
        ShowOnly(openTournamentPanel);
    }

    public void ShowImportTournament()
    {
        ShowOnly(importTournamentPanel);
    }

    public void ShowGuide()
    {
        ShowOnly(guidePanel);
    }

    private void ShowOnly(GameObject targetPanel)
    {
        createTournamentPanel.SetActive(targetPanel == createTournamentPanel);
        openTournamentPanel.SetActive(targetPanel == openTournamentPanel);
        importTournamentPanel.SetActive(targetPanel == importTournamentPanel);
        guidePanel.SetActive(targetPanel == guidePanel);
    }
}