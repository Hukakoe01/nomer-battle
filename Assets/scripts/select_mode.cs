using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class select_mode : MonoBehaviour
{
    [Header("������ ����")]
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
        SceneManager.LoadScene("StoryScene"); // �������� �� ���� �����
    }

    void OnEndlessModeClicked()
    {
        SceneManager.LoadScene("EndlessScene"); // �������� �� ���� �����
    }

    void OnHostGameClicked()
    {
        SceneManager.LoadScene("HostScene"); // �������� �� ���� �����
    }

    void OnJoinGameClicked()
    {
        SceneManager.LoadScene("JoinScene"); // �������� �� ���� �����
    }

    void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu"); // �������� �� ������ �����
    
    }
}
