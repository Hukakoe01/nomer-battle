using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class select_mode : MonoBehaviour
{
    [Header("Кнопки меню")]
    public Button storyModeButton;
    public Button endlessModeButton;
    public Button hostGameButton;
    public Button joinGameButton;
    public Button backButton;
    public Button exitButton;

    void Start()
    {
        storyModeButton.onClick.AddListener(OnStoryModeClicked);
        endlessModeButton.onClick.AddListener(OnEndlessModeClicked);
        hostGameButton.onClick.AddListener(OnHostGameClicked);
        joinGameButton.onClick.AddListener(OnJoinGameClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    void OnStoryModeClicked()
    {
        SceneManager.LoadScene("StoryScene"); // Заменить на твою сцену
    }

    void OnEndlessModeClicked()
    {
        SceneManager.LoadScene("EndlessScene"); // Заменить на твою сцену
    }

    void OnHostGameClicked()
    {
        SceneManager.LoadScene("HostScene"); // Заменить на твою сцену
    }

    void OnJoinGameClicked()
    {
        SceneManager.LoadScene("JoinScene"); // Заменить на твою сцену
    }

    void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu"); // Заменить на нужную сцену
    
    }
}
