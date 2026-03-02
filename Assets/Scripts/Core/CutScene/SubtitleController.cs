using TMPro;
using UnityEngine;



public class SubtitleController : MonoBehaviour
{

    public TextMeshProUGUI textDisplay;

    public string[] lines;

    public int currentLineIndex = -1;



    void Update()

    {

        if (textDisplay == null) return;

        if (currentLineIndex >= 0 && currentLineIndex < lines.Length)
        {
            textDisplay.text = lines[currentLineIndex];
        }
        else
        {
            textDisplay.text = "";
        }

    }

}