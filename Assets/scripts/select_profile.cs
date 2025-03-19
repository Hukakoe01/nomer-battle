using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class select_profile : MonoBehaviour
{
    int tutorialStatus = -1;
    public void button_profile1()
    {
        tutorialStatus = GetTutorialStatus(1);
    }

    public void button_profile2()
    {
        tutorialStatus = GetTutorialStatus(2);
    }

    public void button_profile3()
    {
        tutorialStatus = GetTutorialStatus(3);
    }
    public void next_step()
    {
        if (tutorialStatus == -1) { }
        else if (tutorialStatus == 0)
        {
            // Переход на сцену с обучением
            SceneManager.LoadScene("TutorialScene");
        }
        else if (tutorialStatus == 1)
        {
            // Переход на другую сцену
            SceneManager.LoadScene("SampleScene");
        }
    }
    public void button_returns()
    {
        SceneManager.LoadScene(0);
    }
    private string dbPath;
    // Start is called before the first frame update
    void Start()
    {
        dbPath = "URI=file:D:/dip/nomer_battle/data.db";
    }


    private int GetTutorialStatus(int nom)
    {
        if (!File.Exists(dbPath))
        {
            Debug.LogError("База данных не найдена: " + dbPath);
            return -1; // Возвращаем -1, если база данных отсутствует
        }
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // Предположим, что у нас есть только один профиль
                cmd.CommandText = "SELECT tutorial FROM profiles WHERE ID = " + nom;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }
        }
        return -1;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
