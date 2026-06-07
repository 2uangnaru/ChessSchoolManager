using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    public static TournamentManager Instance { get; private set; }

    public TournamentData CurrentTournament;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}