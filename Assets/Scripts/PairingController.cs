using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PairingController : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text roundTitleText;
    [SerializeField] private TMP_Text totalPlayersText;
    [SerializeField] private TMP_Text totalBoardsText;
    [SerializeField] private UnityEngine.UI.Button generatePairingButton;
    [Header("Table")]
    [SerializeField] private Transform pairingRowsContent;
    [SerializeField] private PairingRowItem pairingRowPrefab;

    private void OnEnable()
    {
        RefreshInfo();
        RefreshPairingTable();
    }

    private void RefreshInfo()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
        {
            roundTitleText.text = "CHƯA CÓ GIẢI ĐẤU";
            totalPlayersText.text = "Tổng học sinh: 0";
            totalBoardsText.text = "Số bàn: 0";
            return;
        }

        int nextRound = tournament.CurrentRound + 1;
        bool alreadyPaired = tournament.Rounds.Exists(
        r => r.RoundNumber == nextRound);

        generatePairingButton.interactable = !alreadyPaired;
        int totalPlayers = tournament.Players.Count;
        int totalBoards = totalPlayers / 2;

        roundTitleText.text = $"BỐC THĂM VÁN: {nextRound} / {tournament.TotalRounds}";
        totalPlayersText.text = $"Tổng học sinh: {totalPlayers}";
        totalBoardsText.text = $"Số bàn: {totalBoards}";
    }

    public void GeneratePairing()
    {

        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        int roundNumber = tournament.CurrentRound + 1;

        RoundData existingRound =tournament.Rounds.Find(r =>
        r.RoundNumber == roundNumber);

        if (existingRound != null)
        {
            Debug.LogWarning(
                $"Vòng {roundNumber} đã được bốc thăm."
            );

            return;
        }
        if (tournament == null)
        {
            Debug.LogWarning("Chưa có giải đấu.");
            return;
        }

        if (tournament.Players.Count < 2)
        {
            Debug.LogWarning("Cần ít nhất 2 học sinh để bốc thăm.");
            return;
        }

        if (roundNumber > tournament.TotalRounds)
        {
            Debug.LogWarning("Giải đấu đã đủ số ván.");
            return;
        }

        List<PlayerData> shuffledPlayers = new List<PlayerData>(tournament.Players);

        for (int i = 0; i < shuffledPlayers.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledPlayers.Count);
            (shuffledPlayers[i], shuffledPlayers[randomIndex]) =
                (shuffledPlayers[randomIndex], shuffledPlayers[i]);
        }

        RoundData round = new RoundData
        {
            RoundNumber = roundNumber,
            IsFinished = false
        };

        int boardNumber = 1;

        for (int i = 0; i < shuffledPlayers.Count - 1; i += 2)
        {
            PlayerData white = shuffledPlayers[i];
            PlayerData black = shuffledPlayers[i + 1];

            MatchData match = new MatchData
            {
                BoardNumber = boardNumber,
                WhitePlayerId = white.Id,
                BlackPlayerId = black.Id,
                Result = MatchResult.NotPlayed
            };

            round.Matches.Add(match);
            boardNumber++;
        }

        tournament.Rounds.Add(round);

        SaveLoadManager.SaveTournament(tournament);

        RefreshInfo();
        RefreshPairingTable();

        Debug.Log($"Đã tạo bốc thăm ván {roundNumber}");
    }

    private void RefreshPairingTable()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        foreach (Transform child in pairingRowsContent)
        {
            if (child.name.Contains("PairingHeaderRow"))
                continue;

            Destroy(child.gameObject);
        }

        if (tournament == null || tournament.Rounds.Count == 0)
            return;

        RoundData latestRound = tournament.Rounds[tournament.Rounds.Count - 1];

        foreach (MatchData match in latestRound.Matches)
        {
            PlayerData white = FindPlayer(match.WhitePlayerId);
            PlayerData black = FindPlayer(match.BlackPlayerId);

            PairingRowItem row = Instantiate(pairingRowPrefab, pairingRowsContent);

            row.Setup(
                match.BoardNumber,
                white != null ? white.Name : "Không tìm thấy",
                black != null ? black.Name : "Không tìm thấy"
            );
        }
    }

    private PlayerData FindPlayer(int playerId)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        return tournament.Players.Find(p => p.Id == playerId);
    }
}