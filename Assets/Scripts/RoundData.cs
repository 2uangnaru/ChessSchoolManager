using System;
using System.Collections.Generic;

[Serializable]
public class RoundData
{
    public int RoundNumber;

    public bool IsFinished;

    public List<MatchData> Matches = new();
}