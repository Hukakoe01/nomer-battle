using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class main_menu : MonoBehaviour
{
    public GameObject Panel;
    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button_exite;

    private string dbPath;

    public void buton_play()
    {
        SceneManager.LoadScene(3);
    }
    public void buton_setting()
    {
        Panel.SetActive(true);
        Button1.gameObject.SetActive(true);
        Button2.gameObject.SetActive(true);
    }
    public void Close_setting()
    {
        Panel.SetActive(false);
        Button1.gameObject.SetActive(false);
        Button2.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
        string fileName = "data.db";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        dbPath = "URI=file:" + filePath;
    }
    public void button_exit()
    {
        Debug.Log("Игра закрывается...");
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
