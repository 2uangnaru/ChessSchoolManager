using TMPro;
using UnityEngine;

public class CreateTournamentController : MonoBehaviour
{
    [SerializeField] private TMP_InputField tournamentNameInput;
    [SerializeField] private TMP_Dropdown roundDropdown;
    [SerializeField] private MainMenuController mainMenuController;

    public void CreateTournament()
    {
        string tournamentName = tournamentNameInput.text.Trim();

        if (string.IsNullOrEmpty(tournamentName))
        {
            Debug.LogWarning("Chưa nhập tên giải");
            return;
        }

        int totalRounds = int.Parse(roundDropdown.options[roundDropdown.value].text);

        TournamentData data = new TournamentData
        {
            TournamentName = tournamentName,
            TotalRounds = totalRounds,
            CurrentRound = 0
        };

        TournamentManager.Instance.CurrentTournament = data;

        SaveLoadManager.SaveTournament(data);

        Debug.Log($"Đã tạo giải: {data.TournamentName} | Số ván: {data.TotalRounds}");

        mainMenuController.ShowPlayerList();
    }
}