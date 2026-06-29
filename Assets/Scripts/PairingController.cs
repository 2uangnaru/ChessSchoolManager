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
    [SerializeField] private Button gotoResultButton;

    [SerializeField] private TMP_Text gotoResultButtonText;
    [SerializeField] private MainMenuController mainMenuController;

    [SerializeField] private Button exportPairingButton;

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


    private void UpdateExportButtonState()
    {
        TournamentData tournament =
            TournamentManager.Instance.CurrentTournament;

        bool canExport =
            tournament != null &&
            tournament.Rounds.Count > 0;

        exportPairingButton.interactable = canExport;
    }
    private void UpdateResultButtonState()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
        {
            gotoResultButton.interactable = false;
            gotoResultButtonText.text = "CHUYỂN SANG NHẬP KẾT QUẢ";
            return;
        }

        bool tournamentFinished =
            tournament.CurrentRound >= tournament.TotalRounds;

        if (tournamentFinished)
        {
            gotoResultButton.interactable = true;
            gotoResultButtonText.text = "XEM BẢNG XẾP HẠNG";
            return;
        }

        int roundNumber = tournament.CurrentRound + 1;

        bool hasPairing =
            tournament.Rounds.Exists(r => r.RoundNumber == roundNumber);

        gotoResultButton.interactable = hasPairing;
        gotoResultButtonText.text = "CHUYỂN SANG NHẬP KẾT QUẢ";
    }

    public void OnGotoResultOrRankingClicked()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        if (tournament == null)
            return;

        if (tournament.CurrentRound >= tournament.TotalRounds)
        {
            mainMenuController.ShowRanking();
        }
        else
        {
            mainMenuController.ShowResult();
        }
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

        if (tournament == null)
        {
            roundTitleText.text = "CHƯA CÓ GIẢI ĐẤU";
            totalPlayersText.text = "Tổng học sinh: 0";
            totalBoardsText.text = "Số bàn: 0";
            generatePairingButton.interactable = false;

            UpdatePaginationUI(0);
            UpdateResultButtonState();
            UpdateExportButtonState();
            return;
        }

        int nextRound = tournament.CurrentRound + 1;

        if (tournament.CurrentRound >= tournament.TotalRounds)
        {
            roundTitleText.text = "GIẢI ĐẤU ĐÃ KẾT THÚC";
            totalPlayersText.text = $"Tổng học sinh: {tournament.Players.Count}";
            totalBoardsText.text = "Số bàn: 0";

            generatePairingButton.interactable = false;
            UpdateResultButtonState();
            UpdateExportButtonState();
            return;
        }

        bool alreadyPaired =
            tournament.Rounds.Exists(r => r.RoundNumber == nextRound);

        bool enoughPlayers =
            tournament.Players.Count >= 2;

        bool tournamentFinished =
            tournament.CurrentRound >= tournament.TotalRounds;

        generatePairingButton.interactable =
            enoughPlayers &&
            !alreadyPaired &&
            !tournamentFinished;
        int totalPlayers = tournament.Players.Count;
        int totalBoards = totalPlayers / 2;

        roundTitleText.text = $"BỐC THĂM VÁN: {nextRound} / {tournament.TotalRounds}";
        totalPlayersText.text = $"Tổng học sinh: {totalPlayers}";
        totalBoardsText.text = $"Số bàn: {totalBoards}";

        UpdateExportButtonState();

        UpdateResultButtonState();


    }

    public void GeneratePairing()
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

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

        if (tournament.CurrentRound >= tournament.TotalRounds)
        {
            Debug.LogWarning("Giải đấu đã kết thúc, không thể bốc thăm thêm.");
            return;
        }

        int roundNumber = tournament.CurrentRound + 1;

        RoundData existingRound = tournament.Rounds.Find(r => r.RoundNumber == roundNumber);

        if (existingRound != null)
        {
            Debug.LogWarning($"Vòng {roundNumber} đã được bốc thăm.");
            return;
        }

        RoundData round = GenerateSwissRound(tournament, roundNumber);

        tournament.Rounds.Add(round);
        UpdateExportButtonState();

        UpdateResultButtonState();

        SaveLoadManager.SaveTournament(tournament);

        RefreshInfo();
        RefreshPairingTable();

        Debug.Log($"Đã tạo bốc thăm ván {roundNumber}");


    }

    public void RefreshPairingPanel()
    {
        RefreshInfo();
        RefreshPairingTable();
        UpdateResultButtonState();
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

            if (match.IsBye)
            {
                row.Setup(
                    match.BoardNumber,
                    white != null ? white.Name : "Không tìm thấy",
                    "Miễn đấu"
                );
            }
            else
            {
                row.Setup(
                    match.BoardNumber,
                    white != null ? white.Name : "Không tìm thấy",
                    black != null ? black.Name : "Không tìm thấy"
                );
            }
        }

        UpdatePaginationUI(totalItems);
    }


    private RoundData GenerateSwissRound(TournamentData tournament, int roundNumber)
    {
        List<PlayerData> players = new List<PlayerData>(tournament.Players);

        players.Sort((a, b) =>
        {
            int scoreCompare = b.Score.CompareTo(a.Score);
            if (scoreCompare != 0)
                return scoreCompare;

            return a.Id.CompareTo(b.Id);
        });

        RoundData round = new RoundData
        {
            RoundNumber = roundNumber,
            IsFinished = false
        };

        int boardNumber = 1;

        if (players.Count % 2 == 1)
        {
            PlayerData byePlayer = FindByePlayer(players);

            if (byePlayer != null)
            {
                players.Remove(byePlayer);

                round.Matches.Add(new MatchData
                {
                    BoardNumber = boardNumber,
                    WhitePlayerId = byePlayer.Id,
                    BlackPlayerId = -1,
                    Result = MatchResult.Bye,
                    IsBye = true
                });

                byePlayer.Score += 1f;
                byePlayer.HadBye = true;

                boardNumber++;
            }
        }

        List<PlayerData> unpaired = new List<PlayerData>(players);

        while (unpaired.Count >= 2)
        {
            PlayerData playerA = unpaired[0];
            unpaired.RemoveAt(0);

            PlayerData playerB = FindBestOpponent(playerA, unpaired);

            if (playerB == null)
            {
                playerB = unpaired[0];
            }

            unpaired.Remove(playerB);

            AssignColors(playerA, playerB, out PlayerData white, out PlayerData black);

            white.WhiteCount++;
            black.BlackCount++;

            round.Matches.Add(new MatchData
            {
                BoardNumber = boardNumber,
                WhitePlayerId = white.Id,
                BlackPlayerId = black.Id,
                Result = MatchResult.NotPlayed,
                IsBye = false
            });

            boardNumber++;
        }

        return round;
    }

    private PlayerData FindBestOpponent(PlayerData player, List<PlayerData> candidates)
    {
        PlayerData best = null;
        float bestScoreDiff = float.MaxValue;

        foreach (PlayerData candidate in candidates)
        {
            if (HavePlayedBefore(player.Id, candidate.Id))
                continue;

            float scoreDiff = Mathf.Abs(player.Score - candidate.Score);

            if (scoreDiff < bestScoreDiff)
            {
                bestScoreDiff = scoreDiff;
                best = candidate;
            }
        }

        return best;
    }

    private bool HavePlayedBefore(int playerAId, int playerBId)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        foreach (RoundData round in tournament.Rounds)
        {
            foreach (MatchData match in round.Matches)
            {
                if (match.IsBye)
                    continue;

                bool samePair =
                    (match.WhitePlayerId == playerAId && match.BlackPlayerId == playerBId) ||
                    (match.WhitePlayerId == playerBId && match.BlackPlayerId == playerAId);

                if (samePair)
                    return true;
            }
        }

        return false;
    }

    private PlayerData FindByePlayer(List<PlayerData> players)
    {
        List<PlayerData> candidates = new List<PlayerData>();

        foreach (PlayerData player in players)
        {
            if (!player.HadBye)
                candidates.Add(player);
        }

        if (candidates.Count == 0)
            candidates = players;

        candidates.Sort((a, b) =>
        {
            int scoreCompare = a.Score.CompareTo(b.Score);
            if (scoreCompare != 0)
                return scoreCompare;

            return a.Id.CompareTo(b.Id);
        });

        return candidates[0];
    }

    private void AssignColors(
        PlayerData playerA,
        PlayerData playerB,
        out PlayerData white,
        out PlayerData black)
    {
        int colorBalanceA = playerA.WhiteCount - playerA.BlackCount;
        int colorBalanceB = playerB.WhiteCount - playerB.BlackCount;

        if (colorBalanceA > colorBalanceB)
        {
            white = playerB;
            black = playerA;
        }
        else if (colorBalanceB > colorBalanceA)
        {
            white = playerA;
            black = playerB;
        }
        else
        {
            white = playerA.Id < playerB.Id ? playerA : playerB;
            black = white == playerA ? playerB : playerA;
        }
    }

    private PlayerData FindPlayer(int playerId)
    {
        TournamentData tournament = TournamentManager.Instance.CurrentTournament;

        return tournament.Players.Find(p => p.Id == playerId);
    }
}