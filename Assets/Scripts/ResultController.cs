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

    public void FinishRound()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Rounds.Count == 0)
        {
            Debug.LogWarning("Chưa có vòng đấu để chốt.");
            return;
        }

        RoundData round = tournament.Rounds[tournament.Rounds.Count - 1];

        if (round.IsFinished)
        {
            Debug.LogWarning("Vòng này đã được chốt rồi.");
            return;
        }

        foreach (MatchData match in round.Matches)
        {
            if (match.Result == MatchResult.NotPlayed)
            {
                Debug.LogWarning("Vẫn còn bàn chưa nhập kết quả.");
                return;
            }
        }

        foreach (MatchData match in round.Matches)
        {
            PlayerData white = FindPlayer(match.WhitePlayerId);
            PlayerData black = FindPlayer(match.BlackPlayerId);

            if (white == null || black == null)
                continue;

            switch (match.Result)
            {
                case MatchResult.WhiteWin:
                    white.Score += 1f;
                    UpdateElo(white, black, 1f, 0f);
                    break;

                case MatchResult.Draw:
                    white.Score += 0.5f;
                    black.Score += 0.5f;
                    UpdateElo(white, black, 0.5f, 0.5f);
                    break;

                case MatchResult.BlackWin:
                    black.Score += 1f;
                    UpdateElo(white, black, 0f, 1f);
                    break;
            }
        }

        round.IsFinished = true;
        tournament.CurrentRound++;

        SaveLoadManager.SaveTournament(tournament);

        Debug.Log($"Đã chốt ván {round.RoundNumber}. Sang ván {tournament.CurrentRound + 1}");
    }


    private void UpdateElo(PlayerData white, PlayerData black, float whiteResult, float blackResult)
    {
        const int K = 32;

        int oldWhiteElo = white.CurrentElo;
        int oldBlackElo = black.CurrentElo;

        float expectedWhite = 1f / (1f + Mathf.Pow(10f, (oldBlackElo - oldWhiteElo) / 400f));
        float expectedBlack = 1f / (1f + Mathf.Pow(10f, (oldWhiteElo - oldBlackElo) / 400f));

        white.CurrentElo = Mathf.RoundToInt(oldWhiteElo + K * (whiteResult - expectedWhite));
        black.CurrentElo = Mathf.RoundToInt(oldBlackElo + K * (blackResult - expectedBlack));
    }

}