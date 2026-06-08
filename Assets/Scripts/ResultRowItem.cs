using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text boardText;
    [SerializeField] private TMP_Text whitePlayerText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text blackPlayerText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image background;

    private MatchData matchData;
    private Action<ResultRowItem, MatchData> onSelected;

    private readonly Color normalColor = new Color(1f, 1f, 1f, 0.1f);
    private readonly Color selectedColor = new Color(0.4f, 0.7f, 1f, 0.5f);

    public void Setup(MatchData match, string whiteName, string blackName, Action<ResultRowItem, MatchData> onClick)
    {
        matchData = match;
        onSelected = onClick;

        boardText.text = $"Bàn {match.BoardNumber}";
        whitePlayerText.text = whiteName;
        blackPlayerText.text = blackName;

        RefreshResultText();

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            onSelected?.Invoke(this, matchData);
        });

        SetSelected(false);
    }

    public void RefreshResultText()
    {
        resultText.text = matchData.Result switch
        {
            MatchResult.WhiteWin => "1 - 0",
            MatchResult.Draw => "1/2 - 1/2",
            MatchResult.BlackWin => "0 - 1",
            _ => "Chưa nhập"
        };
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? selectedColor : normalColor;
    }
}