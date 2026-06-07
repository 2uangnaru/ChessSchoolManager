using System;
using System.Collections.Generic;

[Serializable]
public class TournamentData
{
    public string TournamentName;

    public int TotalRounds;

    // 0 = chưa bốc thăm vòng nào
    public int CurrentRound;

    public List<PlayerData> Players = new();
}