using System;

[Serializable]
public class PlayerData
{
    public int Id;
    public string Name;
    public string ClassName;

    public float Score;

    public int WhiteCount;
    public int BlackCount;

    public bool HadBye;

    public int InitialElo = 1000;
    public int CurrentElo = 1000;
}