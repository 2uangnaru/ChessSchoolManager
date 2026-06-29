using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CreateTournamentController : MonoBehaviour
{
    [SerializeField] private TMP_InputField tournamentNameInput;
    [SerializeField] private TMP_Dropdown roundDropdown;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private Button createTournamentButton;

    public void CreateTournament()
    {
        string tournamentName = tournamentNameInput.text.Trim();

        if (string.IsNullOrEmpty(tournamentName))
        {
            Debug.LogWarning("Chưa nhập tên giải");
            return;
        }

        int totalRounds = int.Parse(
            roundDropdown.options[roundDropdown.value].text
        );

        TournamentData data = new TournamentData
        {
            TournamentName = tournamentName,
            TotalRounds = totalRounds,
            CurrentRound = 0
        };

        data.Players.Clear();
        data.Rounds.Clear();

        TournamentManager.Instance.CurrentTournament = data;

        mainMenuController.RefreshLeftMenuButtons();

        SaveLoadManager.SaveTournament(data);

        mainMenuController.ResetAllRuntimePanels();

        tournamentNameInput.text = "";

        mainMenuController.ShowPlayerList();

        Debug.Log($"Đã tạo giải mới: {data.TournamentName} | Số ván: {data.TotalRounds}");

        mainMenuController.RefreshLeftMenuButtons();
    }

    private void Start()
    {
        createTournamentButton.interactable = false;
        tournamentNameInput.onValueChanged.AddListener(OnNameChanged);
    }

    private void OnNameChanged(string value)
    {
        createTournamentButton.interactable =
            !string.IsNullOrWhiteSpace(value);
    }



}