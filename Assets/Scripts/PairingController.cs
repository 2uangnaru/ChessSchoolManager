using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private TMP_Text pageInfoText;

    private int currentPage = 1;
    private const int pageSize = 6;

    private void OnEnable()
    {
        RefreshInfo();
        RefreshPairingTable();
    }

    public void PreviousPage()
    {
        if (currentPage <= 1) return;
        currentPage--;
        RefreshPairingTable();
    }

    public void NextPage()
    {
        currentPage++;
        RefreshPairingTable();
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

    private void RefreshInfo()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        int nextRound = tournament.CurrentRound + 1;

        if (tournament.CurrentRound >= tournament.TotalRounds)
        {
            roundTitleText.text = "GIẢI ĐẤU ĐÃ KẾT THÚC";
            totalPlayersText.text = $"Tổng học sinh: {tournament.Players.Count}";
            totalBoardsText.text = "Số bàn: 0";

            generatePairingButton.interactable = false;
            return;
        }

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

        if (tournament.CurrentRound >= tournament.TotalRounds)
        {
            Debug.LogWarning("Giải đấu đã kết thúc, không thể bốc thăm thêm.");
            return;
        }

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

    public void RefreshPairingPanel()
    {
        RefreshInfo();
        RefreshPairingTable();
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
        {
            UpdatePaginationUI(0);
            return;
        }
        RoundData latestRound = tournament.Rounds[tournament.Rounds.Count - 1];

        int totalItems = latestRound.Matches.Count;
        int totalPages = GetTotalPages(totalItems);

        currentPage = Mathf.Clamp(currentPage, 1, totalPages);

        int startIndex = (currentPage - 1) * pageSize;
        int endIndex = Mathf.Min(startIndex + pageSize, totalItems);

        for (int i = startIndex; i < endIndex; i++)
        {
            MatchData match = latestRound.Matches[i];

            PlayerData white = FindPlayer(match.WhitePlayerId);
            PlayerData black = FindPlayer(match.BlackPlayerId);

            PairingRowItem row = Instantiate(pairingRowPrefab, pairingRowsContent);

            row.Setup(
                match.BoardNumber,
                white != null ? white.Name : "Không tìm thấy",
                black != null ? black.Name : "Không tìm thấy"
            );
        }

        UpdatePaginationUI(totalItems);
    }




    private PlayerData FindPlayer(int playerId)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        return tournament.Players.Find(p => p.Id == playerId);
    }
}