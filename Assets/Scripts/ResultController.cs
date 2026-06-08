using TMPro;
using UnityEngine;

public class ResultController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private Transform resultRowsContent;
    [SerializeField] private ResultRowItem resultRowPrefab;

    private ResultRowItem selectedRow;
    private MatchData selectedMatch;

    private void OnEnable()
    {
        RefreshResultPanel();
    }

    public void RefreshResultPanel()
    {
        selectedRow = null;
        selectedMatch = null;

        foreach (Transform child in resultRowsContent)
        {
            Destroy(child.gameObject);
        }

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Rounds.Count == 0)
        {
            resultTitleText.text = "CHƯA CÓ VÁN ĐẤU";
            return;
        }

        RoundData round = tournament.Rounds[tournament.Rounds.Count - 1];

        resultTitleText.text =
            $"KẾT QUẢ VÁN {round.RoundNumber} / {tournament.TotalRounds}";

        foreach (MatchData match in round.Matches)
        {
            PlayerData white = FindPlayer(match.WhitePlayerId);
            PlayerData black = FindPlayer(match.BlackPlayerId);

            ResultRowItem row = Instantiate(resultRowPrefab, resultRowsContent);

            row.Setup(
                match,
                white != null ? white.Name : "Không tìm thấy",
                black != null ? black.Name : "Không tìm thấy",
                OnRowSelected
            );
        }
    }

    private void OnRowSelected(ResultRowItem row, MatchData match)
    {
        if (selectedRow != null)
            selectedRow.SetSelected(false);

        selectedRow = row;
        selectedMatch = match;

        selectedRow.SetSelected(true);
    }

    public void SetWhiteWin()
    {
        SetResult(MatchResult.WhiteWin);
    }

    public void SetDraw()
    {
        SetResult(MatchResult.Draw);
    }

    public void SetBlackWin()
    {
        SetResult(MatchResult.BlackWin);
    }

    public void ClearResult()
    {
        SetResult(MatchResult.NotPlayed);
    }

    private void SetResult(MatchResult result)
    {
        if (selectedMatch == null)
        {
            Debug.LogWarning("Chưa chọn bàn đấu.");
            return;
        }

        selectedMatch.Result = result;
        selectedRow.RefreshResultText();

        SaveLoadManager.SaveTournament(
            TournamentManager.Instance.CurrentTournament
        );
    }

    private PlayerData FindPlayer(int playerId)
    {
        return TournamentManager.Instance
            .CurrentTournament
            .Players
            .Find(p => p.Id == playerId);
    }
}