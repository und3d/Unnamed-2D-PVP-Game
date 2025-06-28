using TMPro;
using UnityEngine;

public class ScoreboardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText, killsText, deathsText, assistsText;

    public void SetData(string playerName, int kills, int deaths, int assists)
    {
        nameText.text = playerName;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
        assistsText.text = assists.ToString();
        
        
    }
}
