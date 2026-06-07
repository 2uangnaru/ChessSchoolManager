using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SavedTournamentRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text tournamentNameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image background;
    private string tournamentName;
    private Action<string, SavedTournamentRowItem> onSelected;
    private Color normalColor =
    new Color(1f, 1f, 1f, 0.1f);

    private Color selectedColor =
        new Color(0.4f, 0.7f, 1f, 0.5f);

    public void Setup(
    string name,
    Action<string, SavedTournamentRowItem> onSelect)
    {
        tournamentName = name;
        onSelected = onSelect;

        tournamentNameText.text = name;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            onSelected?.Invoke(
                tournamentName,
                this
            );
        });
    }
    public void SetSelected(bool selected)
    {
        background.color =
            selected ? selectedColor : normalColor;
    }

}