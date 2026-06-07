using TMPro;
using UnityEngine;

public class StudentRowItem : MonoBehaviour
{
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text actionText;

    public void Setup(PlayerData data, int index)
    {
        indexText.text = index.ToString("00");
        nameText.text = data.Name;
        classText.text = data.ClassName;
        actionText.text = "Xóa";
    }
}