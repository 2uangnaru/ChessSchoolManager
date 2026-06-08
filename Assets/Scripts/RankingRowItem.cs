using TMPro;
using UnityEngine;

public class RankingRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text eloText;

    public void Setup(int rank, PlayerData player)
    {
        rankText.text = rank.ToString();
        nameText.text = player.Name;
        classText.text = player.ClassName;
        scoreText.text = player.Score.ToString("0.0");
        eloText.text = player.CurrentElo.ToString();
    }
}