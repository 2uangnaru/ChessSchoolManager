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
}