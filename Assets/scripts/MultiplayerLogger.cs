using TMPro;
using UnityEngine;

public class MultiplayerLogger : MonoBehaviour
{
    public static MultiplayerLogger Instance;

    public TMP_Text textArea;
    private string log = "";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Log(string message)
    {
        log += message + "\n";

        if (textArea != null)
        {
            textArea.text = log;
        }
    }
}