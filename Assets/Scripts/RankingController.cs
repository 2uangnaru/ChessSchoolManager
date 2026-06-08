using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankingController : MonoBehaviour
{
    [SerializeField] private TMP_Text rankingTitleText;
    [SerializeField] private Transform rankingRowsContent;
    [SerializeField] private RankingRowItem rankingRowPrefab;

    private void OnEnable()
    {
        RefreshRanking();
    }

    public void RefreshRanking()
    {
        foreach (Transform child in rankingRowsContent)
        {
            Destroy(child.gameObject);
        }

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
        {
            rankingTitleText.text = "CHƯA CÓ GIẢI ĐẤU";
            return;
        }

        rankingTitleText.text =
            $"BẢNG XẾP HẠNG SAU VÁN {tournament.CurrentRound} / {tournament.TotalRounds}";

        List<PlayerData> sortedPlayers = tournament.Players
            .OrderByDescending(p => p.Score)
            .ThenByDescending(p => CalculateBuchholz(p, tournament))
            .ThenByDescending(p => CalculateWinCount(p, tournament))
            .ThenByDescending(p => p.CurrentElo)
            .ThenBy(p => p.Name)
            .ToList();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            RankingRowItem row =
                Instantiate(rankingRowPrefab, rankingRowsContent);

            row.Setup(i + 1, sortedPlayers[i]);
        }
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