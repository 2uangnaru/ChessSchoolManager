using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private Transform resultRowsContent;
    [SerializeField] private ResultRowItem resultRowPrefab;
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private Button finishRoundButton;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageInfoText;
    [SerializeField] private Button whiteWinButton;
    [SerializeField] private Button drawButton;
    [SerializeField] private Button blackWinButton;
    [SerializeField] private Button clearResultButton;
    private int currentPage = 1;
    private const int pageSize = 6;

    private ResultRowItem selectedRow;
    private MatchData selectedMatch;

    private void OnEnable()
    {
        RefreshResultPanel();
    }

    public void PreviousPage()
    {
        if (currentPage <= 1) return;
        currentPage--;
        RefreshResultPanel();
    }

    public void NextPage()
    {
        currentPage++;
        RefreshResultPanel();
    }

    private int GetTotalPages(int totalItems)
    {
        if (totalItems <= 0) return 1;
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
    public void RefreshResultPanel()
    {
        selectedRow = null;
        selectedMatch = null;
        UpdateResultButtonsState();

        foreach (Transform child in resultRowsContent)
        {
            Destroy(child.gameObject);
        }

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Rounds.Count == 0)
        {
            resultTitleText.text = "CHƯA CÓ VÁN ĐẤU";
            UpdatePaginationUI(0);
            return;
        }

        RoundData round = tournament.Rounds[tournament.Rounds.Count - 1];

        resultTitleText.text =
            $"KẾT QUẢ VÁN {round.RoundNumber} / {tournament.TotalRounds}";

        int totalItems = round.Matches.Count;
        int totalPages = GetTotalPages(totalItems);

        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        int startIndex = (currentPage - 1) * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, totalItems);

        for (int i = startIndex; i < endIndex; i++)
        {
            MatchData match = round.Matches[i];

            PlayerData white = FindPlayer(match.WhitePlayerId);
            PlayerData black = FindPlayer(match.BlackPlayerId);

            ResultRowItem row = Instantiate(resultRowPrefab, resultRowsContent);

            if (match.IsBye)
            {
                row.Setup(
                    match,
                    white != null ? white.Name : "Không tìm thấy",
                    "Miễn đấu",
                    OnRowSelected
                );
            }
            else
            {
                row.Setup(
                    match,
                    white != null ? white.Name : "Không tìm thấy",
                    black != null ? black.Name : "Không tìm thấy",
                    OnRowSelected
                );
            }
        }

        UpdatePaginationUI(totalItems);

        UpdateFinishButtonState();
    }

    private void UpdateResultButtonsState()
    {
        bool hasSelection = selectedMatch != null;

        whiteWinButton.interactable = hasSelection;
        drawButton.interactable = hasSelection;
        blackWinButton.interactable = hasSelection;
        clearResultButton.interactable = hasSelection;
    }

    private void UpdateFinishButtonState()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null || tournament.Rounds.Count == 0)
        {
            finishRoundButton.interactable = false;
            return;
        }

        RoundData round = tournament.Rounds[tournament.Rounds.Count - 1];

        bool allDone = true;

        foreach (MatchData match in round.Matches)
        {
            if (match.IsBye)
                continue;

            if (match.Result == MatchResult.NotPlayed)
            {
                allDone = false;
                break;
            }
        }

        finishRoundButton.interactable = allDone && !round.IsFinished;
    }

    private void OnRowSelected(ResultRowItem row, MatchData match)
    {
        if (selectedRow != null)
            selectedRow.SetSelected(false);

        selectedRow = row;
        selectedMatch = match;

        selectedRow.SetSelected(true);
        UpdateResultButtonsState();
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
        if (selectedMatch == null || selectedRow == null)
        {
            Debug.LogWarning("Chưa chọn bàn đấu.");
            return;
        }

        selectedMatch.Result = result;

        // QUAN TRỌNG
        selectedRow.RefreshResultText();

        UpdateFinishButtonState();

        selectedRow.SetSelected(false);

        selectedRow = null;
        selectedMatch = null;

        UpdateResultButtonsState();

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
            if (match.IsBye)
                continue;

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

            if (!white.OpponentIds.Contains(black.Id))
                white.OpponentIds.Add(black.Id);

            if (!black.OpponentIds.Contains(white.Id))
                black.OpponentIds.Add(white.Id);

        }


        CalculateBuchholz(tournament);
        round.IsFinished = true;
        tournament.CurrentRound++;

        SaveLoadManager.SaveTournament(tournament);

        mainMenuController.ShowPairing();

        Debug.Log($"Đã chốt ván {round.RoundNumber}. Sang ván {tournament.CurrentRound + 1}");
    }


    private void CalculateBuchholz(TournamentData tournament)
    {
        foreach (PlayerData player in tournament.Players)
        {
            float buchholz = 0;

            foreach (int opponentId in player.OpponentIds)
            {
                PlayerData opponent =
                    tournament.Players.Find(
                        p => p.Id == opponentId);

                if (opponent != null)
                    buchholz += opponent.Score;
            }

            player.Buchholz = buchholz;
        }
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