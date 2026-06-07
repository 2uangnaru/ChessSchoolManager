using TMPro;
using UnityEngine;

public class PairingRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text boardText;
    [SerializeField] private TMP_Text whitePlayerText;
    [SerializeField] private TMP_Text blackPlayerText;

    public void Setup(int boardNumber, string whitePlayerName, string blackPlayerName)
    {
        boardText.text = $"Bàn {boardNumber}";
        whitePlayerText.text = whitePlayerName;
        blackPlayerText.text = blackPlayerName;
    }
}