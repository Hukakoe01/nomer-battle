using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
    public void button_returns()
    {
        SceneManager.LoadScene(0);
    }
}
