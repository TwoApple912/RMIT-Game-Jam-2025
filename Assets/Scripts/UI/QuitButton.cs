using TMPro;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public string newText = "There is no main menu.";
    [Space]
    public TextMeshProUGUI buttonText;

    public void QuitBUttonPressed()
    {
        buttonText.text = newText;
    }
}
