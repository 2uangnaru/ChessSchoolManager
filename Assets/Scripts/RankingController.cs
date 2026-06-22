using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingController : MonoBehaviour
{
    [SerializeField] private TMP_Text rankingTitleText;
    [SerializeField] private Transform rankingRowsContent;
    [SerializeField] private RankingRowItem rankingRowPrefab;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageInfoText;
    [SerializeField] private Button exportRankingButton;

    private int currentPage = 1;
    private const int pageSize = 9;
    private void OnEnable()
    {
        RefreshRanking();
    }

    public void PreviousPage()
    {
        if (currentPage <= 1) return;
        currentPage--;
        RefreshRanking();
    }

    public void NextPage()
    {
        currentPage++;
        RefreshRanking();
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

    public void RefreshRanking()
    {
        foreach (Transform child in rankingRowsContent)
        {
            Destroy(child.gameObject);
        }

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (exportRankingButton != null)
        {
            exportRankingButton.interactable =
                tournament != null &&
                tournament.Rounds.Count > 0;
        }

        if (tournament == null)
        {
            rankingTitleText.text = "CHƯA CÓ GIẢI ĐẤU";
            UpdatePaginationUI(0);
            if (exportRankingButton != null)
                exportRankingButton.interactable = false;
            
            return;
        }

        rankingTitleText.text =
            $"BẢNG XẾP HẠNG SAU VÁN {tournament.CurrentRound} / {tournament.TotalRounds}";

        List<PlayerData> sortedPlayers = tournament.Players
            .OrderByDescending(p => p.Score)
            .ThenByDescending(p => p.Buchholz)
            .ThenByDescending(p => p.CurrentElo)
            .ThenByDescending(p => CalculateBuchholz(p, tournament))
            .ThenByDescending(p => CalculateWinCount(p, tournament))
            .ThenBy(p => p.Name)
            .ToList();

        int totalItems = sortedPlayers.Count;
        int totalPages = GetTotalPages(totalItems);

        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        int startIndex = (currentPage - 1) * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, totalItems);

        for (int i = startIndex; i < endIndex; i++)
        {
            RankingRowItem row =
                Instantiate(rankingRowPrefab, rankingRowsContent);

            row.Setup(i + 1, sortedPlayers[i]);
        }

        UpdatePaginationUI(totalItems);
    }

    private float CalculateBuchholz(PlayerData player, TournamentData tournament)
    {
        float total = 0f;

        foreach (RoundData round in tournament.Rounds)
        {
            foreach (MatchData match in round.Matches)
            {
                int opponentId = -1;

                if (match.WhitePlayerId == player.Id)
                    opponentId = match.BlackPlayerId;
                else if (match.BlackPlayerId == player.Id)
                    opponentId = match.WhitePlayerId;

                if (opponentId == -1)
                    continue;

                PlayerData opponent = tournament.Players.Find(p => p.Id == opponentId);

                if (opponent != null)
                    total += opponent.Score;
            }
        }

        return total;
    }

    private int CalculateWinCount(PlayerData player, TournamentData tournament)
    {
        int wins = 0;

        foreach (RoundData round in tournament.Rounds)
        {
            foreach (MatchData match in round.Matches)
            {
                if (match.WhitePlayerId == player.Id &&
                    match.Result == MatchResult.WhiteWin)
                {
                    wins++;
                }

                if (match.BlackPlayerId == player.Id &&
                    match.Result == MatchResult.BlackWin)
                {
                    wins++;
                }
            }
        }

        return wins;
    }

}