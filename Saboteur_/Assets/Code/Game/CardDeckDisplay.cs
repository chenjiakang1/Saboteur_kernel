using UnityEngine;
using TMPro;

public class CardDeckDisplay : MonoBehaviour
{
    public TextMeshProUGUI deckText;

    public void UpdateText(int count)
    {
        if (deckText != null)
        {
            deckText.text = $"Remaining:{count}";
        }
    }
}
